import { useEffect, useMemo, useState } from 'react'
import { api } from '../api/client'
import {
  DOCUMENT_OWNER_TYPES,
  DOCUMENT_TYPES,
  type Company,
  type Document,
  type Driver,
  type UpsertDocumentRequest,
  type Vehicle,
} from '../api/types'
import { statusLabel } from '../utils/statusLabel'
import { KpiStrip } from '../components/KpiStrip'

const EMPTY_FORM: UpsertDocumentRequest = {
  ownerType: 'Vehicle',
  ownerId: '',
  type: 'Crlv',
  number: '',
  issueDate: '',
  expiryDate: '',
  attachmentUrl: '',
}

const STATUS_BADGE: Record<Document['status'], string> = {
  Valido: 'badge-success',
  ProximoVencimento: 'badge-warning',
  Vencido: 'badge-danger',
}

export default function DocumentsPage() {
  const [documents, setDocuments] = useState<Document[]>([])
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [drivers, setDrivers] = useState<Driver[]>([])
  const [companies, setCompanies] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<UpsertDocumentRequest>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    Promise.all([api.listDocuments(), api.listVehicles(), api.listDrivers(), api.listCompanies()])
      .then(([docs, v, d, c]) => {
        setDocuments(docs)
        setVehicles(v)
        setDrivers(d)
        setCompanies(c)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar os documentos. Tente recarregar a página.'))
      .finally(() => setLoading(false))

  useEffect(() => {
    load()
  }, [])

  // Rótulo legível pro dono de um documento já salvo -- placa pro veículo,
  // nome pro motorista/empresa. Sem isso a tabela só mostraria um GUID cru.
  const ownerLabel = (doc: Document) => {
    if (doc.ownerType === 'Vehicle') return vehicles.find((v) => v.id === doc.ownerId)?.plateNumber ?? doc.ownerId
    if (doc.ownerType === 'Driver') return drivers.find((d) => d.id === doc.ownerId)?.name ?? doc.ownerId
    return companies.find((c) => c.id === doc.ownerId)?.name ?? doc.ownerId
  }

  // Opções do seletor de dono, filtradas pelo OwnerType escolhido no
  // formulário -- é o jeito do usuário achar o dono sem digitar um GUID.
  const ownerOptions = useMemo(() => {
    if (form.ownerType === 'Vehicle') return vehicles.map((v) => ({ id: v.id, label: `${v.plateNumber} — ${v.brand} ${v.model}` }))
    if (form.ownerType === 'Driver') return drivers.map((d) => ({ id: d.id, label: d.name }))
    return companies.map((c) => ({ id: c.id, label: c.name }))
  }, [form.ownerType, vehicles, drivers, companies])

  const openCreate = () => {
    setEditingId(null)
    setForm(EMPTY_FORM)
    setError(null)
    setFormOpen(true)
  }

  const openEdit = (doc: Document) => {
    setEditingId(doc.id)
    setForm({
      ownerType: doc.ownerType,
      ownerId: doc.ownerId,
      type: doc.type,
      number: doc.number,
      issueDate: doc.issueDate,
      expiryDate: doc.expiryDate,
      attachmentUrl: doc.attachmentUrl ?? '',
    })
    setError(null)
    setFormOpen(true)
  }

  const changeOwnerType = (ownerType: UpsertDocumentRequest['ownerType']) =>
    setForm((prev) => ({ ...prev, ownerType, ownerId: '' }))

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      if (editingId) {
        await api.updateDocument(editingId, form)
      } else {
        await api.createDocument(form)
      }
      setFormOpen(false)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar documento.')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (doc: Document) => {
    if (!confirm(`Remover o documento "${doc.number}"?`)) return
    await api.deleteDocument(doc.id)
    await load()
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Documentos</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          Novo documento
        </button>
      </div>

      {!loading && documents.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Total', value: String(documents.length) },
            { label: 'Válidos', value: String(documents.filter((d) => d.status === 'Valido').length), tone: 'success' },
            { label: 'Próximos do vencimento', value: String(documents.filter((d) => d.status === 'ProximoVencimento').length), tone: 'warning' },
            { label: 'Vencidos', value: String(documents.filter((d) => d.status === 'Vencido').length), tone: 'danger' },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : documents.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhum documento cadastrado ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Tipo</th>
              <th>Número</th>
              <th>Dono</th>
              <th>Validade</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {documents.map((doc) => (
              <tr key={doc.id}>
                <td data-label="Tipo">{doc.type}</td>
                <td data-label="Número">{doc.number}</td>
                <td data-label="Dono">
                  {ownerLabel(doc)} <span style={{ color: 'var(--text-muted)' }}>({doc.ownerType})</span>
                </td>
                <td data-label="Validade">{doc.expiryDate}</td>
                <td data-label="Status">
                  <span className={`badge ${STATUS_BADGE[doc.status]}`}>{statusLabel(doc.status)}</span>
                </td>
                <td data-label="Ações" className="table-actions">
                  <button className="btn" onClick={() => openEdit(doc)}>
                    Editar
                  </button>
                  <button className="btn btn-danger" onClick={() => handleDelete(doc)}>
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
            <h2>{editingId ? 'Editar documento' : 'Novo documento'}</h2>

            {error && <p className="form-error">{error}</p>}

            <div className="form-grid-2col">
              <label>
                Tipo de dono
                <select
                  value={form.ownerType}
                  onChange={(e) => changeOwnerType(e.target.value as UpsertDocumentRequest['ownerType'])}
                >
                  {DOCUMENT_OWNER_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Dono
                <select value={form.ownerId} onChange={(e) => setForm({ ...form, ownerId: e.target.value })}>
                  <option value="">Selecione...</option>
                  {ownerOptions.map((o) => (
                    <option key={o.id} value={o.id}>
                      {o.label}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Tipo de documento
                <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value as UpsertDocumentRequest['type'] })}>
                  {DOCUMENT_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Número
                <input value={form.number} onChange={(e) => setForm({ ...form, number: e.target.value })} />
              </label>
              <label>
                Data de emissão
                <input type="date" value={form.issueDate} onChange={(e) => setForm({ ...form, issueDate: e.target.value })} />
              </label>
              <label>
                Data de validade
                <input type="date" value={form.expiryDate} onChange={(e) => setForm({ ...form, expiryDate: e.target.value })} />
              </label>
              <label>
                URL do anexo
                <input value={form.attachmentUrl ?? ''} onChange={(e) => setForm({ ...form, attachmentUrl: e.target.value })} />
              </label>
            </div>

            <div className="modal-actions">
              <button className="btn" onClick={() => setFormOpen(false)}>
                Cancelar
              </button>
              <button
                className="btn btn-primary"
                disabled={saving || !form.ownerId || !form.number || !form.issueDate || !form.expiryDate}
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
