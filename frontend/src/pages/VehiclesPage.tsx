import { useEffect, useState } from 'react'
import { api } from '../api/client'
import { VEHICLE_STATUSES, VEHICLE_TYPES, type UpsertVehicleRequest, type Vehicle } from '../api/types'
import { statusLabel } from '../utils/statusLabel'
import { KpiStrip } from '../components/KpiStrip'

const EMPTY_FORM: UpsertVehicleRequest = {
  plateNumber: '',
  renavam: '',
  brand: '',
  model: '',
  manufactureYear: new Date().getFullYear(),
  modelYear: new Date().getFullYear(),
  type: 'Cavalo',
  axleCount: 2,
  capacityKg: 0,
  tareWeightKg: 0,
  odometer: 0,
  status: 'Disponivel',
}

const STATUS_BADGE: Record<Vehicle['status'], string> = {
  Disponivel: 'badge-success',
  EmViagem: 'badge-warning',
  EmManutencao: 'badge-warning',
  Indisponivel: 'badge-danger',
}

export default function VehiclesPage() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [loading, setLoading] = useState(true)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<UpsertVehicleRequest>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listVehicles()
      .then((data) => {
        setVehicles(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar os veículos. Tente recarregar a página.'))
      .finally(() => setLoading(false))

  useEffect(() => {
    load()
  }, [])

  const openCreate = () => {
    setEditingId(null)
    setForm(EMPTY_FORM)
    setError(null)
    setFormOpen(true)
  }

  const openEdit = (v: Vehicle) => {
    setEditingId(v.id)
    setForm({
      plateNumber: v.plateNumber,
      renavam: v.renavam,
      brand: v.brand,
      model: v.model,
      manufactureYear: v.manufactureYear,
      modelYear: v.modelYear,
      type: v.type,
      axleCount: v.axleCount,
      capacityKg: v.capacityKg,
      tareWeightKg: v.tareWeightKg,
      odometer: v.odometer,
      status: v.status,
    })
    setError(null)
    setFormOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      if (editingId) {
        await api.updateVehicle(editingId, form)
      } else {
        await api.createVehicle(form)
      }
      setFormOpen(false)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar veículo.')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (v: Vehicle) => {
    if (!confirm(`Remover o veículo "${v.plateNumber}"?`)) return
    await api.deleteVehicle(v.id)
    await load()
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Veículos</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          Novo veículo
        </button>
      </div>

      {!loading && vehicles.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Total', value: String(vehicles.length) },
            { label: 'Disponíveis', value: String(vehicles.filter((v) => v.status === 'Disponivel').length), tone: 'success' },
            { label: 'Em viagem', value: String(vehicles.filter((v) => v.status === 'EmViagem').length) },
            { label: 'Em manutenção', value: String(vehicles.filter((v) => v.status === 'EmManutencao').length), tone: 'warning' },
            { label: 'Indisponíveis', value: String(vehicles.filter((v) => v.status === 'Indisponivel').length), tone: 'danger' },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : vehicles.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhum veículo cadastrado ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Placa</th>
              <th>Marca / Modelo</th>
              <th>Tipo</th>
              <th>Odômetro</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {vehicles.map((v) => (
              <tr key={v.id}>
                <td data-label="Placa">{v.plateNumber}</td>
                <td data-label="Marca / Modelo">
                  {v.brand} {v.model}
                </td>
                <td data-label="Tipo">{v.type}</td>
                <td data-label="Odômetro">{v.odometer.toLocaleString('pt-BR')} km</td>
                <td data-label="Status">
                  <span className={`badge ${STATUS_BADGE[v.status]}`}>{statusLabel(v.status)}</span>
                </td>
                <td data-label="Ações" className="table-actions">
                  <button className="btn" onClick={() => openEdit(v)}>
                    Editar
                  </button>
                  <button className="btn btn-danger" onClick={() => handleDelete(v)}>
                    Remover
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {formOpen && (
        <div className="modal-backdrop" onClick={() => setFormOpen(false)}>
          <div className="modal-panel" onClick={(e) => e.stopPropagation()}>
            <h2>{editingId ? 'Editar veículo' : 'Novo veículo'}</h2>

            {error && <p className="form-error">{error}</p>}

            <div className="form-grid-2col">
              <label>
                Placa
                <input value={form.plateNumber} onChange={(e) => setForm({ ...form, plateNumber: e.target.value.toUpperCase() })} />
              </label>
              <label>
                Renavam
                <input value={form.renavam} onChange={(e) => setForm({ ...form, renavam: e.target.value })} />
              </label>
              <label>
                Marca
                <input value={form.brand} onChange={(e) => setForm({ ...form, brand: e.target.value })} />
              </label>
              <label>
                Modelo
                <input value={form.model} onChange={(e) => setForm({ ...form, model: e.target.value })} />
              </label>
              <label>
                Ano de fabricação
                <input
                  type="number"
                  value={form.manufactureYear}
                  onChange={(e) => setForm({ ...form, manufactureYear: Number(e.target.value) })}
                />
              </label>
              <label>
                Ano do modelo
                <input type="number" value={form.modelYear} onChange={(e) => setForm({ ...form, modelYear: Number(e.target.value) })} />
              </label>
              <label>
                Tipo
                <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value as UpsertVehicleRequest['type'] })}>
                  {VEHICLE_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Nº de eixos
                <input type="number" value={form.axleCount} onChange={(e) => setForm({ ...form, axleCount: Number(e.target.value) })} />
              </label>
              <label>
                Capacidade (kg)
                <input type="number" value={form.capacityKg} onChange={(e) => setForm({ ...form, capacityKg: Number(e.target.value) })} />
              </label>
              <label>
                Tara (kg)
                <input type="number" value={form.tareWeightKg} onChange={(e) => setForm({ ...form, tareWeightKg: Number(e.target.value) })} />
              </label>
              <label>
                Odômetro (km)
                <input type="number" value={form.odometer} onChange={(e) => setForm({ ...form, odometer: Number(e.target.value) })} />
              </label>
              <label>
                Status
                <select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value as UpsertVehicleRequest['status'] })}>
                  {VEHICLE_STATUSES.map((s) => (
                    <option key={s} value={s}>
                      {statusLabel(s)}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            <div className="modal-actions">
              <button className="btn" onClick={() => setFormOpen(false)}>
                Cancelar
              </button>
              <button className="btn btn-primary" disabled={saving || !form.plateNumber || !form.renavam} onClick={handleSave}>
                {saving ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
