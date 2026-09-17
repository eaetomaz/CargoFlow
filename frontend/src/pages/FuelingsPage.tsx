import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { CreateFuelingRequest, Driver, Fueling, Trip, Vehicle } from '../api/types'
import { KpiStrip } from '../components/KpiStrip'

const EMPTY_FORM: CreateFuelingRequest = {
  vehicleId: '',
  driverId: null,
  tripId: null,
  gasStationName: '',
  fuelingDate: new Date().toISOString().slice(0, 10),
  literQuantity: 0,
  totalValue: 0,
  odometerReading: 0,
}

function currency(value: number) {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function FuelingsPage() {
  const [fuelings, setFuelings] = useState<Fueling[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [drivers, setDrivers] = useState<Driver[]>([])
  const [trips, setTrips] = useState<Trip[]>([])
  const [efficiencyByVehicle, setEfficiencyByVehicle] = useState<Record<string, number | null>>({})
  const [loading, setLoading] = useState(true)

  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<CreateFuelingRequest>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listFuelings()
      .then((data) => {
        setFuelings(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar os abastecimentos. Tente recarregar a página.'))
      .finally(() => setLoading(false))

  useEffect(() => {
    load()
    api.listVehicles().then(setVehicles)
    api.listDrivers().then(setDrivers)
    api.listTrips().then(setTrips)
  }, [])

  useEffect(() => {
    const vehicleIds = [...new Set(fuelings.map((f) => f.vehicleId))]
    vehicleIds.forEach((id) => {
      if (id in efficiencyByVehicle) return
      api.getFuelEfficiency(id).then((eff) => setEfficiencyByVehicle((prev) => ({ ...prev, [id]: eff.kmPerLiter })))
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [fuelings])

  const selectedVehicle = vehicles.find((v) => v.id === form.vehicleId)

  const openCreate = () => {
    setForm(EMPTY_FORM)
    setError(null)
    setFormOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      await api.createFueling(form)
      setFormOpen(false)
      setEfficiencyByVehicle({})
      await load()
      api.listVehicles().then(setVehicles)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao registrar abastecimento.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Abastecimentos</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          Novo abastecimento
        </button>
      </div>

      {!loading && fuelings.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Abastecimentos', value: String(fuelings.length) },
            { label: 'Litros totais', value: `${fuelings.reduce((sum, f) => sum + f.literQuantity, 0).toLocaleString('pt-BR', { maximumFractionDigits: 0 })}L` },
            { label: 'Gasto total', value: currency(fuelings.reduce((sum, f) => sum + f.totalValue, 0)) },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : fuelings.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhum abastecimento registrado ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Veículo</th>
              <th>Posto</th>
              <th>Data</th>
              <th>Litros</th>
              <th>Valor</th>
              <th>Km/l</th>
            </tr>
          </thead>
          <tbody>
            {fuelings.map((f) => (
              <tr key={f.id}>
                <td data-label="Veículo">{f.vehiclePlate}</td>
                <td data-label="Posto">{f.gasStationName}</td>
                <td data-label="Data">{f.fuelingDate}</td>
                <td data-label="Litros">{f.literQuantity.toLocaleString('pt-BR')}</td>
                <td data-label="Valor">{currency(f.totalValue)}</td>
                <td data-label="Km/l">{efficiencyByVehicle[f.vehicleId] != null ? efficiencyByVehicle[f.vehicleId]!.toFixed(2) : '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {formOpen && (
        <div className="modal-backdrop" onClick={() => setFormOpen(false)}>
          <div className="modal-panel" onClick={(e) => e.stopPropagation()}>
            <h2>Novo abastecimento</h2>
            {error && <p className="form-error">{error}</p>}

            <div className="form-grid-2col">
              <label>
                Veículo {selectedVehicle && <span style={{ color: 'var(--text-muted)', fontWeight: 400 }}>(odômetro atual: {selectedVehicle.odometer.toLocaleString('pt-BR')}km)</span>}
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
                Motorista (opcional)
                <select value={form.driverId ?? ''} onChange={(e) => setForm({ ...form, driverId: e.target.value || null })}>
                  <option value="">Nenhum</option>
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
                  {trips.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.vehiclePlate} — {t.originCity} → {t.destinationCity}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Posto
                <input value={form.gasStationName} onChange={(e) => setForm({ ...form, gasStationName: e.target.value })} />
              </label>
              <label>
                Data
                <input type="date" value={form.fuelingDate} onChange={(e) => setForm({ ...form, fuelingDate: e.target.value })} />
              </label>
              <label>
                Litros
                <input type="number" value={form.literQuantity} onChange={(e) => setForm({ ...form, literQuantity: Number(e.target.value) })} />
              </label>
              <label>
                Valor total
                <input type="number" value={form.totalValue} onChange={(e) => setForm({ ...form, totalValue: Number(e.target.value) })} />
              </label>
              <label>
                Leitura do odômetro (km)
                <input type="number" value={form.odometerReading} onChange={(e) => setForm({ ...form, odometerReading: Number(e.target.value) })} />
              </label>
            </div>

            <div className="modal-actions">
              <button className="btn" onClick={() => setFormOpen(false)}>
                Cancelar
              </button>
              <button
                className="btn btn-primary"
                disabled={saving || !form.vehicleId || !form.gasStationName || form.literQuantity <= 0 || form.totalValue <= 0}
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
