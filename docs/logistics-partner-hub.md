# Feature: Logistics Partner Hub

## Objetivo
Sistema genérico e resiliente que recebe solicitações de serviço do Monitor Field Service e as transforma dinamicamente em requests para centenas de parceiros logísticos, cada um com sua API, payload e autenticação próprios — sem necessidade de deploy para adicionar ou configurar novos parceiros.

## Contexto
O Monitor Field Service precisa despachar serviços (recolhimento, frete de moto, frete de peças) para parceiros logísticos externos. Hoje não existe uma camada de integração genérica — cada parceiro exigiria código customizado. O Hub centraliza essa integração com mapeamento configurável de campos ("de-para") e um mecanismo de resiliência com retry, enfileiramento e notificação de falha.

### Fluxo principal
```
Monitor Field Service
        │
        ▼ POST /api/service-orders
   ┌─────────────┐
   │  Hub (API)   │ ── persiste solicitação (status: Solicitado)
   └─────┬───────┘
         │ responde 202 Accepted (assíncrono)
         ▼
   ┌─────────────────┐
   │  Background Job  │
   │  (Worker/Queue)  │
   └─────┬───────────┘
         │ 1. Busca config do parceiro (URL, auth, field mapping)
         │ 2. Aplica "de-para" nos campos → monta payload do parceiro
         │ 3. Envia HTTP request ao parceiro
         │ 4. Persiste request/response
         ▼
   ┌─────────────────┐
   │ Parceiro (API)   │ ── responde síncrono (aceite/rejeição)
   └─────────────────┘
         │
         │ ... depois, quando status muda:
         ▼
   POST /api/webhooks/{partnerId}
   ┌─────────────────┐
   │  Hub (Webhook)   │
   │  1. Aplica "de-para reverso"
   │  2. Normaliza status
   │  3. Notifica Monitor
   └─────────────────┘
```

### Fluxo de resiliência
```
Envio ao parceiro falha
        │
        ▼
   Retry com backoff exponencial (3 tentativas)
        │ falhou todas?
        ▼
   Enfileira para retry posterior (fila persistente)
        │ expirou TTL da fila?
        ▼
   Notifica Monitor que a solicitação falhou
```

## Critérios de aceite
- [ ] Receber solicitação de serviço via POST e retornar 202 Accepted
- [ ] Cadastrar parceiros logísticos via API (URL, credenciais, método de autenticação)
- [ ] Configurar mapeamento de campos (de-para) por parceiro via API, sem deploy
- [ ] Transformar payload canônico em payload do parceiro usando o mapeamento configurado
- [ ] Enviar request ao parceiro com autenticação configurável (API Key, OAuth, Basic Auth)
- [ ] Persistir request enviado e response recebido de cada interação
- [ ] Receber webhook de status do parceiro em `POST /api/webhooks/{partnerId}`
- [ ] Aplicar "de-para reverso" no payload do webhook para normalizar status
- [ ] Notificar Monitor Field Service sobre mudanças de status
- [ ] Retry com backoff exponencial em caso de falha
- [ ] Enfileirar solicitações que falharam após retries
- [ ] Notificar Monitor sobre falhas definitivas
- [ ] Ciclo de vida do serviço: Solicitado → Aceito → Em andamento → Concluído / Cancelado

## Entidades e banco de dados

### Partner (Parceiro Logístico)
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| Name | string | Nome do parceiro |
| BaseUrl | string | URL base da API |
| AuthType | enum | ApiKey, OAuth2, BasicAuth |
| AuthConfig | jsonb | Credenciais (encrypted) — formato varia por AuthType |
| IsActive | bool | Se o parceiro está ativo |
| CreatedAt | DateTime | Data de criação |
| UpdatedAt | DateTime | Última atualização |

### FieldMapping (Mapeamento de campos via JsonPath)
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| PartnerId | Guid | FK → Partner |
| Direction | enum | Outbound (envio) / Inbound (webhook) |
| SourcePath | string | JsonPath de leitura no payload de origem (ex: `$.client_name`, `$.dados.estado`) |
| TargetPath | string | JsonPath de escrita no payload de destino (ex: `$.cliente.nome`, `$.status`) |
| DefaultValue | string? | Valor padrão quando source é nulo (ex: `false`, `"REBOQUE"`, `0`) |
| Order | int | Ordem de processamento dos mapeamentos |
| ServiceType | enum | Recolhimento, FreteMoto, FretePecas |

#### Exemplo de mapeamento outbound (Soon/ReboqueMe)
O payload canônico do Monitor é flat:
```json
{ "protocol": "548478548725777", "client_name": "henrique", "client_phone": "31888888111", "vehicle_brand": "Jeep", ... }
```

Os mapeamentos constroem o payload aninhado para a API do parceiro:
| SourcePath | TargetPath | Resultado |
|---|---|---|
| `$.protocol` | `$.ownId` | `{ "ownId": "548478548725777" }` |
| `$.client_name` | `$.cliente.nome` | `{ "cliente": { "nome": "henrique" } }` |
| `$.client_phone` | `$.cliente.telefoneCelular` | `{ "cliente": { "telefoneCelular": "31888888111" } }` |
| `$.vehicle_brand` | `$.veiculoCliente.marca` | `{ "veiculoCliente": { "marca": "Jeep" } }` |
| `$.origin_lat` | `$.enderecoOrigem.latitude` | `{ "enderecoOrigem": { "latitude": -23.570193 } }` |
| (nulo) | `$.situacaoVeiculo.capotado` | DefaultValue: `false` |

#### Exemplo de mapeamento inbound (webhook)
O parceiro envia payload aninhado, e o mapeamento extrai para formato canônico:
| SourcePath | TargetPath | Efeito |
|---|---|---|
| `$.dados.estado` | `$.status` | Extrai de objeto aninhado para raiz |
| `$.dados.identificador` | `$.order_id` | Extrai de objeto aninhado para raiz |

### ServiceOrder (Solicitação de serviço)
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| ExternalId | string | ID vindo do Monitor |
| PartnerId | Guid | FK → Partner |
| ServiceType | enum | Recolhimento, FreteMoto, FretePecas |
| Status | enum | Solicitado, Aceito, EmAndamento, Concluido, Cancelado |
| CanonicalPayload | jsonb | Payload original recebido do Monitor |
| PartnerPayload | jsonb | Payload transformado enviado ao parceiro |
| PartnerExternalId | string | ID retornado pelo parceiro |
| CreatedAt | DateTime | |
| UpdatedAt | DateTime | |

### ServiceOrderLog (Histórico de interações)
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| ServiceOrderId | Guid | FK → ServiceOrder |
| Direction | enum | Outbound / Inbound |
| RequestPayload | jsonb | Payload enviado |
| ResponsePayload | jsonb | Payload recebido |
| HttpStatusCode | int | Status HTTP |
| AttemptNumber | int | Número da tentativa |
| CreatedAt | DateTime | |

### PartnerEndpoint (Endpoints do parceiro por tipo de serviço)
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | PK |
| PartnerId | Guid | FK → Partner |
| ServiceType | enum | Recolhimento, FreteMoto, FretePecas |
| HttpMethod | string | POST, PUT, etc. |
| Path | string | Path relativo (ex: `/api/v1/pickups`) |

## Camadas afetadas

### Domain
- **Entidades**: `Partner`, `FieldMapping`, `ServiceOrder`, `ServiceOrderLog`, `PartnerEndpoint`
- **Enums**: `ServiceType`, `ServiceOrderStatus`, `AuthType`, `MappingDirection`
- **Interfaces de repositório**: `IPartnerRepository`, `IFieldMappingRepository`, `IServiceOrderRepository`, `IServiceOrderLogRepository`, `IPartnerEndpointRepository`
- **Interfaces de serviço**: `IPayloadTransformer`, `IPartnerAuthenticator`, `IPartnerNotifier`
- **Eventos de domínio**: `ServiceOrderCreatedEvent`, `ServiceOrderStatusChangedEvent`, `ServiceOrderFailedEvent`

### Application
- **Commands (CQRS via MediatR)**:
  - `CreateServiceOrderCommand` → recebe solicitação do Monitor
  - `ProcessServiceOrderCommand` → transforma e envia ao parceiro
  - `HandleWebhookCommand` → processa callback do parceiro
  - `RetryServiceOrderCommand` → reprocessa solicitação falhada
  - `CreatePartnerCommand` / `UpdatePartnerCommand` → CRUD de parceiros
  - `CreateFieldMappingCommand` / `UpdateFieldMappingCommand` → CRUD de mapeamentos
  - `CreatePartnerEndpointCommand` / `UpdatePartnerEndpointCommand` → CRUD de endpoints
- **Queries**:
  - `GetServiceOrderQuery` / `GetServiceOrdersQuery`
  - `GetPartnerQuery` / `GetPartnersQuery`
  - `GetFieldMappingsQuery`
- **Services**:
  - `PayloadTransformerService` — aplica o de-para via JsonPath nos campos (outbound e inbound), construindo objetos aninhados
  - `JsonPathBuilder` — utilitário para extrair valores via JsonPath e construir JSON com objetos aninhados
  - `PartnerDispatcherService` — orquestra envio ao parceiro com autenticação

### Infrastructure.Data
- **DbContext**: `LogisticsPartnerDbContext`
- **EF Core Mappings**: configurações Fluent API para todas as entidades
- **Repositórios**: implementações dos repositórios do Domain
- **Migrations**: migration inicial com todas as tabelas
- **Database**: PostgreSQL com suporte a jsonb

### Infrastructure.Http
- **`PartnerHttpClient`** — HttpClient tipado para chamadas aos parceiros
- **`IPartnerAuthenticator` implementações**:
  - `ApiKeyAuthenticator`
  - `BasicAuthAuthenticator`
  - `OAuth2Authenticator`
- **Polly policies**: retry com backoff exponencial, circuit breaker

### Infrastructure.BackgroundJobs
- **`ServiceOrderProcessorJob`** — background worker que processa solicitações pendentes
- **`RetryQueueProcessor`** — processa fila de retentativas
- **Opção**: Hangfire ou Worker Service nativo do .NET

### Api
- **Endpoints**:

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/api/service-orders` | Recebe solicitação do Monitor |
| GET | `/api/service-orders/{id}` | Consulta status de uma solicitação |
| GET | `/api/service-orders` | Lista solicitações (com filtros) |
| POST | `/api/webhooks/{partnerId}` | Recebe callback do parceiro |
| POST | `/api/partners` | Cadastra parceiro |
| PUT | `/api/partners/{id}` | Atualiza parceiro |
| GET | `/api/partners` | Lista parceiros |
| GET | `/api/partners/{id}` | Detalhe do parceiro |
| POST | `/api/partners/{id}/field-mappings` | Configura mapeamento de campos |
| PUT | `/api/partners/{id}/field-mappings/{mappingId}` | Atualiza mapeamento |
| GET | `/api/partners/{id}/field-mappings` | Lista mapeamentos |
| POST | `/api/partners/{id}/endpoints` | Configura endpoint por serviço |
| PUT | `/api/partners/{id}/endpoints/{endpointId}` | Atualiza endpoint |
| GET | `/api/partners/{id}/endpoints` | Lista endpoints |

## Regras de negócio
1. O parceiro logístico deve estar ativo (`IsActive = true`) para receber solicitações
2. O mapeamento de campos deve existir para o parceiro + tipo de serviço antes de enviar
3. O endpoint do parceiro deve estar configurado para o tipo de serviço solicitado
4. Retry: máximo 3 tentativas com backoff exponencial (1s, 5s, 25s)
5. Após 3 falhas de retry, enfileirar para tentativa posterior
6. Após expirar TTL da fila (configurável, sugestão: 1 hora), notificar Monitor como falha definitiva
7. Status só pode avançar na ordem: Solicitado → Aceito → Em andamento → Concluído/Cancelado
8. Credenciais dos parceiros devem ser armazenadas de forma segura (encryption at rest)
9. Cada interação (request/response) deve ser logada em `ServiceOrderLog`

## Casos de borda e tratamento de erros
| Cenário | Tratamento |
|---------|------------|
| Parceiro fora do ar | Retry com backoff → enfileirar → notificar Monitor |
| Parceiro retorna 4xx | Não faz retry (erro de payload). Persiste erro, notifica Monitor |
| Parceiro retorna 5xx | Retry com backoff |
| Mapeamento não encontrado | Rejeita solicitação com erro 422 detalhado |
| Parceiro inativo | Rejeita solicitação com erro 422 |
| Webhook com partnerId inválido | Retorna 404 |
| Webhook com payload inválido | Persiste log, retorna 200 (não reprocessa), alerta interno |
| Solicitação duplicada (mesmo ExternalId) | Retorna a solicitação existente (idempotência) |
| Timeout na chamada ao parceiro | Trata como falha, entra no fluxo de retry |

## Passos de implementação

### Fase 1 — Estrutura do projeto
1. Criar solution .NET 8 com Clean Architecture:
   - `LogisticsPartnerHub.Domain`
   - `LogisticsPartnerHub.Application`
   - `LogisticsPartnerHub.Infrastructure`
   - `LogisticsPartnerHub.Api`
2. Configurar dependências: MediatR, FluentValidation, AutoMapper, Polly, EF Core (Npgsql), Serilog

### Fase 2 — Domain
3. Criar enums: `ServiceType`, `ServiceOrderStatus`, `AuthType`, `MappingDirection`
4. Criar entidades: `Partner`, `FieldMapping`, `ServiceOrder`, `ServiceOrderLog`, `PartnerEndpoint`
5. Criar interfaces de repositório
6. Criar interfaces de serviço (`IPayloadTransformer`, `IPartnerAuthenticator`)

### Fase 3 — Infrastructure.Data
7. Configurar `LogisticsPartnerDbContext`
8. Criar mappings EF Core (Fluent API)
9. Implementar repositórios
10. Criar migration inicial:
    ```bash
    dotnet ef migrations add InitialCreate -p src/LogisticsPartnerHub.Infrastructure -s src/LogisticsPartnerHub.Api
    ```

### Fase 4 — Application
11. Criar commands e handlers para CRUD de parceiros
12. Criar commands e handlers para CRUD de mapeamentos e endpoints
13. Implementar `PayloadTransformerService` (aplica de-para usando os FieldMappings do banco)
14. Criar `CreateServiceOrderCommand` + handler (recebe do Monitor, persiste, retorna 202)
15. Criar `ProcessServiceOrderCommand` + handler (transforma payload, envia ao parceiro)
16. Criar `HandleWebhookCommand` + handler (de-para reverso, atualiza status)

### Fase 5 — Infrastructure.Http
17. Implementar `PartnerHttpClient` com Polly (retry + circuit breaker)
18. Implementar autenticadores: `ApiKeyAuthenticator`, `BasicAuthAuthenticator`, `OAuth2Authenticator`

### Fase 6 — Infrastructure.BackgroundJobs
19. Implementar `ServiceOrderProcessorJob` (background worker)
20. Implementar fila de retry com persistência
21. Implementar notificação de falha ao Monitor

### Fase 7 — Api
22. Criar controllers/endpoints conforme tabela de endpoints
23. Configurar AutoMapper profiles
24. Configurar middleware de exceções global
25. Configurar Swagger/OpenAPI

### Fase 8 — Testes
26. Testes unitários do `PayloadTransformerService`
27. Testes unitários dos handlers
28. Testes de integração dos endpoints
29. Testes do fluxo de resiliência (retry, enfileiramento)

## Stack tecnológica
| Componente | Tecnologia |
|-----------|------------|
| Runtime | .NET 8 |
| API | ASP.NET Core Minimal APIs ou Controllers |
| ORM | Entity Framework Core |
| Banco de dados | PostgreSQL |
| CQRS | MediatR |
| Validação | FluentValidation |
| Mapeamento | AutoMapper |
| Resiliência | Polly |
| Background Jobs | .NET BackgroundService ou Hangfire |
| Logs | Serilog |
| Docs | Swagger / OpenAPI |

## Dúvidas em aberto
- Qual o formato exato do payload canônico que o Monitor envia? (definir contrato)
- Qual endpoint do Monitor deve ser chamado para notificar mudanças de status?
- Há necessidade de rate limiting por parceiro?
- Há necessidade de health check dos parceiros?
- O sistema terá interface web própria ou usará outro frontend existente?
- Credenciais sensíveis: usar Azure Key Vault, AWS Secrets Manager, ou encryption no banco?
