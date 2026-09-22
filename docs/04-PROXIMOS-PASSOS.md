# Próximos passos

## Estado: projeto completo conforme o plano original

Os 12 marcos foram concluídos e testados ao vivo (ver `00-CONTEXTO-GERAL.md`). Não há pendência bloqueante nem feature pela metade. Isso é uma lista de ideias/gaps conhecidos, não um backlog urgente.

## O que ficou de fora por decisão de escopo (não por esquecimento)

- **Rastreamento GPS em tempo real** (geolocalização de veículo) — cortado do escopo desde o início, junto com a camada de IA/analytics.
- **RabbitMQ, Redis, OpenTelemetry/Grafana** — ver justificativa em `03-DECISOES-TECNICAS.md`.
- **Testes de integração/e2e, CI/CD** — só unitários na Application layer (37 testes).
- **Geocodificação real** — distância é estimativa manual/aleatória.
- **Portal externo pro cliente** — o sistema é 100% interno/operacional, sem autoatendimento.

## Se um dia quiser evoluir isso

- **CI/CD leve**: GitHub Actions rodando `dotnet test` + `npx tsc -b` a cada push seria barato de adicionar e já pegaria regressão de tipo/teste sem exigir infra nova.
- **Testes de integração** contra o Postgres real (via Testcontainers) cobririam os repositórios, que hoje só são exercitados manualmente.
- **RBAC mais granular**: hoje é só por perfil no controller inteiro; um sistema de permissão por operação (ex: "Operacional pode criar OT mas não cancelar") exigiria uma tabela de permissões de verdade.
- **Simulador com mais variedade**: hoje só tem um tipo de "ocorrência menor" no roteiro (pneu furado/atraso/documentação); dar peso diferente por tipo de carga ou rota tornaria a demonstração mais rica.
- **Exportação de relatórios** (PDF/Excel) do Dashboard e do Financeiro — não existe hoje, tudo é só tela.

## Coisas menores, não bloqueantes

- Placas geradas no seed usam padrão livre (não necessariamente o formato Mercosul real) — não afeta nenhuma regra de negócio, só estética do dado fictício.
- `dotnet-ef` tool está numa versão um pouco mais antiga que o runtime (10.0.6 vs 10.0.11) — só gera um warning, nunca quebrou nada, mas `dotnet tool update` resolveria o aviso.
