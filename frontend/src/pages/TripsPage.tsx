import { useEffect, useState } from 'react'
import { api } from '../api/client'
import {
  TRIP_EXPENSE_TYPES,
  type AddTripExpenseRequest,
  type Company,
  type Driver,
  type ScheduleTripRequest,
  type TransportOrder,
  type Trip,
  type TripProfitability,
  type Vehicle,
} from '../api/types'
import { statusLabel } from '../utils/statusLabel'
import { KpiStrip } from '../components/KpiStrip'

const EMPTY_SCHEDULE_FORM: ScheduleTripRequest = {
  transportOrderId: '',
  vehicleId: '',
  trailerVehicleId: null,
  driverId: '',
  scheduledDepartureAt: new Date().toISOString().slice(0, 16),
  estimatedArrivalAt: new Date(Date.now() + 86400000).toISOString().slice(0, 16),
  plannedDistanceKm: 0,
}

const EMPTY_EXPENSE_FORM: AddTripExpenseRequest = {
  type: 'Pedagio',
  description: '',
  value: 0,
  expenseDate: new Date().toISOString().slice(0, 10),
}

const STATUS_BADGE: Record<string, string> = {
  Programada: 'badge-warning',
  EmAndamento: 'badge-warning',
  Concluida: 'badge-success',
  Cancelada: 'badge-danger',
}

function currency(value: number) {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

function formatDateTime(value: string | null) {
  if (!value) return '—'
  return new Date(value).toLocaleString('pt-BR')
}

export default function TripsPage() {
  const [trips, setTrips] = useState<Trip[]>([])
  const [orders, setOrders] = useState<TransportOrder[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [drivers, setDrivers] = useState<Driver[]>([])
  const [companies, setCompanies] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)

  const [scheduleOpen, setScheduleOpen] = useState(false)
  const [scheduleForm, setScheduleForm] = useState<ScheduleTripRequest>(EMPTY_SCHEDULE_FORM)
  const [scheduleError, setScheduleError] = useState<string | null>(null)
  const [scheduling, setScheduling] = useState(false)

  const [detailTrip, setDetailTrip] = useState<Trip | null>(null)
  const [profitability, setProfitability] = useState<TripProfitability | null>(null)
  const [expenseForm, setExpenseForm] = useState<AddTripExpenseRequest>(EMPTY_EXPENSE_FORM)
  const [actualDistance, setActualDistance] = useState(0)
  const [detailError, setDetailError] = useState<string | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listTrips()
      .then((data) => {
        setTrips(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar as viagens. Tente recarregar a página.'))
      .finally(() => setLoading(false))

  useEffect(() => {
    load()
    api.listTransportOrders().then(setOrders)
    api.listVehicles().then(setVehicles)
    api.listDrivers().then(setDrivers)
    api.listCompanies().then(setCompanies)
  }, [])

  const schedulableOrders = orders.filter((o) => o.status === 'Criada')
  const availableVehicles = vehicles.filter((v) => v.status === 'Disponivel')
  const availableDrivers = drivers.filter((d) => d.availabilityStatus === 'Disponivel')

  const companyName = (id: string) => companies.find((c) => c.id === id)?.name ?? id

  const openSchedule = () => {
    setScheduleForm(EMPTY_SCHEDULE_FORM)
    setScheduleError(null)
    setScheduleOpen(true)
  }

  const handleSchedule = async () => {
    setScheduling(true)
    setScheduleError(null)
    try {
      await api.scheduleTrip({
        ...scheduleForm,
        scheduledDepartureAt: new Date(scheduleForm.scheduledDepartureAt).toISOString(),
        estimatedArrivalAt: new Date(scheduleForm.estimatedArrivalAt).toISOString(),
      })
      setScheduleOpen(false)
      await load()
      api.listTransportOrders().then(setOrders)
      api.listVehicles().then(setVehicles)
      api.listDrivers().then(setDrivers)
    } catch (err) {
      setScheduleError(err instanceof Error ? err.message : 'Erro ao programar viagem.')
    } finally {
      setScheduling(false)
    }
  }

  const openDetail = async (trip: Trip) => {
    setDetailTrip(trip)
    setDetailError(null)
    setActualDistance(Number(trip.plannedDistanceKm))
    setExpenseForm(EMPTY_EXPENSE_FORM)
    const p = await api.getTripProfitability(trip.id)
    setProfitability(p)
  }

  const refreshDetail = async (id: string) => {
    const trip = await api.getTrip(id)
    setDetailTrip(trip)
    setProfitability(await api.getTripProfitability(id))
    await load()
  }

  const handleStart = async () => {
    if (!detailTrip) return
    try {
      await api.startTrip(detailTrip.id)
      await refreshDetail(detailTrip.id)
    } catch (err) {
      setDetailError(err instanceof Error ? err.message : 'Erro ao iniciar viagem.')
    }
  }

  const handleAddExpense = async () => {
    if (!detailTrip) return
    try {
      await api.addTripExpense(detailTrip.id, expenseForm)
      setExpenseForm(EMPTY_EXPENSE_FORM)
      await refreshDetail(detailTrip.id)
    } catch (err) {
      setDetailError(err instanceof Error ? err.message : 'Erro ao adicionar despesa.')
    }
  }

  const handleCompleteDelivery = async () => {
    if (!detailTrip) return
    try {
      await api.completeTripDelivery(detailTrip.id, actualDistance)
      await refreshDetail(detailTrip.id)
    } catch (err) {
      setDetailError(err instanceof Error ? err.message : 'Erro ao concluir entrega.')
    }
  }

  const handleCancelTrip = async () => {
    if (!detailTrip) return
    if (!confirm('Cancelar esta viagem?')) return
    try {
      await api.cancelTrip(detailTrip.id)
      await refreshDetail(detailTrip.id)
    } catch (err) {
      setDetailError(err instanceof Error ? err.message : 'Erro ao cancelar viagem.')
    }
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Viagens</h1>
        <button className="btn btn-primary" onClick={openSchedule}>
          Programar viagem
        </button>
      </div>

      {!loading && trips.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Programadas', value: String(trips.filter((t) => t.status === 'Programada').length) },
            { label: 'Em andamento', value: String(trips.filter((t) => t.status === 'EmAndamento').length), tone: 'warning' },
            { label: 'Concluídas', value: String(trips.filter((t) => t.status === 'Concluida').length), tone: 'success' },
            { label: 'Receita concluída', value: currency(trips.filter((t) => t.status === 'Concluida').reduce((sum, t) => sum + t.freightRevenue, 0)) },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : trips.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhuma viagem ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Rota</th>
              <th>Veículo</th>
              <th>Motorista</th>
              <th>Receita</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {trips.map((t) => (
              <tr key={t.id}>
                <td data-label="Rota">
                  {t.originCity}/{t.originState} → {t.destinationCity}/{t.destinationState}
                </td>
                <td data-label="Veículo">{t.vehiclePlate}</td>
                <td data-label="Motorista">{t.driverName}</td>
                <td data-label="Receita">{currency(t.freightRevenue)}</td>
                <td data-label="Status">
                  <span className={`badge ${STATUS_BADGE[t.status]}`}>{statusLabel(t.status)}</span>
                </td>
                <td data-label="Ações" className="table-actions">
                  <button className="btn" onClick={() => openDetail(t)}>
                    Detalhes
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {scheduleOpen && (
        <div className="modal-backdrop" onClick={() => setScheduleOpen(false)}>
          <div className="modal-panel" onClick={(e) => e.stopPropagation()}>
            <h2>Programar viagem</h2>
            {scheduleError && <p className="form-error">{scheduleError}</p>}

            <div className="form-grid-2col">
              <label>
                Ordem de transporte
                <select value={scheduleForm.transportOrderId} onChange={(e) => setScheduleForm({ ...scheduleForm, transportOrderId: e.target.value })}>
                  <option value="">Selecione...</option>
                  {schedulableOrders.map((o) => (
                    <option key={o.id} value={o.id}>
                      {companyName(o.customerCompanyId)} — {o.originCity}/{o.originState} → {o.destinationCity}/{o.destinationState} ({o.cargoWeightKg}kg)
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Veículo
                <select value={scheduleForm.vehicleId} onChange={(e) => setScheduleForm({ ...scheduleForm, vehicleId: e.target.value })}>
                  <option value="">Selecione...</option>
                  {availableVehicles.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.plateNumber} — {v.brand} {v.model} ({v.type}, {v.capacityKg.toLocaleString('pt-BR')}kg)
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Motorista
                <select value={scheduleForm.driverId} onChange={(e) => setScheduleForm({ ...scheduleForm, driverId: e.target.value })}>
                  <option value="">Selecione...</option>
                  {availableDrivers.map((d) => (
                    <option key={d.id} value={d.id}>
                      {d.name} — CNH {d.cnhCategory} (válida até {d.cnhExpiryDate})
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Distância planejada (km)
                <input type="number" value={scheduleForm.plannedDistanceKm} onChange={(e) => setScheduleForm({ ...scheduleForm, plannedDistanceKm: Number(e.target.value) })} />
              </label>
              <label>
                Saída programada
                <input type="datetime-local" value={scheduleForm.scheduledDepartureAt} onChange={(e) => setScheduleForm({ ...scheduleForm, scheduledDepartureAt: e.target.value })} />
              </label>
              <label>
                Chegada estimada
                <input type="datetime-local" value={scheduleForm.estimatedArrivalAt} onChange={(e) => setScheduleForm({ ...scheduleForm, estimatedArrivalAt: e.target.value })} />
              </label>
            </div>

            <div className="modal-actions">
              <button className="btn" onClick={() => setScheduleOpen(false)}>
                Cancelar
              </button>
              <button
                className="btn btn-primary"
                disabled={scheduling || !scheduleForm.transportOrderId || !scheduleForm.vehicleId || !scheduleForm.driverId}
                onClick={handleSchedule}
              >
                {scheduling ? 'Programando...' : 'Programar'}
              </button>
            </div>
          </div>
        </div>
      )}

      {detailTrip && (
        <div className="modal-backdrop" onClick={() => setDetailTrip(null)}>
          <div className="modal-panel" onClick={(e) => e.stopPropagation()} style={{ maxWidth: 720 }}>
            <h2>
              {detailTrip.originCity}/{detailTrip.originState} → {detailTrip.destinationCity}/{detailTrip.destinationState}
            </h2>
            <p style={{ color: 'var(--text-muted)', marginTop: -8 }}>
              {detailTrip.vehiclePlate} · {detailTrip.driverName} · <span className={`badge ${STATUS_BADGE[detailTrip.status]}`}>{statusLabel(detailTrip.status)}</span>
            </p>

            {detailError && <p className="form-error">{detailError}</p>}

            <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginBottom: 16 }}>
              {detailTrip.status === 'Programada' && (
                <button className="btn btn-primary" onClick={handleStart}>
                  Iniciar viagem
                </button>
              )}
              {detailTrip.status === 'EmAndamento' && (
                <>
                  <input
                    type="number"
                    value={actualDistance}
                    onChange={(e) => setActualDistance(Number(e.target.value))}
                    style={{ width: 120, padding: 8, border: '1px solid var(--border)', borderRadius: 6, background: 'var(--bg)', color: 'var(--text)' }}
                    title="Km realizado"
                  />
                  <button className="btn btn-primary" onClick={handleCompleteDelivery}>
                    Concluir entrega
                  </button>
                </>
              )}
              {(detailTrip.status === 'Programada' || detailTrip.status === 'EmAndamento') && (
                <button className="btn btn-danger" onClick={handleCancelTrip}>
                  Cancelar viagem
                </button>
              )}
            </div>

            {profitability && (
              <fieldset>
                <legend>Rentabilidade</legend>
                <div className="quote-breakdown">
                  <div>
                    <span>Receita</span>
                    <span>{currency(profitability.revenue)}</span>
                  </div>
                  <div>
                    <span>Custo total</span>
                    <span>{currency(profitability.totalCost)}</span>
                  </div>
                  <div>
                    <span>Custo/km</span>
                    <span>{profitability.costPerKm !== null ? currency(profitability.costPerKm) : '—'}</span>
                  </div>
                  <div className="quote-breakdown-total">
                    <span>Margem {profitability.marginPercentage !== null ? `(${profitability.marginPercentage}%)` : ''}</span>
                    <span>{currency(profitability.margin)}</span>
                  </div>
                </div>
              </fieldset>
            )}

            <fieldset>
              <legend>Despesas</legend>
              {detailTrip.expenses.length === 0 ? (
                <p style={{ color: 'var(--text-muted)', margin: '0 0 8px' }}>Nenhuma despesa lançada.</p>
              ) : (
                <ul style={{ margin: '0 0 8px', padding: 0, listStyle: 'none', fontSize: 13 }}>
                  {detailTrip.expenses.map((e) => (
                    <li key={e.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '4px 0', borderBottom: '1px solid var(--border)' }}>
                      <span>
                        {e.type} {e.description ? `— ${e.description}` : ''} ({e.expenseDate})
                      </span>
                      <span>{currency(e.value)}</span>
                    </li>
                  ))}
                </ul>
              )}
              {detailTrip.status !== 'Concluida' && detailTrip.status !== 'Cancelada' && (
                <div className="form-subrow">
                  <select value={expenseForm.type} onChange={(e) => setExpenseForm({ ...expenseForm, type: e.target.value as AddTripExpenseRequest['type'] })}>
                    {TRIP_EXPENSE_TYPES.map((t) => (
                      <option key={t} value={t}>
                        {t}
                      </option>
                    ))}
                  </select>
                  <input placeholder="Descrição" value={expenseForm.description ?? ''} onChange={(e) => setExpenseForm({ ...expenseForm, description: e.target.value })} />
                  <input
                    type="number"
                    placeholder="Valor"
                    value={expenseForm.value}
                    onChange={(e) => setExpenseForm({ ...expenseForm, value: Number(e.target.value) })}
                    style={{ maxWidth: 100 }}
                  />
                  <input type="date" value={expenseForm.expenseDate} onChange={(e) => setExpenseForm({ ...expenseForm, expenseDate: e.target.value })} style={{ maxWidth: 150 }} />
                  <button type="button" className="btn btn-primary" onClick={handleAddExpense} disabled={expenseForm.value <= 0}>
                    Adicionar
                  </button>
                </div>
              )}
            </fieldset>

            <fieldset>
              <legend>Linha do tempo</legend>
              <ul style={{ margin: 0, padding: 0, listStyle: 'none', fontSize: 13 }}>
                {detailTrip.events.map((e) => (
                  <li key={e.id} style={{ padding: '4px 0', borderBottom: '1px solid var(--border)' }}>
                    <strong>{formatDateTime(e.occurredAt)}</strong> — {e.description}
                  </li>
                ))}
              </ul>
            </fieldset>

            <div className="modal-actions">
              <button className="btn" onClick={() => setDetailTrip(null)}>
                Fechar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
