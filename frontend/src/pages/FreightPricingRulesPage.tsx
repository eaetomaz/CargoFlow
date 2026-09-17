import { useEffect, useState } from 'react'
import { api } from '../api/client'
import { CARGO_TYPES, VEHICLE_TYPES, type FreightPricingRule, type UpsertFreightPricingRuleRequest } from '../api/types'

const EMPTY_FORM: UpsertFreightPricingRuleRequest = {
  vehicleType: 'CaminhaoToco',
  cargoType: null,
  pricePerKg: 0,
  pricePerKm: 0,
  tollPerKm: 0,
  adValoremPercentage: 0,
  grisPercentage: 0,
  minimumFreightValue: 0,
  effectiveFrom: new Date().toISOString().slice(0, 10),
  effectiveTo: null,
}

export default function FreightPricingRulesPage() {
  const [rules, setRules] = useState<FreightPricingRule[]>([])
  const [loading, setLoading] = useState(true)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const [form, setForm] = useState<UpsertFreightPricingRuleRequest>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const [loadError, setLoadError] = useState<string | null>(null)

  const load = () =>
    api
      .listPricingRules()
      .then((data) => {
        setRules(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar a tabela de preços. Tente recarregar a página.'))
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

  const openEdit = (r: FreightPricingRule) => {
    setEditingId(r.id)
    setForm({ ...r })
    setError(null)
    setFormOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    setError(null)
    try {
      if (editingId) await api.updatePricingRule(editingId, form)
      else await api.createPricingRule(form)
      setFormOpen(false)
      await load()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar regra de preço.')
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (r: FreightPricingRule) => {
    if (!confirm(`Remover a regra de preço de ${r.vehicleType}?`)) return
    await api.deletePricingRule(r.id)
    await load()
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Tabela de preços</h1>
        <button className="btn btn-primary" onClick={openCreate}>
          Nova regra
        </button>
      </div>
      <p style={{ color: 'var(--text-muted)', marginTop: -4 }}>
        Usada pelo cálculo de cotação de frete. Regra específica de tipo de carga tem prioridade sobre a genérica pro mesmo veículo.
      </p>

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : rules.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhuma regra cadastrada ainda.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Veículo</th>
              <th>Carga</th>
              <th>R$/kg</th>
              <th>R$/km</th>
              <th>Pedágio R$/km</th>
              <th>Ad valorem</th>
              <th>GRIS</th>
              <th>Mínimo</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {rules.map((r) => (
              <tr key={r.id}>
                <td data-label="Veículo">{r.vehicleType}</td>
                <td data-label="Carga">{r.cargoType ?? 'Qualquer'}</td>
                <td data-label="R$/kg">{r.pricePerKg.toFixed(4)}</td>
                <td data-label="R$/km">{r.pricePerKm.toFixed(4)}</td>
                <td data-label="Pedágio R$/km">{r.tollPerKm.toFixed(4)}</td>
                <td data-label="Ad valorem">{r.adValoremPercentage}%</td>
                <td data-label="GRIS">{r.grisPercentage}%</td>
                <td data-label="Mínimo">R$ {r.minimumFreightValue.toFixed(2)}</td>
                <td data-label="Ações" className="table-actions">
                  <button className="btn" onClick={() => openEdit(r)}>
                    Editar
                  </button>
                  <button className="btn btn-danger" onClick={() => handleDelete(r)}>
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
            <h2>{editingId ? 'Editar regra de preço' : 'Nova regra de preço'}</h2>

            {error && <p className="form-error">{error}</p>}

            <div className="form-grid-2col">
              <label>
                Tipo de veículo
                <select value={form.vehicleType} onChange={(e) => setForm({ ...form, vehicleType: e.target.value as UpsertFreightPricingRuleRequest['vehicleType'] })}>
                  {VEHICLE_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Tipo de carga (vazio = qualquer)
                <select
                  value={form.cargoType ?? ''}
                  onChange={(e) => setForm({ ...form, cargoType: e.target.value ? (e.target.value as UpsertFreightPricingRuleRequest['cargoType']) : null })}
                >
                  <option value="">Qualquer</option>
                  {CARGO_TYPES.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Preço por kg (R$)
                <input type="number" step="0.0001" value={form.pricePerKg} onChange={(e) => setForm({ ...form, pricePerKg: Number(e.target.value) })} />
              </label>
              <label>
                Preço por km (R$)
                <input type="number" step="0.0001" value={form.pricePerKm} onChange={(e) => setForm({ ...form, pricePerKm: Number(e.target.value) })} />
              </label>
              <label>
                Pedágio por km (R$)
                <input type="number" step="0.0001" value={form.tollPerKm} onChange={(e) => setForm({ ...form, tollPerKm: Number(e.target.value) })} />
              </label>
              <label>
                Ad valorem (%)
                <input type="number" step="0.01" value={form.adValoremPercentage} onChange={(e) => setForm({ ...form, adValoremPercentage: Number(e.target.value) })} />
              </label>
              <label>
                GRIS (%)
                <input type="number" step="0.01" value={form.grisPercentage} onChange={(e) => setForm({ ...form, grisPercentage: Number(e.target.value) })} />
              </label>
              <label>
                Valor mínimo do frete (R$)
                <input type="number" step="0.01" value={form.minimumFreightValue} onChange={(e) => setForm({ ...form, minimumFreightValue: Number(e.target.value) })} />
              </label>
              <label>
                Vigência início
                <input type="date" value={form.effectiveFrom} onChange={(e) => setForm({ ...form, effectiveFrom: e.target.value })} />
              </label>
              <label>
                Vigência fim (vazio = sem prazo)
                <input type="date" value={form.effectiveTo ?? ''} onChange={(e) => setForm({ ...form, effectiveTo: e.target.value || null })} />
              </label>
            </div>

            <div className="modal-actions">
              <button className="btn" onClick={() => setFormOpen(false)}>
                Cancelar
              </button>
              <button className="btn btn-primary" disabled={saving} onClick={handleSave}>
                {saving ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
