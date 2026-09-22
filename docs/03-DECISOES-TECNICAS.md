# Decisões técnicas e bugs reais encontrados

## SQLite em vez de PostgreSQL (2026-09-09)

Rodou com PostgreSQL via Docker Compose até 2026-09-09. Trocado pra **SQLite** porque o projeto ia ser demonstrado numa máquina sem Docker instalado — o objetivo do `start-cargoflow.bat` sempre foi "clonar e já funcionar", e isso deixou de valer quando a máquina de destino não tinha Docker. Como o domínio não usa nenhum recurso específico do Postgres (sem `jsonb`, arrays, `ILIKE`, funções nativas), a troca de provider do EF Core (`Npgsql.EntityFrameworkCore.PostgreSQL` → `Microsoft.EntityFrameworkCore.Sqlite`) não exigiu nenhuma mudança de modelo — só `DependencyInjection.cs`, `CargoFlowDbContextFactory.cs`, a connection string (`.env`/`appsettings.Development.json`) e a migration inicial regenerada do zero (as migrations antigas eram Npgsql-specific e foram apagadas, não adaptadas). `docker-compose.yml` foi removido e `start-cargoflow.ps1` não sobe mais nenhum container — só a Api e o frontend. Banco agora é `CargoFlow.Api/cargoflow.db`, recriado a cada `dotnet ef database drop` como antes. Testado ao vivo: migrations + seed automáticos no primeiro `dotnet run`, login funcionando, 37 testes unitários (que não dependem de banco) continuam passando.

## Por que não RabbitMQ/Redis/OpenTelemetry

O domínio pede "processamento assíncrono orientado a eventos" — entregue via **MediatR em processo**: domain events publicados logo após `SaveChanges`, handlers reagindo a eles (ex: `BillingGenerationHandler`). Um broker de mensagens real, cache distribuído e observabilidade completa agregariam complexidade de infraestrutura sem agregar nada ao que o projeto quer demonstrar nesta escala — a arquitetura de eventos (vocabulário, handlers, idempotência) é a mesma que seria usada com um broker de verdade, só a transportadora muda. Decisão confirmada com o usuário antes de começar a implementar (ver `00-CONTEXTO-GERAL.md`).

## Domain events no Domain, não na Application

`AggregateRoot` referencia só `MediatR.Contracts` (as interfaces `INotification` etc., não a implementação completa `MediatR`) — mantém o Domain com zero dependência de infraestrutura, mesmo levantando eventos. Os handlers de verdade (que dependem de repositórios) ficam na Application, que já depende do Domain de qualquer forma.

## Só `Trip` tem modelo rico de verdade

Todas as outras entidades (Company, Vehicle, Driver, Document, FreightQuote...) são CRUD com validação em nível de serviço — propriedades públicas, sem guards internos. Só `Trip` ganhou o tratamento de agregado rico (métodos `Schedule/Start/CompleteDelivery/Cancel` com guards de transição de estado + domain events) porque é o único objeto que **precisa** disso: é o centro do domínio e o que o simulador movimenta repetidamente. Aplicar o mesmo rigor em tudo teria sido complexidade sem retorno.

## Sem geocodificação real

Distância entre origem/destino é uma estimativa plausível (300-1200km) informada manualmente na cotação e gerada aleatoriamente pelo simulador — nenhuma integração de mapas/geocoding está no escopo. Documentado como decisão consciente, não gap esquecido.

## Ícones: SVG à mão, não uma biblioteca

Depois do polimento visual (pedido: "ícones mais profissionais, brancos e minimalistas, não coloridos"), em vez de adicionar `lucide-react`/`heroicons` como dependência nova, os ícones foram escritos à mão como componentes React (`frontend/src/components/icons/index.tsx`), estilo Feather Icons (linha só, `viewBox 24x24`, `strokeWidth 1.8`, cantos arredondados). Cada ícone usa `stroke="currentColor"` — não tem cor própria, herda a cor do texto ao redor (cinza-mudo por padrão, azul de destaque no item ativo/hover), que é o que dá o efeito "profissional/monocromático" pedido, não literalmente pixels brancos. Motivo de não usar biblioteca: mantém zero dependência nova nesse ponto do projeto, controle total sobre o traço exato, e consistência com a convenção já estabelecida ("sem biblioteca de componentes" no frontend inteiro).

## Bugs reais encontrados e corrigidos durante a implementação

1. **Consumo de combustível absurdo no seed (281 km/l).** O gerador de abastecimentos espalhava as leituras de odômetro como frações do odômetro total do veículo (55%, 75%, 90%...), o que pra veículos com centenas de milhares de km gerava intervalos de dezenas de milhares de km entre reabastecimentos — divididos por só 150-450L, o km/l calculado ficava absurdo. Só apareceu testando o Dashboard ao vivo (KPI "Consumo médio da frota"). Corrigido reescrevendo o gerador pra trabalhar de trás pra frente a partir do odômetro atual, com intervalos realistas (~400-900km, consumo 2-3.5 km/l) — agora sai ~2.8 km/l, coerente com caminhão pesado de verdade.
2. **Migração inicial deixando config existente inválida (não é deste projeto, é o padrão que motivou testar isso aqui também).** Lição aplicada preventivamente: sempre que uma coluna nova tem regra de validação (ex: mínimo/máximo), garantir que o `defaultValue` da migração já é um valor válido, não `0`/vazio.
3. **Conflito de porta do Postgres.** `docker-compose.yml` inicialmente mapeava 5432→5432, mas a máquina já tinha um Postgres nativo nessa porta (usado pelo product-hunter). Resolvido mapeando pra **5433** no host, documentado em `.env.example` com o motivo.
4. **`dotnet build` falhando com MSB3027/MSB3021 (arquivo em uso).** Processo `dotnet run` anterior (Api) ainda de pé, segurando as DLLs. Precisa `netstat -ano | findstr :7099` → `taskkill //F //PID <pid>` antes de rebuildar — recorrente sempre que um `dotnet run` em background não foi encerrado corretamente.
5. **Nav horizontal virando bagunça com 13 links.** O nav original (herdado do padrão do product-hunter) não escalava pra esse número de páginas. Resolvido com sidebar colapsável/responsiva (ver commit/sessão de 2026-09-09) — o product-hunter em si não precisou disso porque tem bem menos páginas.
6. **Status exibidos como nome cru do enum C#** (`EmManutencao`, `ProximoVencimento`) em vez de texto legível. Corrigido com um mapa central (`frontend/src/utils/statusLabel.ts`) aplicado em todas as 12 telas que mostravam status — pego só depois de "pronto", num pedido de polimento à parte.

## Convenções herdadas do product-hunter (repo irmão)

Solução .NET em 4 camadas + Tests, EF Core + Npgsql, `Entity` base class com setters protegidos, `.env` carregado via `DotNetEnv` (nunca em `appsettings.json`), migrations aplicadas automaticamente no startup (`context.Database.MigrateAsync()`), `docker-compose.yml` só com o serviço Postgres, credenciais de dev reais commitadas (repo privado pessoal). Frontend: Vite + React + TS sem lib de componentes, CSS na mão, `client.ts`/`types.ts` como camada de acesso à Api.
