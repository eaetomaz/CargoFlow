import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { api } from '../api/client'
import { useSimulationStream } from '../hooks/useSimulationStream'
import type { Dashboard } from '../api/types'
import { statusLabel } from '../utils/statusLabel'
import {
  AlertTriangleIcon,
  CheckCircleIcon,
  DollarIcon,
  DropletIcon,
  FlagIcon,
  type IconProps,
  PackageIcon,
  PlayIcon,
  StopCircleIcon,
  TruckIcon,
  XCircleIcon,
} from '../components/icons'
import type { ComponentType } from 'react'

function currency(value: number) {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', maximumFractionDigits: 0 })
}

function shortDate(value: string) {
  const [, month, day] = value.split('-')
  return `${day}/${month}`
}

interface KpiCardProps {
  label: string
  value: string
  to: string
  danger?: boolean
}

function KpiCard({ label, value, to, danger }: KpiCardProps) {
  return (
    <Link to={to} className="card kpi-card">
      <span className="kpi-label">{label}</span>
      <span className="kpi-value" style={danger ? { color: 'var(--danger)' } : undefined}>
        {value}
      </span>
    </Link>
  )
}

const EVENT_ICON: Record<string, ComponentType<IconProps>> = {
  SimulationStarted: PlayIcon,
  TripStarted: TruckIcon,
  FuelingRegistered: DropletIcon,
  OccurrenceCreated: AlertTriangleIcon,
  OccurrenceResolved: CheckCircleIcon,
  DeliveryCompleted: PackageIcon,
  BillingGenerated: DollarIcon,
  SimulationFinished: FlagIcon,
  SimulationStopped: StopCircleIcon,
  SimulationStepFailed: XCircleIcon,
}

export default function DashboardPage() {
  const [dashboard, setDashboard] = useState<Dashboard | null>(null)
  const [loading, setLoading] = useState(true)
  const [speedMultiplier, setSpeedMultiplier] = useState(60)
  const [starting, setStarting] = useState(false)
  const [simError, setSimError] = useState<string | null>(null)
  const [streamActive, setStreamActive] = useState(false)

  const { status, events, connected } = useSimulationStream(streamActive)
  const previousStatusRef = useRef<string | null>(null)

  const loadDashboard = () => api.getDashboard().then(setDashboard)

  useEffect(() => {
    loadDashboard().finally(() => setLoading(false))

    // Se já tinha uma simulação rodando de uma sessão anterior (outra aba,
    // reload da página), reconecta ao feed em vez de esconder o painel.
    api
      .getSimulationStatus()
      .then((s) => {
        if (s.status === 'Running') setStreamActive(true)
      })
      .catch(() => {})
  }, [])

  // Atualiza o dashboard periodicamente enquanto a simulação mexe em dado
  // real, e mais uma vez assim que ela termina, pra refletir o estado final.
  useEffect(() => {
    if (status?.status === 'Running') {
      const timer = setInterval(loadDashboard, 5000)
      return () => clearInterval(timer)
    }

    if (previousStatusRef.current === 'Running') {
      loadDashboard()
    }
    previousStatusRef.current = status?.status ?? null
  }, [status?.status])

  const handleStart = async () => {
    setStarting(true)
    setSimError(null)
    try {
      await api.startSimulation(speedMultiplier)
      setStreamActive(true)
    } catch (err) {
      setSimError(err instanceof Error ? err.message : 'Erro ao iniciar simulação.')
    } finally {
      setStarting(false)
    }
  }

  const handleStop = async () => {
    try {
      await api.stopSimulation()
    } catch (err) {
      setSimError(err instanceof Error ? err.message : 'Erro ao parar simulação.')
    }
  }

  if (loading) return <p>Carregando...</p>
  if (!dashboard) return <p>Não foi possível carregar o dashboard.</p>

  const isRunning = status?.status === 'Running'

  return (
    <div>
      <h1>Dashboard</h1>
      <p style={{ color: 'var(--text-muted)', marginTop: -4 }}>Clique em qualquer indicador pra ver os registros por trás dele.</p>

      <div className="card simulation-panel" style={{ marginBottom: 20 }}>
        <div className="simulation-panel-header">
          <div>
            <h3 style={{ margin: 0 }}>Simulador de operação</h3>
            <p style={{ color: 'var(--text-muted)', fontSize: 13, margin: '4px 0 0' }}>
              Programa OTs "Criadas" em viagens reais e simula o ciclo completo (saída, abastecimento, ocorrência, entrega, faturamento) em relógio
              virtual acelerado -- tudo mutação real via os mesmos serviços da UI.
            </p>
          </div>

          {!isRunning ? (
            <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
              <select value={speedMultiplier} onChange={(e) => setSpeedMultiplier(Number(e.target.value))}>
                <option value={30}>30x</option>
                <option value={60}>60x</option>
                <option value={120}>120x</option>
                <option value={300}>300x</option>
              </select>
              <button className="btn btn-primary" disabled={starting} onClick={handleStart}>
                {starting ? 'Iniciando...' : 'Iniciar simulação'}
              </button>
            </div>
          ) : (
            <button className="btn btn-danger" onClick={handleStop}>
              Parar simulação
            </button>
          )}
        </div>

        {simError && <p className="form-error" style={{ marginTop: 8 }}>{simError}</p>}

        {status && status.status !== 'Idle' && (
          <div className="simulation-status-row">
            <span className={`badge ${isRunning ? 'badge-success' : 'badge-muted'}`}>{isRunning ? 'Rodando' : 'Concluída'}</span>
            <span>Relógio virtual: {status.currentVirtualTime ? new Date(status.currentVirtualTime).toLocaleString('pt-BR') : '--'}</span>
            <span>
              {status.completedTripCount}/{status.scheduledTripCount} viagens concluídas
            </span>
            <span>Velocidade: {status.speedMultiplier}x</span>
            {isRunning && <span style={{ color: connected ? 'var(--success)' : 'var(--warning)' }}>{connected ? '● ao vivo' : '○ reconectando...'}</span>}
          </div>
        )}

        {events.length > 0 && (
          <ul className="simulation-feed">
            {events.map((e, i) => {
              const EventIcon = EVENT_ICON[e.type]
              return (
                <li key={i}>
                  <span className="simulation-feed-time">{e.occurredAtVirtual}</span>
                  <span className="simulation-feed-icon">{EventIcon ? <EventIcon size={14} /> : <span className="simulation-feed-dot" />}</span>
                  <span>{e.message}</span>
                </li>
              )
            })}
          </ul>
        )}
      </div>

      <div className="kpi-grid" style={{ marginBottom: 20 }}>
        <KpiCard label="Viagens ativas" value={String(dashboard.activeTripsCount)} to="/viagens" />
        <KpiCard label="Entregas hoje" value={String(dashboard.deliveriesTodayCount)} to="/viagens" />
        <KpiCard label="Entregas atrasadas" value={String(dashboard.delayedTripsCount)} to="/viagens" danger={dashboard.delayedTripsCount > 0} />
        <KpiCard label="Faturamento" value={currency(dashboard.totalBilled)} to="/financeiro" />
        <KpiCard label="Receita (viagens concluídas)" value={currency(dashboard.totalRevenue)} to="/viagens" />
        <KpiCard
          label={`Margem${dashboard.aggregateMarginPercentage !== null ? ` (${dashboard.aggregateMarginPercentage}%)` : ''}`}
          value={currency(dashboard.aggregateMargin)}
          to="/financeiro"
          danger={dashboard.aggregateMargin < 0}
        />
        <KpiCard label="Veículos indisponíveis" value={String(dashboard.unavailableVehiclesCount)} to="/veiculos" danger={dashboard.unavailableVehiclesCount > 0} />
        <KpiCard label="Documentos a vencer" value={String(dashboard.documentsNearExpiryCount)} to="/documentos" danger={dashboard.documentsNearExpiryCount > 0} />
        <KpiCard label="Ocorrências abertas" value={String(dashboard.openOccurrencesCount)} to="/ocorrencias" danger={dashboard.openOccurrencesCount > 0} />
        <KpiCard label="Consumo médio da frota" value={dashboard.fleetAverageFuelEfficiencyKmL !== null ? `${dashboard.fleetAverageFuelEfficiencyKmL} km/l` : '—'} to="/abastecimentos" />
        <KpiCard label="Custo por km" value={dashboard.averageCostPerKm !== null ? currency(dashboard.averageCostPerKm) : '—'} to="/viagens" />
      </div>

      <div className="card" style={{ marginBottom: 20 }}>
        <h3 style={{ marginTop: 0 }}>Receita e entregas -- últimos 14 dias</h3>
        <ResponsiveContainer width="100%" height={220}>
          <AreaChart data={dashboard.revenueByDay}>
            <defs>
              <linearGradient id="revenueFill" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="var(--accent)" stopOpacity={0.35} />
                <stop offset="95%" stopColor="var(--accent)" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
            <XAxis dataKey="date" tickFormatter={shortDate} tick={{ fontSize: 12, fill: 'var(--text-muted)' }} />
            <YAxis tick={{ fontSize: 12, fill: 'var(--text-muted)' }} width={70} tickFormatter={(v) => currency(v)} />
            <Tooltip
              formatter={(value) => [currency(Number(value)), 'Receita']}
              labelFormatter={(label) => shortDate(String(label))}
              contentStyle={{ background: 'var(--surface)', border: '1px solid var(--border)', borderRadius: 8, fontSize: 13 }}
            />
            <Area type="monotone" dataKey="revenue" stroke="var(--accent)" fill="url(#revenueFill)" strokeWidth={2} />
          </AreaChart>
        </ResponsiveContainer>
      </div>

      <div className="dashboard-two-col">
        <div className="card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h3 style={{ margin: 0 }}>Documentos próximos do vencimento</h3>
            <Link to="/documentos">Ver todos</Link>
          </div>
          {dashboard.expiringDocuments.length === 0 ? (
            <p style={{ color: 'var(--text-muted)', fontSize: 13 }}>Nada vencendo -- tudo em dia.</p>
          ) : (
            <ul style={{ listStyle: 'none', margin: '12px 0 0', padding: 0, fontSize: 13 }}>
              {dashboard.expiringDocuments.map((d) => (
                <li key={d.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '6px 0', borderBottom: '1px solid var(--border)' }}>
                  <span>
                    {d.type} <span style={{ color: 'var(--text-muted)' }}>({d.ownerType})</span>
                  </span>
                  <span>
                    {d.expiryDate} <span className={`badge ${d.status === 'Vencido' ? 'badge-danger' : 'badge-warning'}`}>{statusLabel(d.status)}</span>
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h3 style={{ margin: 0 }}>Ocorrências abertas</h3>
            <Link to="/ocorrencias">Ver todas</Link>
          </div>
          {dashboard.openOccurrences.length === 0 ? (
            <p style={{ color: 'var(--text-muted)', fontSize: 13 }}>Nenhuma ocorrência aberta.</p>
          ) : (
            <ul style={{ listStyle: 'none', margin: '12px 0 0', padding: 0, fontSize: 13 }}>
              {dashboard.openOccurrences.map((o) => (
                <li key={o.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '6px 0', borderBottom: '1px solid var(--border)' }}>
                  <span>
                    {o.type} <span style={{ color: 'var(--text-muted)' }}>({o.vehiclePlate})</span>
                  </span>
                  <span className="badge badge-warning">{statusLabel(o.status)}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  )
}
