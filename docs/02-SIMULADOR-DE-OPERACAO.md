# Simulador de operação

O diferencial do projeto — o botão "Iniciar simulação" no Dashboard. Objetivo: demonstrar o sistema "vivo" sem precisar digitar tudo manualmente, mutando dado real via os mesmos serviços que a UI usa (nenhum caminho de dado falso paralelo).

## Como funciona, passo a passo

1. **`POST /api/simulation/start`** (perfil Administrador/Operacional, `speedMultiplier` opcional — 30x a 300x no seletor da UI) — pega até 4 `TransportOrder`s em status "Criada", escolhe veículo (capacidade compatível) e motorista (disponível + CNH válida) elegíveis, e chama o **mesmo** `TripSchedulingService.ScheduleTripAsync` que a tela "Viagens" usa.
2. Pra cada viagem programada, o `TripScenarioGenerator` monta um roteiro de passos com horário virtual: sair → abastecer → (≈35% de chance) uma ocorrência menor que se resolve sozinha 20-40min depois → chegar (5-7h virtuais depois da saída).
3. O `SimulationEngine` (singleton, guarda o estado entre requisições HTTP) tem um relógio virtual que avança conforme o tempo real passa, multiplicado pelo `speedMultiplier`. Um `SimulationHostedService` (`BackgroundService`, tick a cada 1s real) chama `engine.TickAsync()`, que executa todo passo cujo horário virtual já venceu.
4. Cada passo chama o Application Service real (`ITripService.StartAsync`, `IFuelingService.CreateAsync`, `IOccurrenceService.CreateAsync`/`ResolveAsync`, `ITripService.CompleteDeliveryAsync`) — grava no banco de verdade.
5. `CompleteDeliveryAsync` dispara `DeliveryCompletedEvent` → `BillingGenerationHandler` cria o `AccountReceivable` e marca a OT como "Faturada" **automaticamente**, sem nenhum código extra no simulador — é o mesmo pipeline de eventos que já existia pra uso manual.
6. Cada passo publica um evento pra uma lista de assinantes (`Channel<SimulationEventDto>` por conexão SSE) — é isso que alimenta o feed ao vivo.
7. Quando a fila de passos pendentes esvazia, o `SimulationEngine` marca `Status = Finished` sozinho e publica um evento `SimulationFinished`. Sem loop infinito, sem intervenção manual.

## Transmissão ao vivo (SSE)

`GET /api/simulation/stream` — Server-Sent Events **sem pacote NuGet extra** (`Response.WriteAsync` manual com `event: X\ndata: Y\n\n`).

**Detalhe importante**: o frontend consome isso com `fetch` + `ReadableStream` manual (`useSimulationStream.ts`), **não** o `EventSource` nativo do navegador — porque `EventSource` não permite mandar header `Authorization`, e o endpoint continua protegido pelo mesmo JWT que todo o resto da Api (nunca ficou anônimo). O hook também faz polling de `/api/simulation/status` a cada 3s como complemento, porque o servidor só manda um snapshot de status na conexão inicial, não a cada tick.

## O que testar se mexer nisso de novo

- `dotnet ef database drop --force` + rodar de novo reseeda o banco com 4 OTs "Criada" frescas (o simulador consome esse pool a cada rodada — depois de rodar uma vez, pode não sobrar OT elegível; gerar mais via Cotações → aprovar → "Gerar OT").
- Testado com `speedMultiplier: 300` — uma simulação de 2-4 viagens completa em ~1-2 minutos reais.
- Verificação feita tanto via curl direto (`/api/simulation/start`, poll de `/api/simulation/status`) quanto pelo navegador de verdade clicando o botão e vendo o feed atualizar.
