import { useEffect, useState } from 'react'
import { api, openSimulationStream } from '../api/client'
import type { SimulationEvent, SimulationStatus } from '../api/types'

const MAX_EVENTS = 50

// Consome o feed SSE (via fetch + ReadableStream, não EventSource nativo --
// ver client.ts) e complementa com polling leve de /api/simulation/status a
// cada 3s, porque o servidor só manda um snapshot de status na conexão, não
// a cada tick -- sem o polling, contadores como "pendingStepCount" ficam
// visivelmente parados entre eventos.
export function useSimulationStream(active: boolean) {
  const [status, setStatus] = useState<SimulationStatus | null>(null)
  const [events, setEvents] = useState<SimulationEvent[]>([])
  const [connected, setConnected] = useState(false)

  useEffect(() => {
    if (!active) return

    let cancelled = false
    const { response, abort } = openSimulationStream()

    const run = async () => {
      try {
        const res = await response
        if (!res.ok || !res.body) return
        setConnected(true)

        const reader = res.body.getReader()
        const decoder = new TextDecoder()
        let buffer = ''

        while (!cancelled) {
          const { done, value } = await reader.read()
          if (done) break
          buffer += decoder.decode(value, { stream: true })

          const parts = buffer.split('\n\n')
          buffer = parts.pop() ?? ''

          for (const part of parts) {
            const lines = part.split('\n')
            const dataLine = lines.find((l) => l.startsWith('data:'))
            if (!dataLine) continue

            const eventName = lines.find((l) => l.startsWith('event:'))?.slice(6).trim() ?? 'event'
            const payload = JSON.parse(dataLine.slice(5).trim())

            if (eventName === 'status') {
              setStatus(payload as SimulationStatus)
            } else {
              setEvents((prev) => [payload as SimulationEvent, ...prev].slice(0, MAX_EVENTS))
            }
          }
        }
      } catch {
        // Stream abortado (unmount) ou caiu -- silencioso, o polling de
        // status abaixo continua cobrindo o essencial.
      } finally {
        setConnected(false)
      }
    }

    run()

    return () => {
      cancelled = true
      abort()
    }
  }, [active])

  useEffect(() => {
    if (!active) return

    const poll = () => api.getSimulationStatus().then(setStatus).catch(() => {})
    poll()
    const timer = setInterval(poll, 3000)
    return () => clearInterval(timer)
  }, [active])

  return { status, events, connected }
}
