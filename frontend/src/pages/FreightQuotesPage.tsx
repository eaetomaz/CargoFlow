import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { CARGO_TYPES, VEHICLE_TYPES, type Company, type CreateFreightQuoteRequest, type FreightQuote, type FreightQuoteBreakdown } from '../api/types'
import { statusLabel } from '../utils/statusLabel'
import { KpiStrip } from '../components/KpiStrip'

const EMPTY_FORM: CreateFreightQuoteRequest = {
  customerCompanyId: '',
  originCity: '',
  originState: '',
  destinationCity: '',
  destinationState: '',
  cargoType: 'Geral',
  cargoWeightKg: 0,
  cargoValue: 0,
  requiredVehicleType: 'CaminhaoToco',
  estimatedDistanceKm: 0,
  otherCostsValue: 0,
  validForDays: 15,
}

const STATUS_BADGE: Record<string, string> = {
  Draft: 'badge-muted',
  Sent: 'badge-warning',
  Approved: 'badge-success',
  Rejected: 'badge-danger',
  Expired: 'badge-muted',
}

function currency(value: number) {
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

export default function FreightQuotesPage() {
  const [quotes, setQuotes] = useState<FreightQuote[]>([])
  const [customers, setCustomers] = useState<Company[]>([])
  const [loading, setLoading] = useState(true)
  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<CreateFreightQuoteRequest>(EMPTY_FORM)
  const [preview, setPreview] = useState<FreightQuoteBreakdown | null>(null)
  const [previewError, setPreviewError] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listFreightQuotes()
      .then((data) => {
        setQuotes(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar as cotações. Tente recarregar a página.'))
      .finally(() => setLoading(false))

  useEffect(() => {
    load()
    api.listCompanies().then((all) => setCustomers(all.filter((c) => c.roles.includes('Cliente'))))
  }, [])

  // Recalcula o preview a cada mudança relevante do formulário -- é isso
  // que dá a sensação de "cálculo ao vivo" enquanto o usuário preenche.
  useEffect(() => {
    if (!formOpen) return
    if (form.cargoWeightKg <= 0 && form.cargoValue <= 0 && form.estimatedDistanceKm <= 0) {
      setPreview(null)
      return
    }

    const timer = setTimeout(() => {
      api
        .calculateFreightQuote({
          cargoType: form.cargoType,
          cargoWeightKg: form.cargoWeightKg,
          cargoValue: form.cargoValue,
          requiredVehicleType: form.requiredVehicleType,
          estimatedDistanceKm: form.estimatedDistanceKm,
          otherCostsValue: form.otherCostsValue,
        })
        .then((b) => {
          setPreview(b)
          setPreviewError(null)
        })
        .catch((err) => {
          setPreview(null)
          setPreviewError(err instanceof Error ? err.message : 'Erro ao calcular.')
        })
    }, 300)

    return () => clearTimeout(timer)
  }, [formOpen, form.cargoType, form.cargoWeightKg, form.cargoValue, form.requiredVehicleType, form.estimatedDistanceKm, form.otherCostsValue])

  const openCreate = () => {
    setForm(EMPTY_FORM)
    setPreview(null)
    setPreviewError(null)
    setError(null)
    setFormOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      await api.createFreightQuote(form)
      setFormOpen(false)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao criar cotação.')
    } finally {
      setSaving(false)
    }
  }

  const handleApprove = async (q: FreightQuote) => {
    await api.approveFreightQuote(q.id)
    await load()
  }

  const handleReject = async (q: FreightQuote) => {
    if (!confirm('Rejeitar esta cotação?')) return
    await api.rejectFreightQuote(q.id)
    await load()
  }

  const handleGenerateOrder = async (q: FreightQuote) => {
    try {
      await api.createTransportOrderFromQuote(q.id, {
        cargoDescription: `${q.cargoType} -- ${q.originCity}/${q.originState} -> ${q.destinationCity}/${q.destinationState}`,
        requestedPickupDate: new Date().toISOString().slice(0, 10),
        requestedDeliveryDate: new Date(Date.now() + 3 * 86400000).toISOString().slice(0, 10),
      })
      alert('Ordem de transporte gerada! Confira na aba Ordens de Transporte.')
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Erro ao gerar OT.')
    }
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Cotações de frete</h1>
        <div style={{ display: 'flex', gap: 8 }}>
          <Link className="btn" to="/tabela-precos">
            Tabela de preços
          </Link>
          <button className="btn btn-primary" onClick={openCreate}>
            Nova cotação
          </button>
        </div>
      </div>

      {!loading && quotes.length > 0 && (
        <KpiStrip
          items={[
            { label: 'Aguardando decisão', value: String(quotes.filter((q) => q.status === 'Sent').length), tone: 'warning' },
            { label: 'Aprovadas', value: String(quotes.filter((q) => q.status === 'Approved').length), tone: 'success' },
            { label: 'Rejeitadas', value: String(quotes.filter((q) => q.status === 'Rejected').length), tone: 'danger' },
            { label: 'Valor cotado (aprovadas)', value: currency(quotes.filter((q) => q.status === 'Approved').reduce((sum, q) => sum + q.totalValue, 0)) },
          ]}
        />
      )}

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : quotes.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhuma cotação ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Cliente</th>
              <th>Rota</th>
              <th>Veículo</th>
              <th>Total</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {quotes.map((q) => (
              <tr key={q.id}>
                <td data-label="Cliente">{q.customerCompanyName}</td>
                <td data-label="Rota">
                  {q.originCity}/{q.originState} → {q.destinationCity}/{q.destinationState}
                </td>
                <td data-label="Veículo">{q.requiredVehicleType}</td>
                <td data-label="Total">{currency(q.totalValue)}</td>
                <td data-label="Status">
                  <span className={`badge ${STATUS_BADGE[q.status]}`}>{statusLabel(q.status)}</span>
                </td>
                <td data-label="Ações" className="table-actions">
                  {q.status === 'Sent' && (
                    <>
                      <button className="btn btn-primary" onClick={() => handleApprove(q)}>
                        Aprovar
                      </button>
                      <button className="btn btn-danger" onClick={() => handleReject(q)}>
                        Rejeitar
                      </button>
                    </>
                  )}
                  {q.status === 'Approved' && (
                    <button className="btn btn-primary" onClick={() => handleGenerateOrder(q)}>
                      Gerar OT
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
            <h2>Nova cotação</h2>

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
                Tipo de veículo necessário
                <select value={form.requiredVehicleType} onChange={(e) => setForm({ ...form, requiredVehicleType: e.target.value as CreateFreightQuoteRequest['requiredVehicleType'] })}>
                  {VEHICLE_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
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
                Tipo de carga
                <select value={form.cargoType} onChange={(e) => setForm({ ...form, cargoType: e.target.value as CreateFreightQuoteRequest['cargoType'] })}>
                  {CARGO_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
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
                Distância estimada (km)
                <input type="number" value={form.estimatedDistanceKm} onChange={(e) => setForm({ ...form, estimatedDistanceKm: Number(e.target.value) })} />
              </label>
              <label>
                Outros custos (R$)
                <input type="number" value={form.otherCostsValue} onChange={(e) => setForm({ ...form, otherCostsValue: Number(e.target.value) })} />
              </label>
              <label>
                Válida por (dias)
                <input type="number" value={form.validForDays} onChange={(e) => setForm({ ...form, validForDays: Number(e.target.value) })} />
              </label>
            </div>

            <fieldset>
              <legend>Cálculo</legend>
              {previewError ? (
                <p className="form-error" style={{ margin: 0 }}>
                  {previewError}
                </p>
              ) : preview ? (
                <div className="quote-breakdown">
                  <div>
                    <span>Frete peso/km</span>
                    <span>{currency(preview.freightWeightValue)}</span>
                  </div>
                  <div>
                    <span>Pedágio</span>
                    <span>{currency(preview.tollValue)}</span>
                  </div>
                  <div>
                    <span>Ad valorem</span>
                    <span>{currency(preview.adValoremValue)}</span>
                  </div>
                  <div>
                    <span>GRIS</span>
                    <span>{currency(preview.grisValue)}</span>
                  </div>
                  <div>
                    <span>Outros custos</span>
                    <span>{currency(preview.otherCostsValue)}</span>
                  </div>
                  <div className="quote-breakdown-total">
                    <span>Total</span>
                    <span>{currency(preview.totalValue)}</span>
                  </div>
                </div>
              ) : (
                <p style={{ color: 'var(--text-muted)', margin: 0 }}>Preencha peso, valor da carga ou distância pra ver o cálculo.</p>
              )}
            </fieldset>

            <div className="modal-actions">
              <button className="btn" onClick={() => setFormOpen(false)}>
                Cancelar
              </button>
              <button className="btn btn-primary" disabled={saving || !form.customerCompanyId || !preview} onClick={handleSave}>
                {saving ? 'Criando...' : 'Criar cotação'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
