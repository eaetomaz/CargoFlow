import type {
  AccountPayable,
  AccountReceivable,
  AddTripExpenseRequest,
  AuditLog,
  CalculateFreightQuoteRequest,
  Company,
  CreateAccountPayableRequest,
  CreateAccountReceivableRequest,
  Dashboard,
  CompleteMaintenanceOrderRequest,
  CreateFreightQuoteRequest,
  CreateFuelingRequest,
  CreateMaintenanceOrderRequest,
  CreateOccurrenceRequest,
  CreateTransportOrderRequest,
  Document,
  Driver,
  FreightPricingRule,
  FreightQuote,
  FreightQuoteBreakdown,
  FinanceSummary,
  FuelEfficiency,
  Fueling,
  LoginResponse,
  MaintenanceOrder,
  Occurrence,
  ScheduleTripRequest,
  SimulationStatus,
  TransportOrder,
  Trip,
  TripProfitability,
  UpsertCompanyRequest,
  UpsertDocumentRequest,
  UpsertDriverRequest,
  UpsertFreightPricingRuleRequest,
  UpsertVehicleRequest,
  User,
  Vehicle,
} from './types'

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7099'
const TOKEN_STORAGE_KEY = 'cargoflow_token'

export const auth = {
  getToken: () => localStorage.getItem(TOKEN_STORAGE_KEY),
  setToken: (token: string) => localStorage.setItem(TOKEN_STORAGE_KEY, token),
  clearToken: () => localStorage.removeItem(TOKEN_STORAGE_KEY),
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const token = auth.getToken()
  const response = await fetch(`${BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    ...options,
  })

  if (response.status === 401) {
    auth.clearToken()
    window.location.href = '/login'
    throw new Error('Sessão expirada.')
  }

  if (!response.ok) {
    const body = await response.text()
    // Erros de validação (400) voltam como { message: "..." } -- extrai a
    // mensagem legível em vez de mostrar o JSON cru pro usuário.
    let readableMessage: string | null = null
    try {
      const parsed = JSON.parse(body) as { message?: string }
      readableMessage = parsed.message ?? null
    } catch {
      // corpo não era JSON com "message" -- cai pro erro genérico abaixo
    }
    throw new Error(readableMessage ?? `Erro ${response.status} em ${path}: ${body}`)
  }

  return response.status === 204 ? (undefined as T) : ((await response.json()) as T)
}

export const api = {
  health: () => fetch(`${BASE_URL}/health`).then((r) => r.json() as Promise<{ status: string }>),

  login: (username: string, password: string) =>
    fetch(`${BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password }),
    }).then(async (response) => {
      if (!response.ok) throw new Error('Usuário ou senha inválidos.')
      return response.json() as Promise<LoginResponse>
    }),

  me: () => request<User>('/api/auth/me'),

  listCompanies: () => request<Company[]>('/api/companies'),
  getCompany: (id: string) => request<Company>(`/api/companies/${id}`),
  createCompany: (body: UpsertCompanyRequest) =>
    request<Company>('/api/companies', { method: 'POST', body: JSON.stringify(body) }),
  updateCompany: (id: string, body: UpsertCompanyRequest) =>
    request<Company>(`/api/companies/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteCompany: (id: string) => request<void>(`/api/companies/${id}`, { method: 'DELETE' }),

  listVehicles: () => request<Vehicle[]>('/api/vehicles'),
  getVehicle: (id: string) => request<Vehicle>(`/api/vehicles/${id}`),
  createVehicle: (body: UpsertVehicleRequest) =>
    request<Vehicle>('/api/vehicles', { method: 'POST', body: JSON.stringify(body) }),
  updateVehicle: (id: string, body: UpsertVehicleRequest) =>
    request<Vehicle>(`/api/vehicles/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteVehicle: (id: string) => request<void>(`/api/vehicles/${id}`, { method: 'DELETE' }),

  listDrivers: () => request<Driver[]>('/api/drivers'),
  getDriver: (id: string) => request<Driver>(`/api/drivers/${id}`),
  createDriver: (body: UpsertDriverRequest) =>
    request<Driver>('/api/drivers', { method: 'POST', body: JSON.stringify(body) }),
  updateDriver: (id: string, body: UpsertDriverRequest) =>
    request<Driver>(`/api/drivers/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteDriver: (id: string) => request<void>(`/api/drivers/${id}`, { method: 'DELETE' }),

  listDocuments: () => request<Document[]>('/api/documents'),
  listExpiringDocuments: () => request<Document[]>('/api/documents/expiring'),
  getDocument: (id: string) => request<Document>(`/api/documents/${id}`),
  createDocument: (body: UpsertDocumentRequest) =>
    request<Document>('/api/documents', { method: 'POST', body: JSON.stringify(body) }),
  updateDocument: (id: string, body: UpsertDocumentRequest) =>
    request<Document>(`/api/documents/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteDocument: (id: string) => request<void>(`/api/documents/${id}`, { method: 'DELETE' }),

  listPricingRules: () => request<FreightPricingRule[]>('/api/freight-pricing-rules'),
  createPricingRule: (body: UpsertFreightPricingRuleRequest) =>
    request<FreightPricingRule>('/api/freight-pricing-rules', { method: 'POST', body: JSON.stringify(body) }),
  updatePricingRule: (id: string, body: UpsertFreightPricingRuleRequest) =>
    request<FreightPricingRule>(`/api/freight-pricing-rules/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deletePricingRule: (id: string) => request<void>(`/api/freight-pricing-rules/${id}`, { method: 'DELETE' }),

  calculateFreightQuote: (body: CalculateFreightQuoteRequest) =>
    request<FreightQuoteBreakdown>('/api/freight-quotes/calculate', { method: 'POST', body: JSON.stringify(body) }),
  listFreightQuotes: () => request<FreightQuote[]>('/api/freight-quotes'),
  createFreightQuote: (body: CreateFreightQuoteRequest) =>
    request<FreightQuote>('/api/freight-quotes', { method: 'POST', body: JSON.stringify(body) }),
  approveFreightQuote: (id: string) => request<FreightQuote>(`/api/freight-quotes/${id}/approve`, { method: 'POST' }),
  rejectFreightQuote: (id: string) => request<FreightQuote>(`/api/freight-quotes/${id}/reject`, { method: 'POST' }),

  listTransportOrders: () => request<TransportOrder[]>('/api/transport-orders'),
  createTransportOrder: (body: CreateTransportOrderRequest) =>
    request<TransportOrder>('/api/transport-orders', { method: 'POST', body: JSON.stringify(body) }),
  createTransportOrderFromQuote: (freightQuoteId: string, body: { cargoDescription: string; requestedPickupDate: string; requestedDeliveryDate: string }) =>
    request<TransportOrder>(`/api/transport-orders/from-quote/${freightQuoteId}`, { method: 'POST', body: JSON.stringify(body) }),
  cancelTransportOrder: (id: string) => request<TransportOrder>(`/api/transport-orders/${id}/cancel`, { method: 'POST' }),

  listTrips: () => request<Trip[]>('/api/trips'),
  getTrip: (id: string) => request<Trip>(`/api/trips/${id}`),
  scheduleTrip: (body: ScheduleTripRequest) => request<Trip>('/api/trips/schedule', { method: 'POST', body: JSON.stringify(body) }),
  startTrip: (id: string) => request<Trip>(`/api/trips/${id}/start`, { method: 'POST' }),
  addTripExpense: (id: string, body: AddTripExpenseRequest) =>
    request<Trip>(`/api/trips/${id}/expenses`, { method: 'POST', body: JSON.stringify(body) }),
  completeTripDelivery: (id: string, actualDistanceKm: number) =>
    request<Trip>(`/api/trips/${id}/complete-delivery`, { method: 'POST', body: JSON.stringify({ actualDistanceKm }) }),
  cancelTrip: (id: string) => request<Trip>(`/api/trips/${id}/cancel`, { method: 'POST' }),
  getTripProfitability: (id: string) => request<TripProfitability>(`/api/trips/${id}/profitability`),

  listOccurrences: () => request<Occurrence[]>('/api/occurrences'),
  getOccurrence: (id: string) => request<Occurrence>(`/api/occurrences/${id}`),
  createOccurrence: (body: CreateOccurrenceRequest) =>
    request<Occurrence>('/api/occurrences', { method: 'POST', body: JSON.stringify(body) }),
  resolveOccurrence: (id: string) => request<Occurrence>(`/api/occurrences/${id}/resolve`, { method: 'POST' }),
  cancelOccurrence: (id: string) => request<Occurrence>(`/api/occurrences/${id}/cancel`, { method: 'POST' }),
  addOccurrenceAttachment: (id: string, body: { fileUrl: string; fileName: string }) =>
    request<Occurrence>(`/api/occurrences/${id}/attachments`, { method: 'POST', body: JSON.stringify(body) }),

  listFuelings: () => request<Fueling[]>('/api/fuelings'),
  getFueling: (id: string) => request<Fueling>(`/api/fuelings/${id}`),
  createFueling: (body: CreateFuelingRequest) =>
    request<Fueling>('/api/fuelings', { method: 'POST', body: JSON.stringify(body) }),
  getFuelEfficiency: (vehicleId: string) => request<FuelEfficiency>(`/api/vehicles/${vehicleId}/fuel-efficiency`),

  listMaintenanceOrders: () => request<MaintenanceOrder[]>('/api/maintenance-orders'),
  getMaintenanceOrder: (id: string) => request<MaintenanceOrder>(`/api/maintenance-orders/${id}`),
  createMaintenanceOrder: (body: CreateMaintenanceOrderRequest) =>
    request<MaintenanceOrder>('/api/maintenance-orders', { method: 'POST', body: JSON.stringify(body) }),
  startMaintenanceOrder: (id: string) => request<MaintenanceOrder>(`/api/maintenance-orders/${id}/start`, { method: 'POST' }),
  completeMaintenanceOrder: (id: string, body: CompleteMaintenanceOrderRequest) =>
    request<MaintenanceOrder>(`/api/maintenance-orders/${id}/complete`, { method: 'POST', body: JSON.stringify(body) }),

  listAccountsPayable: () => request<AccountPayable[]>('/api/accounts-payable'),
  createAccountPayable: (body: CreateAccountPayableRequest) =>
    request<AccountPayable>('/api/accounts-payable', { method: 'POST', body: JSON.stringify(body) }),
  payAccountPayable: (id: string) => request<AccountPayable>(`/api/accounts-payable/${id}/pay`, { method: 'POST' }),
  cancelAccountPayable: (id: string) => request<AccountPayable>(`/api/accounts-payable/${id}/cancel`, { method: 'POST' }),

  listAccountsReceivable: () => request<AccountReceivable[]>('/api/accounts-receivable'),
  createAccountReceivable: (body: CreateAccountReceivableRequest) =>
    request<AccountReceivable>('/api/accounts-receivable', { method: 'POST', body: JSON.stringify(body) }),
  receiveAccountReceivable: (id: string) => request<AccountReceivable>(`/api/accounts-receivable/${id}/receive`, { method: 'POST' }),
  cancelAccountReceivable: (id: string) => request<AccountReceivable>(`/api/accounts-receivable/${id}/cancel`, { method: 'POST' }),

  getFinanceSummary: () => request<FinanceSummary>('/api/finance/summary'),

  getDashboard: () => request<Dashboard>('/api/dashboard'),

  listAuditLogs: (entity?: string, entityId?: string) => {
    const params = new URLSearchParams()
    if (entity) params.set('entity', entity)
    if (entityId) params.set('entityId', entityId)
    const query = params.toString()
    return request<AuditLog[]>(`/api/audit-logs${query ? `?${query}` : ''}`)
  },
  cancelMaintenanceOrder: (id: string) => request<MaintenanceOrder>(`/api/maintenance-orders/${id}/cancel`, { method: 'POST' }),

  startSimulation: (speedMultiplier?: number) =>
    request<SimulationStatus>('/api/simulation/start', { method: 'POST', body: JSON.stringify({ speedMultiplier: speedMultiplier ?? null }) }),
  stopSimulation: () => request<SimulationStatus>('/api/simulation/stop', { method: 'POST' }),
  getSimulationStatus: () => request<SimulationStatus>('/api/simulation/status'),
}

// Exportado à parte (não é um Promise que resolve uma vez, é um stream) --
// consumido pelo hook useSimulationStream. fetch em vez de EventSource
// nativo porque EventSource não permite mandar Authorization: Bearer.
export function openSimulationStream(): { response: Promise<Response>; abort: () => void } {
  const controller = new AbortController()
  const token = auth.getToken()

  const response = fetch(`${BASE_URL}/api/simulation/stream`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    signal: controller.signal,
  })

  return { response, abort: () => controller.abort() }
}
