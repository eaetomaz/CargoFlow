import { useEffect, useState } from 'react'
import { api } from '../api/client'
import { MAINTENANCE_TYPES, type CreateMaintenanceOrderRequest, type MaintenanceOrder, type Vehicle } from '../api/types'
import { statusLabel } from '../utils/statusLabel'
import { KpiStrip } from '../components/KpiStrip'

const EMPTY_FORM: CreateMaintenanceOrderRequest = {
  vehicleId: '',
  type: 'Preventiva',
  description: '',
  scheduledDate: null,
  serviceProvider: '',
}

const STATUS_BADGE: Record<string, string> = {
  Aberta: 'badge-warning',
  EmExecucao: 'badge-warning',
  Concluida: 'badge-success',
  Cancelada: 'badge-muted',
}

function currency(value: number | null) {
  if (value == null) return '—'
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function MaintenancePage() {
  const [orders, setOrders] = useState<MaintenanceOrder[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [loading, setLoading] = useState(true)

  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<CreateMaintenanceOrderRequest>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [completing, setCompleting] = useState<MaintenanceOrder | null>(null)
  const [completeCost, setCompleteCost] = useState(0)
  const [completeOdometer, setCompleteOdometer] = useState(0)
  const [completeError, setCompleteError] = useState<string | null>(null)

  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listMaintenanceOrders()
      .then((data) => {
        setOrders(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar as manutenções. Tente recarregar a página.'))
      .finally(() => setLoading(false))

  useEffect(() => {
    load()
    api.listVehicles().then(setVehicles)
  }, [])

  const openCreate = () => {
    setForm(EMPTY_FORM)
    setError(null)
    setFormOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      await api.createMaintenanceOrder({ ...form, scheduledDate: form.scheduledDate || null, serviceProvider: form.serviceProvider || null })
      setFormOpen(false)
      await load()
      api.listVehicles().then(setVehicles)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao abrir ordem de manutenção.')
    } finally {
      setSaving(false)
    }
  }

  const handleStart = async (order: MaintenanceOrder) => {
    try {
      await api.startMaintenanceOrder(order.id)
      await load()
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Erro ao iniciar ordem.')
    }
  }

  const openComplete = (order: MaintenanceOrder) => {
    setCompleting(order)
    setCompleteCost(0)
    setCompleteOdometer(0)
    setCompleteError(null)
  }

  const handleComplete = async () => {
    if (!completing) return
    setCompleteError(null)
    try {
      await api.completeMaintenanceOrder(completing.id, { cost: completeCost, odometerAtService: completeOdometer || null })
      setCompleting(null)
      await load()
      api.listVehicles().then(setVehicles)
    } catch (err) {
      setCompleteError(err instanceof Error ? err.message : 'Erro ao concluir ordem.')
    }
  }

  const handleCancel = async (order: MaintenanceOrder) => {
    if (!confirm('Cancelar esta ordem de manutenção?')) return
    try {
      await api.cancelMaintenanceOrder(order.id)
      await load()
      api.listVehicles().then(setVehicles)
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Erro ao cancelar ordem.')
    }
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Manutenção</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          Nova ordem
        </button>
      </div>

      {!loading && orders.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Abertas', value: String(orders.filter((o) => o.status === 'Aberta').length), tone: 'warning' },
            { label: 'Em execução', value: String(orders.filter((o) => o.status === 'EmExecucao').length) },
            { label: 'Concluídas', value: String(orders.filter((o) => o.status === 'Concluida').length), tone: 'success' },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : orders.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhuma ordem de manutenção ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Veículo</th>
              <th>Tipo</th>
              <th>Descrição</th>
              <th>Status</th>
              <th>Custo</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {orders.map((o) => (
              <tr key={o.id}>
                <td data-label="Veículo">{o.vehiclePlate}</td>
                <td data-label="Tipo">{o.type}</td>
                <td data-label="Descrição">{o.description}</td>
                <td data-label="Status">
                  <span className={`badge ${STATUS_BADGE[o.status]}`}>{statusLabel(o.status)}</span>
                </td>
                <td data-label="Custo">{currency(o.cost)}</td>
                <td data-label="Ações" className="table-actions">
                  {o.status === 'Aberta' && (
                    <button className="btn" onClick={() => handleStart(o)}>
                      Iniciar
                    </button>
                  )}
                  {o.status === 'EmExecucao' && (
                    <button className="btn btn-primary" onClick={() => openComplete(o)}>
                      Concluir
                    </button>
                  )}
                  {(o.status === 'Aberta' || o.status === 'EmExecucao') && (
                    <button className="btn btn-danger" onClick={() => handleCancel(o)}>
                      Cancelar
                    </button>
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
            <h2>Nova ordem de manutenção</h2>
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
                Tipo
                <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value as CreateMaintenanceOrderRequest['type'] })}>
                  {MAINTENANCE_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Data programada (opcional)
                <input type="date" value={form.scheduledDate ?? ''} onChange={(e) => setForm({ ...form, scheduledDate: e.target.value || null })} />
              </label>
              <label>
                Prestador (opcional)
                <input value={form.serviceProvider ?? ''} onChange={(e) => setForm({ ...form, serviceProvider: e.target.value })} />
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
              <button className="btn btn-primary" disabled={saving || !form.vehicleId || !form.description} onClick={handleSave}>
                {saving ? 'Salvando...' : 'Abrir ordem'}
              </button>
            </div>
          </div>
        </div>
      )}

      {completing && (
        <div className="modal-backdrop" onClick={() => setCompleting(null)}>
          <div className="modal-panel" onClick={(e) => e.stopPropagation()} style={{ maxWidth: 420 }}>
            <h2>Concluir manutenção</h2>
            <p style={{ color: 'var(--text-muted)', marginTop: -8 }}>{completing.vehiclePlate}</p>
            {completeError && <p className="form-error">{completeError}</p>}

            <div className="form-grid-2col">
              <label>
                Custo
                <input type="number" value={completeCost} onChange={(e) => setCompleteCost(Number(e.target.value))} />
              </label>
              <label>
                Odômetro no serviço (opcional)
                <input type="number" value={completeOdometer} onChange={(e) => setCompleteOdometer(Number(e.target.value))} />
              </label>
            </div>

            <div className="modal-actions">
              <button className="btn" onClick={() => setCompleting(null)}>
                Cancelar
              </button>
              <button className="btn btn-primary" disabled={completeCost <= 0} onClick={handleComplete}>
                Concluir
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
