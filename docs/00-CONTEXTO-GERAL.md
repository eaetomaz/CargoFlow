# CargoFlow — Contexto Geral

> Visão geral do projeto: o que é, de onde veio, como está organizado e onde fica cada coisa. Última atualização: 2026-09-09 (2ª rodada de polimento visual).

## O que é isso

**CargoFlow** é um TMS (Transportation Management System) construído do zero como **projeto de portfólio/estudo pessoal** — não um cliente real, não é pra produção. Vive em `C:\Dev\Private\CargoFlow`.

Cobre o ciclo real de uma operação de transporte de cargas: cotação de frete configurável → ordem de transporte → programação → viagem (com custo/receita/margem calculados) → ocorrências, abastecimento, manutenção → financeiro → auditoria. Tudo orientado a regras de negócio reais (não hardcoded) e amarrado por um **simulador de operação** que movimenta o sistema inteiro sozinho, em relógio virtual acelerado, mutando dado real via os mesmos serviços que a UI usa.

## Origem

O ponto de partida foi um documento de briefing (`CargoFlow_Contexto.md`) descrevendo um TMS bem mais ambicioso — 15 domínios, RabbitMQ, Redis, OpenTelemetry/Grafana, RBAC granular, testes de integração/e2e, CI/CD, 10 documentos de arquitetura. Antes de implementar, 4 decisões de escopo reduziram a ambição sem abrir mão de "funcional de verdade":

1. **Infra simplificada** — API + PostgreSQL + React 100% funcionais; processamento assíncrono simulado em processo via **MediatR** (sem RabbitMQ real); sem Redis; sem OpenTelemetry/Grafana.
2. **Domínio: Core + Operação + Gestão** — todos os módulos do doc original **exceto** rastreamento GPS em tempo real e a camada de IA/analytics.
3. **Simulador de operação incluído** — foi tratado como o diferencial do projeto, não um nice-to-have.
4. **Testes/docs leves** — testes unitários só nas regras de negócio centrais, sem suíte de integração/e2e nem CI/CD; um README bom em vez dos 10 documentos originais.

O projeto foi construído em etapas ao longo de setembro/2026, do zero até os 12 marcos do plano concluídos, seguido de **duas rodadas de polimento visual** depois de "pronto":

- **1ª rodada**: sidebar moderna e colapsável (era um nav horizontal que não escalava com 13 links) + status legíveis em todas as telas (estavam mostrando o nome cru do enum C#, tipo "EmManutencao").
- **2ª rodada**: fonte reduzida globalmente (achada grande demais), ícones da sidebar/feed do simulador trocados de emoji colorido pra um conjunto de ícones de linha monocromáticos (herdam a cor do texto, sem cor própria), e uma faixa de KPIs rápidos adicionada no topo de 10 telas de listagem (derivados do dado já carregado, sem chamada extra à Api) — pra dar uma sensação mais "viva"/analítica além do puro cadastro.

## Stack

- **Backend**: C# / .NET 10, ASP.NET Core Web API, Entity Framework Core
- **Banco**: SQLite (arquivo local `CargoFlow.Api/cargoflow.db`, sem servidor nem Docker — trocado de PostgreSQL em 2026-09-09 pra rodar em qualquer máquina, inclusive sem Docker instalado; ver `03-DECISOES-TECNICAS.md`)
- **Frontend**: React + TypeScript (Vite), CSS próprio (sem biblioteca de componentes)
- **Autenticação**: JWT, RBAC com 5 perfis fixos
- **Eventos em processo**: MediatR (domain events, sem message broker externo)
- **Gráficos**: Recharts
- **Testes**: xUnit (37 testes, só na Application layer)
- **Seed de dados**: Bogus (`pt_BR`) — tudo fictício

## Como rodar

**Um clique** (Windows): `start-cargoflow.bat` na raiz do repo — sobe Api + frontend, espera tudo ficar pronto, abre o navegador em `http://localhost:5180` sozinho. Sem Docker: o banco é o arquivo SQLite `CargoFlow.Api/cargoflow.db`.

**Portas**: Api `https://localhost:7099` / `http://localhost:5099`, frontend `http://localhost:5180`.

**Login** — 5 usuários seedados automaticamente, senha `CargoFlow@123` pra todos: `admin` (Administrador), `operacional` (Operacional), `financeiro` (Financeiro), `manutencao` (Manutenção), `motorista` (Motorista).

`.env` já vem com credenciais de dev reais commitadas (repo privado pessoal — mesma política já usada no product-hunter/seleto-site: "clonar e já funcionar", nunca fazer isso num repo público).

## Estado atual (2026-09-09) — os 12 marcos do plano concluídos

Todo marco foi **testado ao vivo** antes de ser dado como pronto (curl + navegador real, nunca só "compilou"):

1. Scaffold da solução + atalho `start-cargoflow.bat`
2. Domínio core (Empresas/Veículos/Motoristas/Documentos)
3. Auth/RBAC (JWT, 5 perfis)
4. CRUD completo dos 4 módulos core + seed Bogus
5. Cotação de frete (tabela de preços configurável + cálculo ao vivo)
6. Núcleo operacional (OT → Programação → Viagem, 3 regras de elegibilidade, MediatR)
7. Ocorrências + Abastecimento + Manutenção
8. Financeiro + Auditoria (faturamento automático por evento)
9. Dashboard com drill-down (KPIs clicáveis, gráfico de tendência)
10. **Simulador de operação** (o diferencial — ver `02-SIMULADOR-DE-OPERACAO.md`)
11. Sidebar moderna + colapsável (pedido de polimento pós-entrega, substituiu o nav horizontal original)
12. README + polimento (cobertura de testes fechada, estados de erro de carregamento, favicon, status legíveis em todas as telas)

Nada ficou pela metade — não existe feature "quase pronta" pendurada.

**Depois dos 12 marcos**, mais uma rodada de polimento visual (ver acima): fonte menor, ícones SVG monocromáticos (`frontend/src/components/icons/`), e `KpiStrip` (`frontend/src/components/KpiStrip.tsx`) em Veículos/Motoristas/Documentos/Viagens/Ocorrências/Manutenção/Cotações/Ordens de Transporte/Empresas/Abastecimentos.

## Outros arquivos desta documentação

- `01-MODELO-DE-DOMINIO-E-REGRAS.md` — entidades, arquitetura em camadas, tabela de regras de negócio e onde cada uma vive no código.
- `02-SIMULADOR-DE-OPERACAO.md` — como o simulador funciona por dentro (relógio virtual, SSE, orquestração).
- `03-DECISOES-TECNICAS.md` — decisões de arquitetura e os bugs reais encontrados e corrigidos durante a implementação.
- `04-PROXIMOS-PASSOS.md` — o que ficou de fora por decisão de escopo (não por esquecimento) e ideias pra evoluir o projeto se um dia isso importar.
- `screenshots/` — 5 capturas de tela (dashboard com simulador ao vivo, cotações, viagens, veículos, financeiro) usadas na seção "Telas" do `README.md`. Geradas em 2026-09-25 com Playwright + Edge headless (1440x900) contra uma API com banco novo, rodando o simulador a 300x; se as telas mudarem visualmente, precisam ser refeitas.
