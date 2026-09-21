// Nome apresentável pra cada valor de status vindo da Api (que usa o nome
// literal do enum C#, ex. "EmManutencao") -- um mapa só, compartilhado por
// todas as telas, porque nenhum valor colide entre os diferentes status do
// domínio (Cancelada/Vencido/Aberta significam a mesma coisa em qualquer
// tela onde aparecem).
const STATUS_LABELS: Record<string, string> = {
  // Veículos / Motoristas
  Disponivel: 'Disponível',
  EmViagem: 'Em viagem',
  EmManutencao: 'Em manutenção',
  Indisponivel: 'Indisponível',
  Ferias: 'Férias',
  Afastado: 'Afastado',
  Inativo: 'Inativo',

  // Documentos
  Valido: 'Válido',
  ProximoVencimento: 'Próximo do vencimento',
  Vencido: 'Vencido',

  // Cotações de frete
  Draft: 'Rascunho',
  Sent: 'Aguardando decisão',
  Approved: 'Aprovada',
  Rejected: 'Rejeitada',
  Expired: 'Expirada',

  // Ordens de transporte / Viagens
  Criada: 'Criada',
  Programada: 'Programada',
  EmTransporte: 'Em transporte',
  Entregue: 'Entregue',
  Faturada: 'Faturada',
  EmAndamento: 'Em andamento',
  Concluida: 'Concluída',
  Cancelada: 'Cancelada',

  // Ocorrências / Manutenção
  Aberta: 'Aberta',
  Resolvida: 'Resolvida',
  EmExecucao: 'Em execução',

  // Financeiro
  Pendente: 'Pendente',
  Pago: 'Pago',
  Recebido: 'Recebido',
  Cancelado: 'Cancelado',
}

export function statusLabel(status: string): string {
  return STATUS_LABELS[status] ?? status
}
