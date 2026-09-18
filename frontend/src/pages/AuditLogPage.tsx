import { Fragment, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { AuditLog } from '../api/types'

const OPERATION_BADGE: Record<string, string> = {
  Create: 'badge-success',
  Update: 'badge-warning',
  Delete: 'badge-danger',
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString('pt-BR')
}

export default function AuditLogPage() {
  const [logs, setLogs] = useState<AuditLog[]>([])
  const [loading, setLoading] = useState(true)
  const [entityFilter, setEntityFilter] = useState('')
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const [loadError, setLoadError] = useState<string | null>(null)

  const load = (entity?: string) => {
    setLoading(true)
    api
      .listAuditLogs(entity || undefined)
      .then((data) => {
        setLogs(data)
        setLoadError(null)
      })
      .catch(() => setLoadError('Não foi possível carregar a auditoria. Tente recarregar a página.'))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    load()
  }, [])

  const handleFilter = (e: React.FormEvent) => {
    e.preventDefault()
    load(entityFilter)
  }

  return (
    <div>
      <h1>Auditoria</h1>
      <p style={{ color: 'var(--text-muted)', marginTop: -4 }}>
        Toda mudança em entidade auditável (Empresas, Frota, Motoristas, Documentos, Cotações, OTs, Viagens, Financeiro...) é registrada aqui automaticamente.
      </p>

      <form onSubmit={handleFilter} style={{ display: 'flex', gap: 8, marginBottom: 16 }}>
        <input
          placeholder="Filtrar por entidade (ex: Trip, Company, AccountReceivable)"
          value={entityFilter}
          onChange={(e) => setEntityFilter(e.target.value)}
          style={{ flex: '1 1 300px', padding: 8, border: '1px solid var(--border)', borderRadius: 6, background: 'var(--surface)', color: 'var(--text)' }}
        />
        <button className="btn btn-primary" type="submit">
          Filtrar
        </button>
        {entityFilter && (
          <button
            className="btn"
            type="button"
            onClick={() => {
              setEntityFilter('')
              load()
            }}
          >
            Limpar
          </button>
        )}
      </form>

      {loadError && <p className="form-error">{loadError}</p>}

      {loading ? (
        <p>Carregando...</p>
      ) : logs.length === 0 ? (
        <p style={{ color: 'var(--text-muted)' }}>Nenhum registro encontrado.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Data/hora</th>
              <th>Usuário</th>
              <th>Entidade</th>
              <th>Operação</th>
              <th>Origem</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {logs.map((log) => (
              <Fragment key={log.id}>
                <tr>
                  <td data-label="Data/hora">{formatDateTime(log.changedAt)}</td>
                  <td data-label="Usuário">{log.userName}</td>
                  <td data-label="Entidade">
                    {log.entityName} <span style={{ color: 'var(--text-muted)', fontSize: 12 }}>#{log.entityId.slice(0, 8)}</span>
                  </td>
                  <td data-label="Operação">
                    <span className={`badge ${OPERATION_BADGE[log.operation]}`}>{log.operation}</span>
                  </td>
                  <td data-label="Origem">{log.source}</td>
                  <td data-label="Detalhes" className="table-actions">
                    <button className="btn" onClick={() => setExpandedId(expandedId === log.id ? null : log.id)}>
                      {expandedId === log.id ? 'Ocultar' : 'Ver valores'}
                    </button>
                  </td>
                </tr>
                {expandedId === log.id && (
                  <tr>
                    <td colSpan={6} style={{ background: 'var(--bg)' }}>
                      <div style={{ display: 'flex', gap: 16, flexWrap: 'wrap', padding: 8 }}>
                        {log.oldValuesJson && (
                          <div style={{ flex: '1 1 300px' }}>
                            <strong style={{ fontSize: 12, color: 'var(--text-muted)' }}>Antes</strong>
                            <pre className="audit-json">{JSON.stringify(JSON.parse(log.oldValuesJson), null, 2)}</pre>
                          </div>
                        )}
                        {log.newValuesJson && (
                          <div style={{ flex: '1 1 300px' }}>
                            <strong style={{ fontSize: 12, color: 'var(--text-muted)' }}>Depois</strong>
                            <pre className="audit-json">{JSON.stringify(JSON.parse(log.newValuesJson), null, 2)}</pre>
                          </div>
                        )}
                      </div>
                    </td>
                  </tr>
                )}
              </Fragment>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
