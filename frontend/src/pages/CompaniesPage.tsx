import { useEffect, useState } from 'react'
import { api } from '../api/client'
import { KpiStrip } from '../components/KpiStrip'
import {
  ADDRESS_TYPES,
  COMPANY_ROLES,
  type Company,
  type CompanyRole,
  type UpsertAddressRequest,
  type UpsertCompanyRequest,
  type UpsertContactRequest,
} from '../api/types'

const EMPTY_ADDRESS: UpsertAddressRequest = {
  type: 'Principal',
  street: '',
  number: '',
  complement: '',
  district: '',
  city: '',
  state: '',
  zipCode: '',
  isDefault: true,
}

const EMPTY_CONTACT: UpsertContactRequest = { name: '', role: '', email: '', phone: '' }

const EMPTY_FORM: UpsertCompanyRequest = {
  name: '',
  tradeName: '',
  document: '',
  documentType: 'Cnpj',
  stateRegistration: '',
  email: '',
  phone: '',
  roles: [],
  isActive: true,
  addresses: [],
  contacts: [],
}

export default function CompaniesPage() {
  const [companies, setCompanies] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<UpsertCompanyRequest>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listCompanies()
      .then((data) => {
        setCompanies(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar as empresas. Tente recarregar a página.'))
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

  const openEdit = (c: Company) => {
    setEditingId(c.id)
    setForm({
      name: c.name,
      tradeName: c.tradeName ?? '',
      document: c.document,
      documentType: c.documentType === 'Cpf' ? 'Cpf' : 'Cnpj',
      stateRegistration: c.stateRegistration ?? '',
      email: c.email ?? '',
      phone: c.phone ?? '',
      roles: c.roles,
      isActive: c.isActive,
      addresses: c.addresses.map((a) => ({ ...a })),
      contacts: c.contacts.map((ct) => ({ ...ct })),
    })
    setError(null)
    setFormOpen(true)
  }

  const toggleRole = (role: CompanyRole) =>
    setForm((prev) => ({
      ...prev,
      roles: prev.roles.includes(role) ? prev.roles.filter((r) => r !== role) : [...prev.roles, role],
    }))

  const addAddress = () => setForm((prev) => ({ ...prev, addresses: [...prev.addresses, { ...EMPTY_ADDRESS }] }))
  const removeAddress = (index: number) =>
    setForm((prev) => ({ ...prev, addresses: prev.addresses.filter((_, i) => i !== index) }))
  const updateAddress = (index: number, patch: Partial<UpsertAddressRequest>) =>
    setForm((prev) => ({
      ...prev,
      addresses: prev.addresses.map((a, i) => (i === index ? { ...a, ...patch } : a)),
    }))

  const addContact = () => setForm((prev) => ({ ...prev, contacts: [...prev.contacts, { ...EMPTY_CONTACT }] }))
  const removeContact = (index: number) =>
    setForm((prev) => ({ ...prev, contacts: prev.contacts.filter((_, i) => i !== index) }))
  const updateContact = (index: number, patch: Partial<UpsertContactRequest>) =>
    setForm((prev) => ({
      ...prev,
      contacts: prev.contacts.map((c, i) => (i === index ? { ...c, ...patch } : c)),
    }))

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      if (editingId) {
        await api.updateCompany(editingId, form)
      } else {
        await api.createCompany(form)
      }
      setFormOpen(false)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar empresa.')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (c: Company) => {
    if (!confirm(`Remover a empresa "${c.name}"?`)) return
    await api.deleteCompany(c.id)
    await load()
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Empresas</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          Nova empresa
        </button>
      </div>

      {!loading && companies.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Total', value: String(companies.length) },
            { label: 'Ativas', value: String(companies.filter((c) => c.isActive).length), tone: 'success' },
            { label: 'Clientes', value: String(companies.filter((c) => c.roles.includes('Cliente')).length) },
            { label: 'Fornecedores', value: String(companies.filter((c) => c.roles.includes('Fornecedor')).length) },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : companies.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhuma empresa cadastrada ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Nome</th>
              <th>Documento</th>
              <th>Papéis</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {companies.map((c) => (
              <tr key={c.id}>
                <td data-label="Nome">
                  {c.name}
                  {c.tradeName && <span style={{ color: 'var(--text-muted)' }}> ({c.tradeName})</span>}
                </td>
                <td data-label="Documento">{c.document}</td>
                <td data-label="Papéis">{c.roles.join(', ') || '—'}</td>
                <td data-label="Status">
                  <span className={c.isActive ? 'badge badge-success' : 'badge badge-muted'}>
                    {c.isActive ? 'Ativa' : 'Inativa'}
                  </span>
                </td>
                <td data-label="Ações" className="table-actions">
                  <button className="btn" onClick={() => openEdit(c)}>
                    Editar
                  </button>
                  <button className="btn btn-danger" onClick={() => handleDelete(c)}>
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
            <h2>{editingId ? 'Editar empresa' : 'Nova empresa'}</h2>

            {error && <p className="form-error">{error}</p>}

            <div className="form-grid-2col">
              <label>
                Nome
                <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </label>
              <label>
                Nome fantasia
                <input value={form.tradeName ?? ''} onChange={(e) => setForm({ ...form, tradeName: e.target.value })} />
              </label>
              <label>
                Documento
                <input value={form.document} onChange={(e) => setForm({ ...form, document: e.target.value })} />
              </label>
              <label>
                Tipo de documento
                <select value={form.documentType} onChange={(e) => setForm({ ...form, documentType: e.target.value as 'Cnpj' | 'Cpf' })}>
                  <option value="Cnpj">CNPJ</option>
                  <option value="Cpf">CPF</option>
                </select>
              </label>
              <label>
                Inscrição estadual
                <input value={form.stateRegistration ?? ''} onChange={(e) => setForm({ ...form, stateRegistration: e.target.value })} />
              </label>
              <label>
                E-mail
                <input value={form.email ?? ''} onChange={(e) => setForm({ ...form, email: e.target.value })} />
              </label>
              <label>
                Telefone
                <input value={form.phone ?? ''} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
              </label>
              <label className="checkbox-label">
                <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
                Ativa
              </label>
            </div>

            <fieldset>
              <legend>Papéis</legend>
              <div className="chip-checkbox-row">
                {COMPANY_ROLES.map((role) => (
                  <label key={role} className="chip-checkbox">
                    <input type="checkbox" checked={form.roles.includes(role)} onChange={() => toggleRole(role)} />
                    {role}
                  </label>
                ))}
              </div>
            </fieldset>

            <fieldset>
              <legend>Endereços</legend>
              {form.addresses.map((a, i) => (
                <div key={i} className="form-subrow">
                  <select value={a.type} onChange={(e) => updateAddress(i, { type: e.target.value as UpsertAddressRequest['type'] })}>
                    {ADDRESS_TYPES.map((t) => (
                      <option key={t} value={t}>
                        {t}
                      </option>
                    ))}
                  </select>
                  <input placeholder="Rua" value={a.street} onChange={(e) => updateAddress(i, { street: e.target.value })} />
                  <input placeholder="Número" value={a.number} onChange={(e) => updateAddress(i, { number: e.target.value })} style={{ maxWidth: 90 }} />
                  <input placeholder="Bairro" value={a.district} onChange={(e) => updateAddress(i, { district: e.target.value })} />
                  <input placeholder="Cidade" value={a.city} onChange={(e) => updateAddress(i, { city: e.target.value })} />
                  <input placeholder="UF" value={a.state} onChange={(e) => updateAddress(i, { state: e.target.value })} style={{ maxWidth: 60 }} />
                  <input placeholder="CEP" value={a.zipCode} onChange={(e) => updateAddress(i, { zipCode: e.target.value })} style={{ maxWidth: 110 }} />
                  <button type="button" className="btn btn-danger" onClick={() => removeAddress(i)}>
                    Remover
                  </button>
                </div>
              ))}
              <button type="button" className="btn" onClick={addAddress}>
                + Endereço
              </button>
            </fieldset>

            <fieldset>
              <legend>Contatos</legend>
              {form.contacts.map((c, i) => (
                <div key={i} className="form-subrow">
                  <input placeholder="Nome" value={c.name} onChange={(e) => updateContact(i, { name: e.target.value })} />
                  <input placeholder="Cargo" value={c.role ?? ''} onChange={(e) => updateContact(i, { role: e.target.value })} />
                  <input placeholder="E-mail" value={c.email ?? ''} onChange={(e) => updateContact(i, { email: e.target.value })} />
                  <input placeholder="Telefone" value={c.phone ?? ''} onChange={(e) => updateContact(i, { phone: e.target.value })} />
                  <button type="button" className="btn btn-danger" onClick={() => removeContact(i)}>
                    Remover
                  </button>
                </div>
              ))}
              <button type="button" className="btn" onClick={addContact}>
                + Contato
              </button>
            </fieldset>

            <div className="modal-actions">
              <button className="btn" onClick={() => setFormOpen(false)}>
                Cancelar
              </button>
              <button className="btn btn-primary" disabled={saving || !form.name || !form.document} onClick={handleSave}>
                {saving ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
