export interface MiniKpi {
  label: string
  value: string
  tone?: 'default' | 'success' | 'warning' | 'danger'
}

// Faixa de indicadores rápidos no topo de cada tela de listagem -- deriva
// os números direto da lista já carregada (sem chamada extra à Api), só
// pra dar uma leitura imediata do que tem ali antes de rolar a tabela.
export function KpiStrip({ items }: { items: MiniKpi[] }) {
  return (
    <div className="mini-kpi-row">
      {items.map((item) => (
        <div key={item.label} className={`mini-kpi mini-kpi-${item.tone ?? 'default'}`}>
          <span className="mini-kpi-value">{item.value}</span>
          <span className="mini-kpi-label">{item.label}</span>
        </div>
      ))}
    </div>
  )
}
