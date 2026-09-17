import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import type { Company, CreateTransportOrderRequest, TransportOrder } from '../api/types'
import { statusLabel } from '../utils/statusLabel'
import { KpiStrip } from '../components/KpiStrip'

const EMPTY_FORM: CreateTransportOrderRequest = {
  customerCompanyId: '',
  shipperCompanyId: null,
  consigneeCompanyId: null,
  originCity: '',
  originState: '',
  destinationCity: '',
  destinationState: '',
  cargoDescription: '',
  cargoWeightKg: 0,
  cargoValue: 0,
  freightValue: 0,
  requestedPickupDate: new Date().toISOString().slice(0, 10),
  requestedDeliveryDate: new Date().toISOString().slice(0, 10),
}

const STATUS_BADGE: Record<string, string> = {
  Criada: 'badge-muted',
  Programada: 'badge-warning',
  EmTransporte: 'badge-warning',
  Entregue: 'badge-success',
  Faturada: 'badge-success',
  Cancelada: 'badge-danger',
}

function currency(value: number) {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function TransportOrdersPage() {
  const [orders, setOrders] = useState<TransportOrder[]>([])
  const [customers, setCustomers] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<CreateTransportOrderRequest>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listTransportOrders()
      .then((data) => {
        setOrders(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar as ordens de transporte. Tente recarregar a página.'))
      .finally(() => setLoading(false))

  useEffect(() => {
    load()
    api.listCompanies().then((all) => setCustomers(all.filter((c) => c.roles.includes('Cliente'))))
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
      await api.createTransportOrder(form)
      setFormOpen(false)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao criar OT.')
    } finally {
      setSaving(false)
    }
  }

  const handleCancel = async (o: TransportOrder) => {
    if (!confirm('Cancelar esta ordem de transporte?')) return
    try {
      await api.cancelTransportOrder(o.id)
      await load()
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Erro ao cancelar.')
    }
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Ordens de transporte</h1>
        <div style={{ display: 'flex', gap: 8 }}>
          <Link className="btn" to="/viagens">
            Ir pra Viagens (programar)
          </Link>
          <button className="btn btn-primary" onClick={openCreate}>
            Nova OT
          </button>
        </div>
      </div>

      {!loading && orders.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Criadas', value: String(orders.filter((o) => o.status === 'Criada').length), tone: 'warning' },
            { label: 'Programadas', value: String(orders.filter((o) => o.status === 'Programada').length) },
            { label: 'Em transporte', value: String(orders.filter((o) => o.status === 'EmTransporte').length) },
            { label: 'Faturadas', value: String(orders.filter((o) => o.status === 'Faturada').length), tone: 'success' },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : orders.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhuma OT ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Cliente</th>
              <th>Rota</th>
              <th>Carga</th>
              <th>Frete</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {orders.map((o) => (
              <tr key={o.id}>
                <td data-label="Cliente">{o.customerCompanyName}</td>
                <td data-label="Rota">
                  {o.originCity}/{o.originState} → {o.destinationCity}/{o.destinationState}
                </td>
                <td data-label="Carga">{o.cargoDescription || `${o.cargoWeightKg.toLocaleString('pt-BR')} kg`}</td>
                <td data-label="Frete">{currency(o.freightValue)}</td>
                <td data-label="Status">
                  <span className={`badge ${STATUS_BADGE[o.status]}`}>{statusLabel(o.status)}</span>
                </td>
                <td data-label="Ações" className="table-actions">
                  {(o.status === 'Criada' || o.status === 'Programada') && (
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
            <h2>Nova ordem de transporte</h2>

            {error && <p className="form-error">{error}</p>}

            <div className="form-grid-2col">
              <label>
                Cliente
                <select value={form.customerCompanyId} onChange={(e) => setForm({ ...form, customerCompanyId: e.target.value })}>
                  <option value="">Selecione...</option>
                  {customers.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Descrição da carga
                <input value={form.cargoDescription} onChange={(e) => setForm({ ...form, cargoDescription: e.target.value })} />
              </label>
              <label>
                Cidade de origem
                <input value={form.originCity} onChange={(e) => setForm({ ...form, originCity: e.target.value })} />
              </label>
              <label>
                UF origem
                <input value={form.originState} maxLength={2} onChange={(e) => setForm({ ...form, originState: e.target.value.toUpperCase() })} />
              </label>
              <label>
                Cidade de destino
                <input value={form.destinationCity} onChange={(e) => setForm({ ...form, destinationCity: e.target.value })} />
              </label>
              <label>
                UF destino
                <input value={form.destinationState} maxLength={2} onChange={(e) => setForm({ ...form, destinationState: e.target.value.toUpperCase() })} />
              </label>
              <label>
                Peso da carga (kg)
                <input type="number" value={form.cargoWeightKg} onChange={(e) => setForm({ ...form, cargoWeightKg: Number(e.target.value) })} />
              </label>
              <label>
                Valor da carga (R$)
                <input type="number" value={form.cargoValue} onChange={(e) => setForm({ ...form, cargoValue: Number(e.target.value) })} />
              </label>
              <label>
                Valor do frete (R$)
                <input type="number" value={form.freightValue} onChange={(e) => setForm({ ...form, freightValue: Number(e.target.value) })} />
              </label>
              <label>
                Data de coleta desejada
                <input type="date" value={form.requestedPickupDate} onChange={(e) => setForm({ ...form, requestedPickupDate: e.target.value })} />
              </label>
              <label>
                Data de entrega desejada
                <input type="date" value={form.requestedDeliveryDate} onChange={(e) => setForm({ ...form, requestedDeliveryDate: e.target.value })} />
              </label>
            </div>

            <div className="modal-actions">
              <button className="btn" onClick={() => setFormOpen(false)}>
                Cancelar
              </button>
              <button className="btn btn-primary" disabled={saving || !form.customerCompanyId} onClick={handleSave}>
                {saving ? 'Criando...' : 'Criar OT'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
