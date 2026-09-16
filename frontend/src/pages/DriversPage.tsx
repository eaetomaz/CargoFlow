import { useEffect, useState } from 'react'
import { api } from '../api/client'
import {
  CNH_CATEGORIES,
  DRIVER_AVAILABILITY_STATUSES,
  EMPLOYMENT_TYPES,
  type Driver,
  type UpsertDriverRequest,
} from '../api/types'
import { statusLabel } from '../utils/statusLabel'
import { KpiStrip } from '../components/KpiStrip'

const EMPTY_FORM: UpsertDriverRequest = {
  name: '',
  cpf: '',
  birthDate: '',
  phone: '',
  email: '',
  cnhNumber: '',
  cnhCategory: 'B',
  cnhExpiryDate: '',
  hireDate: '',
  employmentType: 'Clt',
  availabilityStatus: 'Disponivel',
}

const STATUS_BADGE: Record<Driver['availabilityStatus'], string> = {
  Disponivel: 'badge-success',
  EmViagem: 'badge-warning',
  Ferias: 'badge-warning',
  Afastado: 'badge-danger',
  Inativo: 'badge-muted',
}

export default function DriversPage() {
  const [drivers, setDrivers] = useState<Driver[]>([])
  const [loading, setLoading] = useState(true)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<UpsertDriverRequest>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listDrivers()
      .then((data) => {
        setDrivers(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar os motoristas. Tente recarregar a página.'))
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

  const openEdit = (d: Driver) => {
    setEditingId(d.id)
    setForm({
      name: d.name,
      cpf: d.cpf,
      birthDate: d.birthDate,
      phone: d.phone ?? '',
      email: d.email ?? '',
      cnhNumber: d.cnhNumber,
      cnhCategory: d.cnhCategory,
      cnhExpiryDate: d.cnhExpiryDate,
      hireDate: d.hireDate,
      employmentType: d.employmentType,
      availabilityStatus: d.availabilityStatus,
    })
    setError(null)
    setFormOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      if (editingId) {
        await api.updateDriver(editingId, form)
      } else {
        await api.createDriver(form)
      }
      setFormOpen(false)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar motorista.')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (d: Driver) => {
    if (!confirm(`Remover o motorista "${d.name}"?`)) return
    await api.deleteDriver(d.id)
    await load()
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Motoristas</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          Novo motorista
        </button>
      </div>

      {!loading && drivers.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Total', value: String(drivers.length) },
            { label: 'Disponíveis', value: String(drivers.filter((d) => d.availabilityStatus === 'Disponivel').length), tone: 'success' },
            { label: 'Em viagem', value: String(drivers.filter((d) => d.availabilityStatus === 'EmViagem').length) },
            {
              label: 'CNH vencendo (30d)',
              value: String(
                drivers.filter((d) => {
                  const days = (new Date(d.cnhExpiryDate).getTime() - Date.now()) / 86400000
                  return days <= 30
                }).length,
              ),
              tone: 'warning',
            },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : drivers.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhum motorista cadastrado ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Nome</th>
              <th>CPF</th>
              <th>CNH</th>
              <th>Validade CNH</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {drivers.map((d) => (
              <tr key={d.id}>
                <td data-label="Nome">{d.name}</td>
                <td data-label="CPF">{d.cpf}</td>
                <td data-label="CNH">
                  {d.cnhNumber} ({d.cnhCategory})
                </td>
                <td data-label="Validade CNH">{d.cnhExpiryDate}</td>
                <td data-label="Status">
                  <span className={`badge ${STATUS_BADGE[d.availabilityStatus]}`}>{statusLabel(d.availabilityStatus)}</span>
                </td>
                <td data-label="Ações" className="table-actions">
                  <button className="btn" onClick={() => openEdit(d)}>
                    Editar
                  </button>
                  <button className="btn btn-danger" onClick={() => handleDelete(d)}>
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
            <h2>{editingId ? 'Editar motorista' : 'Novo motorista'}</h2>

            {error && <p className="form-error">{error}</p>}

            <div className="form-grid-2col">
              <label>
                Nome
                <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </label>
              <label>
                CPF
                <input value={form.cpf} onChange={(e) => setForm({ ...form, cpf: e.target.value })} />
              </label>
              <label>
                Data de nascimento
                <input type="date" value={form.birthDate} onChange={(e) => setForm({ ...form, birthDate: e.target.value })} />
              </label>
              <label>
                Telefone
                <input value={form.phone ?? ''} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
              </label>
              <label>
                E-mail
                <input value={form.email ?? ''} onChange={(e) => setForm({ ...form, email: e.target.value })} />
              </label>
              <label>
                Nº da CNH
                <input value={form.cnhNumber} onChange={(e) => setForm({ ...form, cnhNumber: e.target.value })} />
              </label>
              <label>
                Categoria da CNH
                <select
                  value={form.cnhCategory}
                  onChange={(e) => setForm({ ...form, cnhCategory: e.target.value as UpsertDriverRequest['cnhCategory'] })}
                >
                  {CNH_CATEGORIES.map((c) => (
                    <option key={c} value={c}>
                      {c}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Validade da CNH
                <input type="date" value={form.cnhExpiryDate} onChange={(e) => setForm({ ...form, cnhExpiryDate: e.target.value })} />
              </label>
              <label>
                Data de admissão
                <input type="date" value={form.hireDate} onChange={(e) => setForm({ ...form, hireDate: e.target.value })} />
              </label>
              <label>
                Vínculo
                <select
                  value={form.employmentType}
                  onChange={(e) => setForm({ ...form, employmentType: e.target.value as UpsertDriverRequest['employmentType'] })}
                >
                  {EMPLOYMENT_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Disponibilidade
                <select
                  value={form.availabilityStatus}
                  onChange={(e) => setForm({ ...form, availabilityStatus: e.target.value as UpsertDriverRequest['availabilityStatus'] })}
                >
                  {DRIVER_AVAILABILITY_STATUSES.map((s) => (
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
              <button
                className="btn btn-primary"
                disabled={saving || !form.name || !form.cpf || !form.cnhNumber || !form.birthDate || !form.cnhExpiryDate || !form.hireDate}
                onClick={handleSave}
              >
                {saving ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
