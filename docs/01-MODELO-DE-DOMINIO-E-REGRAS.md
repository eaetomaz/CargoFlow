# Modelo de domínio e regras de negócio

## Arquitetura em camadas (modular monolith)

```
CargoFlow.Api             -- Controllers, autenticação JWT, SSE, hosted services
CargoFlow.Application      -- Casos de uso, DTOs, regras de negócio, MediatR handlers
CargoFlow.Domain            -- Entidades ricas, enums, domain events (zero dependências)
CargoFlow.Infrastructure    -- EF Core, repositórios, seed de dados
CargoFlow.Application.Tests -- xUnit (37 testes)
frontend/                    -- React + TypeScript (Vite)
```

Organização por módulo dentro de cada camada — `Domain/Entities/Trips/`, `Application/Trips/`, `Infrastructure/Persistence/Repositories/Trips/` etc. Mesmo módulo, mesma pasta, em todas as camadas.

Sem projeto `Workers` separado (diferente do product-hunter) — o simulador precisa empurrar eventos ao vivo via SSE pro mesmo processo que serve a Api, então todo `BackgroundService` roda dentro do próprio `CargoFlow.Api`.

## Entidades principais

- **Company** (multi-papel via `[Flags] CompanyRoles`: Cliente/Embarcador/Destinatario/Fornecedor/Parceiro) + `Address` + `Contact`.
- **Vehicle** (placa/RENAVAM únicos, `VehicleType`, `VehicleStatus` Disponivel/EmViagem/EmManutencao/Indisponivel).
- **Driver** (CPF/CNH únicos, `CnhCategory`, `DriverAvailabilityStatus`).
- **Document** — polimórfico via `OwnerType`/`OwnerId` (cobre Vehicle/Driver/Company numa tabela só), `Status` (Válido/PróximoVencimento/Vencido) recalculado automaticamente.
- **FreightPricingRule** (por tipo de veículo + opcionalmente tipo de carga) + **FreightQuote** (snapshot do cálculo no momento da cotação — nunca recalcula se a regra de preço mudar depois).
- **TransportOrder** (Criada→Programada→EmTransporte→Entregue→Faturada, Cancelada de qualquer estado pré-Entregue).
- **Trip** — o único agregado com modelo realmente rico (guards + domain events via `AggregateRoot`), é o objeto central do domínio e o que o simulador movimenta. `TripExpense` + `TripEvent` (log append-only da timeline, com `Source`: Manual/Simulator/System).
- **Occurrence** (+ `OccurrenceAttachment`) — uma "Quebra" torna o veículo `EmManutencao` automaticamente.
- **Fueling** — valida coerência de odômetro, alimenta cálculo de km/l.
- **MaintenanceOrder** (Preventiva/Corretiva).
- **AccountPayable** / **AccountReceivable** — "faturamento" é literalmente a criação de um `AccountReceivable`, sem entidade de Invoice separada.
- **AuditLog** — genérico, criado via interceptor de `SaveChanges`.
- **User** (5 `UserRole` fixos, `DriverId` opcional pra ligar login de motorista ao cadastro dele).

## Regras de negócio → onde vivem

| Regra | Onde vive |
|---|---|
| Motorista com CNH vencida não pode ser escalado | `TripSchedulingRules.ValidateDriverEligibility` |
| Veículo indisponível não pode ser usado | `TripSchedulingRules.ValidateVehicleEligibility` |
| Veículo incompatível (capacidade) não pode ser programado | `TripSchedulingRules.ValidateVehicleCompatibility` |
| Documento crítico vencido bloqueia a operação | `DocumentComplianceService.HasBlockingExpiredDocumentsAsync` (chamado pelas 2 regras acima) |
| Viagem concluída/cancelada não recebe novas despesas | Guard no próprio `Trip` (rich domain model, não checagem em service) |
| Entrega concluída gera faturamento automaticamente | `DeliveryCompletedEvent` (MediatR) → `BillingGenerationHandler` |
| Custos da viagem compõem sua rentabilidade | `TripFinancialService.CalculateProfitability` — calculado sob demanda, nunca armazenado |
| Abastecimento respeita coerência de quilometragem | `FuelingOdometerValidator` |
| Evento duplicado não duplica efeito (idempotência) | `BillingGenerationHandler` checa se já existe `AccountReceivable` pro `TripId` antes de criar |
| Operação financeira crítica é auditável | Interceptor de `SaveChanges` no `CargoFlowDbContext`, varre `IAuditable` |

## Domain events (vocabulário MediatR)

`TripScheduledEvent`, `TripStartedEvent`, `DeliveryCompletedEvent`, `TripCancelledEvent` — definidos no **Domain** (não na Application), referenciando só `MediatR.Contracts` (as interfaces, não a implementação completa) pra manter o Domain com zero dependência de infraestrutura. Os handlers (que reagem a esses eventos, como `BillingGenerationHandler`) ficam na Application.

`Trip.Schedule/Start/CompleteDelivery/Cancel` aceitam um parâmetro opcional `TripEventSource source = Manual` — é isso que permite ao simulador marcar seus próprios eventos como `Source: Simulator` na timeline, sem duplicar lógica.

## Auth/RBAC

JWT bearer (não Identity completo — 5 perfis fixos não justificam o peso do framework inteiro). `PasswordHasher<User>` nativo do ASP.NET Core. Política padrão nega tudo (`[Authorize]` global), cada controller opta pelos perfis que aceita. Financeiro e Auditoria são visíveis só pra Administrador/Financeiro e Administrador respectivamente (tanto no backend quanto escondidos da sidebar no frontend).
