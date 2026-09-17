import { useEffect, useState } from 'react'
import { api } from '../api/client'
import {
  OCCURRENCE_TYPES,
  type CreateOccurrenceRequest,
  type Driver,
  type Occurrence,
  type Trip,
  type Vehicle,
} from '../api/types'
import { statusLabel } from '../utils/statusLabel'
import { KpiStrip } from '../components/KpiStrip'

const EMPTY_FORM: CreateOccurrenceRequest = {
  tripId: null,
  vehicleId: '',
  driverId: '',
  type: 'Atraso',
  occurredAt: new Date().toISOString().slice(0, 16),
  location: '',
  description: '',
  responsibleUserId: null,
}

const STATUS_BADGE: Record<string, string> = {
  Aberta: 'badge-warning',
  EmAndamento: 'badge-warning',
  Resolvida: 'badge-success',
  Cancelada: 'badge-muted',
}

function formatDateTime(value: string | null) {
  if (!value) return '—'
  return new Date(value).toLocaleString('pt-BR')
}

export default function OccurrencesPage() {
  const [occurrences, setOccurrences] = useState<Occurrence[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [drivers, setDrivers] = useState<Driver[]>([])
  const [trips, setTrips] = useState<Trip[]>([])
  const [loading, setLoading] = useState(true)

  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<CreateOccurrenceRequest>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listOccurrences()
      .then((data) => {
        setOccurrences(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar as ocorrências. Tente recarregar a página.'))
      .finally(() => setLoading(false))

  useEffect(() => {
    load()
    api.listVehicles().then(setVehicles)
    api.listDrivers().then(setDrivers)
    api.listTrips().then(setTrips)
  }, [])

  const openTrips = trips.filter((t) => t.status !== 'Concluida' && t.status !== 'Cancelada')

  const openCreate = () => {
    setForm(EMPTY_FORM)
    setError(null)
    setFormOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      await api.createOccurrence({
        ...form,
        tripId: form.tripId || null,
        occurredAt: new Date(form.occurredAt).toISOString(),
      })
      setFormOpen(false)
      await load()
      api.listVehicles().then(setVehicles)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao registrar ocorrência.')
    } finally {
      setSaving(false)
    }
  }

  const handleResolve = async (o: Occurrence) => {
    try {
      await api.resolveOccurrence(o.id)
      await load()
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Erro ao resolver ocorrência.')
    }
  }

  const handleCancel = async (o: Occurrence) => {
    if (!confirm('Cancelar esta ocorrência?')) return
    try {
      await api.cancelOccurrence(o.id)
      await load()
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Erro ao cancelar ocorrência.')
    }
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Ocorrências</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          Nova ocorrência
        </button>
      </div>

      {!loading && occurrences.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Total', value: String(occurrences.length) },
            { label: 'Abertas', value: String(occurrences.filter((o) => o.status === 'Aberta').length), tone: 'danger' },
            { label: 'Em andamento', value: String(occurrences.filter((o) => o.status === 'EmAndamento').length), tone: 'warning' },
            { label: 'Resolvidas', value: String(occurrences.filter((o) => o.status === 'Resolvida').length), tone: 'success' },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : occurrences.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhuma ocorrência registrada ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Tipo</th>
              <th>Veículo</th>
              <th>Motorista</th>
              <th>Data</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {occurrences.map((o) => (
              <tr key={o.id}>
                <td data-label="Tipo">{o.type}</td>
                <td data-label="Veículo">{o.vehiclePlate}</td>
                <td data-label="Motorista">{o.driverName}</td>
                <td data-label="Data">{formatDateTime(o.occurredAt)}</td>
                <td data-label="Status">
                  <span className={`badge ${STATUS_BADGE[o.status]}`}>{statusLabel(o.status)}</span>
                </td>
                <td data-label="Ações" className="table-actions">
                  {(o.status === 'Aberta' || o.status === 'EmAndamento') && (
                    <>
                      <button className="btn" onClick={() => handleResolve(o)}>
                        Resolver
                      </button>
                      <button className="btn btn-danger" onClick={() => handleCancel(o)}>
                        Cancelar
                      </button>
                    </>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {formOpen && (
        <div className="modal-backdrop" onClick={() => setFormOpen(false)}>
          <div className="modal-panel" onClick={(e) => e.stopPropagation()}>
            <h2>Nova ocorrência</h2>
            {error && <p className="form-error">{error}</p>}

            <div className="form-grid-2col">
              <label>
                Veículo
                <select value={form.vehicleId} onChange={(e) => setForm({ ...form, vehicleId: e.target.value })}>
                  <option value="">Selecione...</option>
                  {vehicles.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.plateNumber} — {v.brand} {v.model}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Motorista
                <select value={form.driverId} onChange={(e) => setForm({ ...form, driverId: e.target.value })}>
                  <option value="">Selecione...</option>
                  {drivers.map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Viagem (opcional)
                <select value={form.tripId ?? ''} onChange={(e) => setForm({ ...form, tripId: e.target.value || null })}>
                  <option value="">Nenhuma</option>
                  {openTrips.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.vehiclePlate} — {t.originCity} → {t.destinationCity}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Tipo
                <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value as CreateOccurrenceRequest['type'] })}>
                  {OCCURRENCE_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Data/hora
                <input type="datetime-local" value={form.occurredAt} onChange={(e) => setForm({ ...form, occurredAt: e.target.value })} />
              </label>
              <label>
                Local
                <input value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} />
              </label>
            </div>

            <label style={{ display: 'flex', flexDirection: 'column', gap: 4, fontSize: 13, marginBottom: 16 }}>
              Descrição
              <textarea
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                rows={3}
                style={{ font: 'inherit', padding: 8, border: '1px solid var(--border)', borderRadius: 6, background: 'var(--bg)', color: 'var(--text)' }}
              />
            </label>

            <div className="modal-actions">
              <button className="btn" onClick={() => setFormOpen(false)}>
                Cancelar
              </button>
              <button
                className="btn btn-primary"
                disabled={saving || !form.vehicleId || !form.driverId || !form.location || !form.description}
                onClick={handleSave}
              >
                {saving ? 'Salvando...' : 'Registrar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
