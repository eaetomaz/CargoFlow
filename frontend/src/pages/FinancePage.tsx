import { useEffect, useState } from 'react'
import { api } from '../api/client'
import {
  ACCOUNT_PAYABLE_CATEGORIES,
  type AccountPayable,
  type AccountReceivable,
  type Company,
  type CreateAccountPayableRequest,
  type FinanceSummary,
} from '../api/types'
import { statusLabel } from '../utils/statusLabel'

const EMPTY_PAYABLE_FORM: CreateAccountPayableRequest = {
  category: 'Despesa',
  description: '',
  supplierCompanyId: null,
  sourceType: null,
  sourceId: null,
  amount: 0,
  dueDate: new Date().toISOString().slice(0, 10),
}

const STATUS_BADGE: Record<string, string> = {
  Pendente: 'badge-warning',
  Pago: 'badge-success',
  Recebido: 'badge-success',
  Vencido: 'badge-danger',
  Cancelado: 'badge-muted',
}

function currency(value: number) {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function FinancePage() {
  const [tab, setTab] = useState<'payable' | 'receivable'>('payable')
  const [summary, setSummary] = useState<FinanceSummary | null>(null)
  const [payables, setPayables] = useState<AccountPayable[]>([])
  const [receivables, setReceivables] = useState<AccountReceivable[]>([])
  const [suppliers, setSuppliers] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)

  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<CreateAccountPayableRequest>(EMPTY_PAYABLE_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () => {
    setLoading(true)
    Promise.all([api.getFinanceSummary(), api.listAccountsPayable(), api.listAccountsReceivable()])
      .then(([s, p, r]) => {
        setSummary(s)
        setPayables(p)
        setReceivables(r)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar os dados financeiros. Tente recarregar a página.'))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
    api.listCompanies().then((all) => setSuppliers(all.filter((c) => c.roles.includes('Fornecedor'))))
  }, [])

  const openCreate = () => {
    setForm(EMPTY_PAYABLE_FORM)
    setError(null)
    setFormOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      await api.createAccountPayable(form)
      setFormOpen(false)
      load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao lançar conta a pagar.')
    } finally {
      setSaving(false)
    }
  }

  const handlePay = async (p: AccountPayable) => {
    await api.payAccountPayable(p.id)
    load()
  }

  const handleCancelPayable = async (p: AccountPayable) => {
    if (!confirm('Cancelar esta conta a pagar?')) return
    await api.cancelAccountPayable(p.id)
    load()
  }

  const handleReceive = async (r: AccountReceivable) => {
    await api.receiveAccountReceivable(r.id)
    load()
  }

  const handleCancelReceivable = async (r: AccountReceivable) => {
    if (!confirm('Cancelar esta conta a receber?')) return
    await api.cancelAccountReceivable(r.id)
    load()
  }

  return (
    <div>
      <h1>Financeiro</h1>

      {summary && (
        <div className="kpi-grid" style={{ marginBottom: 20 }}>
          <div className="card">
            <span className="kpi-label">Recebido</span>
            <span className="kpi-value">{currency(summary.totalReceived)}</span>
          </div>
          <div className="card">
            <span className="kpi-label">A receber (pendente)</span>
            <span className="kpi-value">{currency(summary.totalReceivablePending)}</span>
          </div>
          <div className="card">
            <span className="kpi-label">A receber (vencido)</span>
            <span className="kpi-value" style={{ color: 'var(--danger)' }}>
              {currency(summary.totalReceivableOverdue)}
            </span>
          </div>
          <div className="card">
            <span className="kpi-label">Pago</span>
            <span className="kpi-value">{currency(summary.totalPaid)}</span>
          </div>
          <div className="card">
            <span className="kpi-label">A pagar (pendente)</span>
            <span className="kpi-value">{currency(summary.totalPayablePending)}</span>
          </div>
          <div className="card">
            <span className="kpi-label">A pagar (vencido)</span>
            <span className="kpi-value" style={{ color: 'var(--danger)' }}>
              {currency(summary.totalPayableOverdue)}
            </span>
          </div>
          <div className="card">
            <span className="kpi-label">Margem operacional</span>
            <span className="kpi-value" style={{ color: summary.netMargin >= 0 ? 'var(--success)' : 'var(--danger)' }}>
              {currency(summary.netMargin)}
            </span>
          </div>
        </div>
      )}

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
        <div className="tab-row">
          <button className={`tab-btn ${tab === 'payable' ? 'active' : ''}`} onClick={() => setTab('payable')}>
            Contas a pagar
          </button>
          <button className={`tab-btn ${tab === 'receivable' ? 'active' : ''}`} onClick={() => setTab('receivable')}>
            Contas a receber
          </button>
        </div>
        {tab === 'payable' && (
          <button className="btn btn-primary" onClick={openCreate}>
            Nova conta a pagar
          </button>
        )}
      </div>

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : tab === 'payable' ? (
        payables.length === 0 ? (
          <p style={{ color: 'var(--text-muted)' }}>Nenhuma conta a pagar ainda.</p>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Categoria</th>
                <th>Descrição</th>
                <th>Fornecedor</th>
                <th>Valor</th>
                <th>Vencimento</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {payables.map((p) => (
                <tr key={p.id}>
                  <td data-label="Categoria">{p.category}</td>
                  <td data-label="Descrição">{p.description}</td>
                  <td data-label="Fornecedor">{p.supplierCompanyName ?? '—'}</td>
                  <td data-label="Valor">{currency(p.amount)}</td>
                  <td data-label="Vencimento">{p.dueDate}</td>
                  <td data-label="Status">
                    <span className={`badge ${STATUS_BADGE[p.status]}`}>{statusLabel(p.status)}</span>
                  </td>
                  <td data-label="Ações" className="table-actions">
                    {(p.status === 'Pendente' || p.status === 'Vencido') && (
                      <>
                        <button className="btn btn-primary" onClick={() => handlePay(p)}>
                          Pagar
                        </button>
                        <button className="btn btn-danger" onClick={() => handleCancelPayable(p)}>
                          Cancelar
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )
      ) : receivables.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhuma conta a receber ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Cliente</th>
              <th>Valor</th>
              <th>Vencimento</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {receivables.map((r) => (
              <tr key={r.id}>
                <td data-label="Cliente">{r.customerCompanyName}</td>
                <td data-label="Valor">{currency(r.amount)}</td>
                <td data-label="Vencimento">{r.dueDate}</td>
                <td data-label="Status">
                  <span className={`badge ${STATUS_BADGE[r.status]}`}>{statusLabel(r.status)}</span>
                </td>
                <td data-label="Ações" className="table-actions">
                  {(r.status === 'Pendente' || r.status === 'Vencido') && (
                    <>
                      <button className="btn btn-primary" onClick={() => handleReceive(r)}>
                        Receber
                      </button>
                      <button className="btn btn-danger" onClick={() => handleCancelReceivable(r)}>
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
            <h2>Nova conta a pagar</h2>
            {error && <p className="form-error">{error}</p>}

            <div className="form-grid-2col">
              <label>
                Categoria
                <select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value as CreateAccountPayableRequest['category'] })}>
                  {ACCOUNT_PAYABLE_CATEGORIES.map((c) => (
                    <option key={c} value={c}>
                      {c}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Fornecedor (opcional)
                <select value={form.supplierCompanyId ?? ''} onChange={(e) => setForm({ ...form, supplierCompanyId: e.target.value || null })}>
                  <option value="">—</option>
                  {suppliers.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Descrição
                <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
              </label>
              <label>
                Valor (R$)
                <input type="number" value={form.amount} onChange={(e) => setForm({ ...form, amount: Number(e.target.value) })} />
              </label>
              <label>
                Vencimento
                <input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} />
              </label>
            </div>

            <div className="modal-actions">
              <button className="btn" onClick={() => setFormOpen(false)}>
                Cancelar
              </button>
              <button className="btn btn-primary" disabled={saving || !form.description || form.amount <= 0} onClick={handleSave}>
                {saving ? 'Salvando...' : 'Lançar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
