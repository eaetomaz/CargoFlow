export type UserRole = 'Administrador' | 'Operacional' | 'Financeiro' | 'Manutencao' | 'Motorista'

export interface User {
  id: string
  username: string
  email: string
  fullName: string
  role: UserRole
  driverId: string | null
}

export interface LoginResponse {
  token: string
  expiresAt: string
  user: User
}

export const COMPANY_ROLES = ['Cliente', 'Embarcador', 'Destinatario', 'Fornecedor', 'Parceiro'] as const
export type CompanyRole = (typeof COMPANY_ROLES)[number]

export const ADDRESS_TYPES = ['Principal', 'Cobranca', 'Entrega'] as const
export type AddressType = (typeof ADDRESS_TYPES)[number]

export interface Address {
  id: string
  type: AddressType
  street: string
  number: string
  complement: string | null
  district: string
  city: string
  state: string
  zipCode: string
  isDefault: boolean
}

export interface Contact {
  id: string
  name: string
  role: string | null
  email: string | null
  phone: string | null
}

export interface Company {
  id: string
  name: string
  tradeName: string | null
  document: string
  documentType: 'Cnpj' | 'Cpf'
  stateRegistration: string | null
  email: string | null
  phone: string | null
  roles: CompanyRole[]
  isActive: boolean
  addresses: Address[]
  contacts: Contact[]
}

export type UpsertAddressRequest = Omit<Address, 'id'>
export type UpsertContactRequest = Omit<Contact, 'id'>

export interface UpsertCompanyRequest {
  name: string
  tradeName: string | null
  document: string
  documentType: 'Cnpj' | 'Cpf'
  stateRegistration: string | null
  email: string | null
  phone: string | null
  roles: CompanyRole[]
  isActive: boolean
  addresses: UpsertAddressRequest[]
  contacts: UpsertContactRequest[]
}

// Fleet (Vehicles)

export const VEHICLE_TYPES = ['Cavalo', 'CaminhaoToco', 'CaminhaoTruck', 'Carreta', 'Bitrem', 'Rodotrem'] as const
export type VehicleType = (typeof VEHICLE_TYPES)[number]

export const VEHICLE_STATUSES = ['Disponivel', 'EmViagem', 'EmManutencao', 'Indisponivel'] as const
export type VehicleStatus = (typeof VEHICLE_STATUSES)[number]

export interface Vehicle {
  id: string
  plateNumber: string
  renavam: string
  brand: string
  model: string
  manufactureYear: number
  modelYear: number
  type: VehicleType
  axleCount: number
  capacityKg: number
  tareWeightKg: number
  odometer: number
  status: VehicleStatus
}

export type UpsertVehicleRequest = Omit<Vehicle, 'id'>

// Drivers

export const CNH_CATEGORIES = ['A', 'B', 'C', 'D', 'E'] as const
export type CnhCategory = (typeof CNH_CATEGORIES)[number]

export const EMPLOYMENT_TYPES = ['Clt', 'Autonomo', 'Agregado'] as const
export type EmploymentType = (typeof EMPLOYMENT_TYPES)[number]

export const DRIVER_AVAILABILITY_STATUSES = ['Disponivel', 'EmViagem', 'Ferias', 'Afastado', 'Inativo'] as const
export type DriverAvailabilityStatus = (typeof DRIVER_AVAILABILITY_STATUSES)[number]

export interface Driver {
  id: string
  name: string
  cpf: string
  birthDate: string
  phone: string | null
  email: string | null
  cnhNumber: string
  cnhCategory: CnhCategory
  cnhExpiryDate: string
  hireDate: string
  employmentType: EmploymentType
  availabilityStatus: DriverAvailabilityStatus
}

export type UpsertDriverRequest = Omit<Driver, 'id'>

// Documents

export const DOCUMENT_OWNER_TYPES = ['Vehicle', 'Driver', 'Company'] as const
export type DocumentOwnerType = (typeof DOCUMENT_OWNER_TYPES)[number]

export const DOCUMENT_TYPES = ['Cnh', 'Crlv', 'ApoliceSeguro', 'Licenciamento', 'Antt', 'ContratoTransporte', 'Outro'] as const
export type DocumentType = (typeof DOCUMENT_TYPES)[number]

export const DOCUMENT_STATUSES = ['Valido', 'ProximoVencimento', 'Vencido'] as const
export type DocumentStatus = (typeof DOCUMENT_STATUSES)[number]

export interface Document {
  id: string
  ownerType: DocumentOwnerType
  ownerId: string
  type: DocumentType
  number: string
  issueDate: string
  expiryDate: string
  status: DocumentStatus
  attachmentUrl: string | null
}

export type UpsertDocumentRequest = Omit<Document, 'id' | 'status'>

// Freight (Cotação)

export const CARGO_TYPES = ['Geral', 'Perecivel', 'Fragil', 'Perigosa', 'Granel', 'Refrigerada', 'Container'] as const
export type CargoType = (typeof CARGO_TYPES)[number]

export interface FreightPricingRule {
  id: string
  vehicleType: VehicleType
  cargoType: CargoType | null
  pricePerKg: number
  pricePerKm: number
  tollPerKm: number
  adValoremPercentage: number
  grisPercentage: number
  minimumFreightValue: number
  effectiveFrom: string
  effectiveTo: string | null
}

export type UpsertFreightPricingRuleRequest = Omit<FreightPricingRule, 'id'>

export interface FreightQuoteBreakdown {
  freightWeightValue: number
  tollValue: number
  adValoremValue: number
  grisValue: number
  otherCostsValue: number
  totalValue: number
}

export const FREIGHT_QUOTE_STATUSES = ['Draft', 'Sent', 'Approved', 'Rejected', 'Expired'] as const
export type FreightQuoteStatus = (typeof FREIGHT_QUOTE_STATUSES)[number]

export interface FreightQuote {
  id: string
  customerCompanyId: string
  customerCompanyName: string
  originCity: string
  originState: string
  destinationCity: string
  destinationState: string
  cargoType: CargoType
  cargoWeightKg: number
  cargoValue: number
  requiredVehicleType: VehicleType
  estimatedDistanceKm: number
  freightWeightValue: number
  tollValue: number
  adValoremValue: number
  grisValue: number
  otherCostsValue: number
  totalValue: number
  status: FreightQuoteStatus
  expiresAt: string
  createdAt: string
}

export interface CalculateFreightQuoteRequest {
  cargoType: CargoType
  cargoWeightKg: number
  cargoValue: number
  requiredVehicleType: VehicleType
  estimatedDistanceKm: number
  otherCostsValue: number
}

export interface CreateFreightQuoteRequest extends CalculateFreightQuoteRequest {
  customerCompanyId: string
  originCity: string
  originState: string
  destinationCity: string
  destinationState: string
  validForDays: number
}

// Transport Orders

export const TRANSPORT_ORDER_STATUSES = ['Criada', 'Programada', 'EmTransporte', 'Entregue', 'Faturada', 'Cancelada'] as const
export type TransportOrderStatus = (typeof TRANSPORT_ORDER_STATUSES)[number]

export interface TransportOrder {
  id: string
  freightQuoteId: string | null
  customerCompanyId: string
  customerCompanyName: string
  shipperCompanyId: string | null
  consigneeCompanyId: string | null
  originCity: string
  originState: string
  destinationCity: string
  destinationState: string
  cargoDescription: string
  cargoWeightKg: number
  cargoValue: number
  freightValue: number
  status: TransportOrderStatus
  requestedPickupDate: string
  requestedDeliveryDate: string
}

export interface CreateTransportOrderRequest {
  customerCompanyId: string
  shipperCompanyId: string | null
  consigneeCompanyId: string | null
  originCity: string
  originState: string
  destinationCity: string
  destinationState: string
  cargoDescription: string
  cargoWeightKg: number
  cargoValue: number
  freightValue: number
  requestedPickupDate: string
  requestedDeliveryDate: string
}

// Trips

export const TRIP_STATUSES = ['Programada', 'EmAndamento', 'Concluida', 'Cancelada'] as const
export type TripStatus = (typeof TRIP_STATUSES)[number]

export const TRIP_EXPENSE_TYPES = ['Pedagio', 'Diaria', 'Manutencao', 'Diversos'] as const
export type TripExpenseType = (typeof TRIP_EXPENSE_TYPES)[number]

export interface TripEvent {
  id: string
  type: string
  occurredAt: string
  description: string
  source: string
}

export interface TripExpense {
  id: string
  type: TripExpenseType
  description: string | null
  value: number
  expenseDate: string
}

export interface Trip {
  id: string
  transportOrderId: string
  vehicleId: string
  vehiclePlate: string
  trailerVehicleId: string | null
  driverId: string
  driverName: string
  originCity: string
  originState: string
  destinationCity: string
  destinationState: string
  scheduledDepartureAt: string
  actualDepartureAt: string | null
  estimatedArrivalAt: string
  actualArrivalAt: string | null
  plannedDistanceKm: number
  actualDistanceKm: number | null
  status: TripStatus
  freightRevenue: number
  expenses: TripExpense[]
  events: TripEvent[]
}

export interface ScheduleTripRequest {
  transportOrderId: string
  vehicleId: string
  trailerVehicleId: string | null
  driverId: string
  scheduledDepartureAt: string
  estimatedArrivalAt: string
  plannedDistanceKm: number
}

export interface AddTripExpenseRequest {
  type: TripExpenseType
  description: string | null
  value: number
  expenseDate: string
}

export interface TripProfitability {
  revenue: number
  totalCost: number
  margin: number
  marginPercentage: number | null
  costPerKm: number | null
}

// Occurrences

export const OCCURRENCE_TYPES = ['PneuFurado', 'Acidente', 'Atraso', 'Quebra', 'ClienteAusente', 'CargaAvariada', 'DocumentacaoIrregular'] as const
export type OccurrenceType = (typeof OCCURRENCE_TYPES)[number]

export const OCCURRENCE_STATUSES = ['Aberta', 'EmAndamento', 'Resolvida', 'Cancelada'] as const
export type OccurrenceStatus = (typeof OCCURRENCE_STATUSES)[number]

export interface OccurrenceAttachment {
  id: string
  fileUrl: string
  fileName: string
  uploadedAt: string
}

export interface Occurrence {
  id: string
  tripId: string | null
  vehicleId: string
  vehiclePlate: string
  driverId: string
  driverName: string
  type: OccurrenceType
  occurredAt: string
  location: string
  description: string
  responsibleUserId: string | null
  status: OccurrenceStatus
  resolvedAt: string | null
  attachments: OccurrenceAttachment[]
}

export interface CreateOccurrenceRequest {
  tripId: string | null
  vehicleId: string
  driverId: string
  type: OccurrenceType
  occurredAt: string
  location: string
  description: string
  responsibleUserId: string | null
}

// Fuelings

export interface Fueling {
  id: string
  vehicleId: string
  vehiclePlate: string
  driverId: string | null
  driverName: string | null
  tripId: string | null
  gasStationName: string
  fuelingDate: string
  literQuantity: number
  totalValue: number
  odometerReading: number
}

export interface CreateFuelingRequest {
  vehicleId: string
  driverId: string | null
  tripId: string | null
  gasStationName: string
  fuelingDate: string
  literQuantity: number
  totalValue: number
  odometerReading: number
}

export interface FuelEfficiency {
  vehicleId: string
  kmPerLiter: number | null
  costPerKm: number | null
}

// Maintenance

export const MAINTENANCE_TYPES = ['Preventiva', 'Corretiva'] as const
export type MaintenanceType = (typeof MAINTENANCE_TYPES)[number]

export const MAINTENANCE_ORDER_STATUSES = ['Aberta', 'EmExecucao', 'Concluida', 'Cancelada'] as const
export type MaintenanceOrderStatus = (typeof MAINTENANCE_ORDER_STATUSES)[number]

export interface MaintenanceOrder {
  id: string
  vehicleId: string
  vehiclePlate: string
  type: MaintenanceType
  description: string
  status: MaintenanceOrderStatus
  openedAt: string
  scheduledDate: string | null
  startedAt: string | null
  completedAt: string | null
  odometerAtService: number | null
  cost: number | null
  serviceProvider: string | null
}

export interface CreateMaintenanceOrderRequest {
  vehicleId: string
  type: MaintenanceType
  description: string
  scheduledDate: string | null
  serviceProvider: string | null
}

export interface CompleteMaintenanceOrderRequest {
  cost: number
  odometerAtService: number | null
}

// Finance

export const ACCOUNT_PAYABLE_CATEGORIES = ['Combustivel', 'Manutencao', 'Pedagio', 'Fornecedor', 'Despesa'] as const
export type AccountPayableCategory = (typeof ACCOUNT_PAYABLE_CATEGORIES)[number]

export const ACCOUNT_STATUSES = ['Pendente', 'Pago', 'Vencido', 'Cancelado'] as const
export type AccountPayableStatus = (typeof ACCOUNT_STATUSES)[number]

export const RECEIVABLE_STATUSES = ['Pendente', 'Recebido', 'Vencido', 'Cancelado'] as const
export type AccountReceivableStatus = (typeof RECEIVABLE_STATUSES)[number]

export interface AccountPayable {
  id: string
  category: AccountPayableCategory
  description: string
  supplierCompanyId: string | null
  supplierCompanyName: string | null
  sourceType: string | null
  sourceId: string | null
  amount: number
  dueDate: string
  paidDate: string | null
  status: AccountPayableStatus
}

export interface CreateAccountPayableRequest {
  category: AccountPayableCategory
  description: string
  supplierCompanyId: string | null
  sourceType: string | null
  sourceId: string | null
  amount: number
  dueDate: string
}

export interface AccountReceivable {
  id: string
  customerCompanyId: string
  customerCompanyName: string
  tripId: string | null
  transportOrderId: string | null
  amount: number
  dueDate: string
  receivedDate: string | null
  status: AccountReceivableStatus
}

export interface CreateAccountReceivableRequest {
  customerCompanyId: string
  tripId: string | null
  transportOrderId: string | null
  amount: number
  dueDate: string
}

export interface FinanceSummary {
  totalReceived: number
  totalReceivablePending: number
  totalReceivableOverdue: number
  totalPaid: number
  totalPayablePending: number
  totalPayableOverdue: number
  netMargin: number
}

// Audit

export interface AuditLog {
  id: string
  userId: string | null
  userName: string
  entityName: string
  entityId: string
  operation: 'Create' | 'Update' | 'Delete'
  oldValuesJson: string | null
  newValuesJson: string | null
  changedAt: string
  source: string
}

// Dashboard

export interface DailyRevenue {
  date: string
  revenue: number
  deliveries: number
}

export interface ExpiringDocumentSummary {
  id: string
  ownerType: string
  type: string
  expiryDate: string
  status: string
}

export interface OpenOccurrenceSummary {
  id: string
  vehiclePlate: string
  type: string
  occurredAt: string
  status: string
}

export interface Dashboard {
  activeTripsCount: number
  deliveriesTodayCount: number
  totalRevenue: number
  totalBilled: number
  aggregateMargin: number
  aggregateMarginPercentage: number | null
  delayedTripsCount: number
  unavailableVehiclesCount: number
  documentsNearExpiryCount: number
  openOccurrencesCount: number
  fleetAverageFuelEfficiencyKmL: number | null
  averageCostPerKm: number | null
  revenueByDay: DailyRevenue[]
  expiringDocuments: ExpiringDocumentSummary[]
  openOccurrences: OpenOccurrenceSummary[]
}

// Simulação de operação

export type SimulationRunStatus = 'Idle' | 'Running' | 'Finished'

export interface SimulationStatus {
  status: SimulationRunStatus
  startedAtVirtual: string | null
  currentVirtualTime: string | null
  speedMultiplier: number
  scheduledTripCount: number
  completedTripCount: number
  pendingStepCount: number
}

export interface SimulationEvent {
  type: string
  occurredAtVirtual: string
  message: string
  tripId: string | null
}
