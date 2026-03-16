# Logistics Partner Hub - Guia de Uso

## Pre-requisitos

- .NET 10 SDK
- EF Core Tools (`dotnet tool install --global dotnet-ef`)
- PostgreSQL rodando localmente (ou via Docker)
- Um cliente HTTP (curl, Postman, Insomnia, etc.)

> **Importante:** Verifique se o `dotnet-ef` esta na versao 10.x:
> ```bash
> dotnet ef --version
> # Se estiver desatualizado:
> dotnet tool update --global dotnet-ef
> ```

## 1. Configurar o banco de dados

Crie o banco PostgreSQL:

```bash
createdb logistics_partner_hub
```

Ou via Docker:

```bash
docker run -d --name postgres-hub \
  -e POSTGRES_DB=logistics_partner_hub \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16
```

A connection string padrao esta em `appsettings.json`:

```
Host=localhost;Database=logistics_partner_hub;Username=postgres;Password=postgres
```

## 2. Aplicar migrations e rodar a API

> **IMPORTANTE:** Todos os comandos `dotnet ef` devem ser executados a partir da pasta do projeto de startup (`src/LogisticsPartnerHub.Api`), usando o flag `-p` para apontar para o projeto de Infrastructure e `-o` para definir a pasta de saida das migrations.

```bash
# Navegue ate o projeto de startup
cd src/LogisticsPartnerHub.Api

# Criar a migration (apenas na primeira vez ou quando alterar entidades)
dotnet ef migrations add InitialCreate \
  -p ../LogisticsPartnerHub.Infrastructure \
  -o ../LogisticsPartnerHub.Infrastructure/Data/Migrations

# Aplicar a migration no banco de dados
dotnet ef database update -p ../LogisticsPartnerHub.Infrastructure

# Verificar se a migration foi aplicada
dotnet ef migrations list -p ../LogisticsPartnerHub.Infrastructure

# Rodar a API
dotnet run
```

### Criando novas migrations no futuro

Sempre que alterar entidades ou configuracoes do EF Core:

```bash
cd src/LogisticsPartnerHub.Api

dotnet ef migrations add NomeDaMigration \
  -p ../LogisticsPartnerHub.Infrastructure \
  -o ../LogisticsPartnerHub.Infrastructure/Data/Migrations

dotnet ef database update -p ../LogisticsPartnerHub.Infrastructure
```

### Troubleshooting de migrations

| Problema | Solucao |
|----------|---------|
| `doesn't reference Microsoft.EntityFrameworkCore.Design` | O pacote `Microsoft.EntityFrameworkCore.Design` deve estar no projeto **Api** (startup), nao apenas no Infrastructure |
| `EF tools version is older than runtime` | Atualize com `dotnet tool update --global dotnet-ef` |
| `Failed to connect to database` | Verifique se o PostgreSQL esta rodando e se a connection string em `appsettings.json` esta correta |
| `relation already exists` | A migration ja foi aplicada. Use `dotnet ef migrations list` para verificar |

A API estara disponivel em `http://localhost:5000` (ou a porta configurada).

Acesse o Swagger em: `http://localhost:5000/swagger`

---

## 3. Passo a passo: Configurar um parceiro e enviar uma solicitacao

O fluxo completo envolve 4 etapas:

1. Cadastrar o parceiro logistico
2. Configurar o mapeamento de campos (de-para)
3. Configurar o endpoint do parceiro
4. Enviar uma solicitacao de servico

### 3.1 Cadastrar um parceiro logistico

```bash
curl -X POST http://localhost:5000/api/partners \
  -H "Content-Type: application/json" \
  -d '{
    "name": "TransLog Express",
    "baseUrl": "https://api.translog.com.br",
    "authType": "ApiKey",
    "authConfig": "{\"headerName\": \"X-Api-Key\", \"apiKey\": \"minha-chave-secreta-123\"}"
  }'
```

**Resposta (201 Created):**

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "TransLog Express",
  "baseUrl": "https://api.translog.com.br",
  "authType": "ApiKey",
  "isActive": true,
  "createdAt": "2026-03-10T23:00:00Z",
  "updatedAt": "2026-03-10T23:00:00Z"
}
```

> Guarde o `id` retornado — ele sera usado em todos os proximos passos.
> Substitua `a1b2c3d4-e5f6-7890-abcd-ef1234567890` pelo id real retornado.

#### Tipos de autenticacao suportados

**ApiKey:**
```json
{
  "authType": "ApiKey",
  "authConfig": "{\"headerName\": \"X-Api-Key\", \"apiKey\": \"sua-chave\"}"
}
```

**BasicAuth:**
```json
{
  "authType": "BasicAuth",
  "authConfig": "{\"username\": \"user\", \"password\": \"pass\"}"
}
```

**OAuth2 (Client Credentials):**
```json
{
  "authType": "OAuth2",
  "authConfig": "{\"tokenUrl\": \"https://auth.parceiro.com/token\", \"clientId\": \"id\", \"clientSecret\": \"secret\", \"scope\": \"api\"}"
}
```

### 3.2 Configurar mapeamento de campos (de-para) - OUTBOUND

O mapeamento Outbound transforma o payload canonico (vindo do Monitor) para o formato do parceiro.

Exemplo: o Monitor envia `pickup_address`, mas o parceiro TransLog espera `endereco_retirada`.

```bash
PARTNER_ID="a1b2c3d4-e5f6-7890-abcd-ef1234567890"

# Mapeamento: pickup_address -> endereco_retirada
curl -X POST http://localhost:5000/api/partners/$PARTNER_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$PARTNER_ID\",
    \"direction\": \"Outbound\",
    \"sourceField\": \"pickup_address\",
    \"targetField\": \"endereco_retirada\",
    \"serviceType\": \"Recolhimento\"
  }"

# Mapeamento: vehicle_plate -> placa_veiculo
curl -X POST http://localhost:5000/api/partners/$PARTNER_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$PARTNER_ID\",
    \"direction\": \"Outbound\",
    \"sourceField\": \"vehicle_plate\",
    \"targetField\": \"placa_veiculo\",
    \"serviceType\": \"Recolhimento\"
  }"

# Mapeamento: contact_phone -> telefone_contato
curl -X POST http://localhost:5000/api/partners/$PARTNER_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$PARTNER_ID\",
    \"direction\": \"Outbound\",
    \"sourceField\": \"contact_phone\",
    \"targetField\": \"telefone_contato\",
    \"serviceType\": \"Recolhimento\"
  }"
```

### 3.3 Configurar mapeamento de campos (de-para) - INBOUND

O mapeamento Inbound transforma o payload do webhook do parceiro para o formato canonico do Hub.

```bash
# Mapeamento reverso: estado -> status
curl -X POST http://localhost:5000/api/partners/$PARTNER_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$PARTNER_ID\",
    \"direction\": \"Inbound\",
    \"sourceField\": \"estado\",
    \"targetField\": \"status\",
    \"serviceType\": \"Recolhimento\"
  }"

# Mapeamento reverso: id_pedido -> order_id
curl -X POST http://localhost:5000/api/partners/$PARTNER_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$PARTNER_ID\",
    \"direction\": \"Inbound\",
    \"sourceField\": \"id_pedido\",
    \"targetField\": \"order_id\",
    \"serviceType\": \"Recolhimento\"
  }"
```

### 3.4 Configurar o endpoint do parceiro

Define qual URL e metodo HTTP usar para cada tipo de servico.

```bash
curl -X POST http://localhost:5000/api/partners/$PARTNER_ID/endpoints \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$PARTNER_ID\",
    \"serviceType\": \"Recolhimento\",
    \"httpMethod\": \"POST\",
    \"path\": \"/api/v1/recolhimentos\"
  }"
```

### 3.5 Enviar uma solicitacao de servico

Agora o Monitor pode enviar uma solicitacao. O Hub recebe, persiste e retorna `202 Accepted`. O processamento (de-para + envio ao parceiro) acontece em background.

```bash
curl -X POST http://localhost:5000/api/service-orders \
  -H "Content-Type: application/json" \
  -d "{
    \"externalId\": \"MONITOR-OS-001\",
    \"partnerId\": \"$PARTNER_ID\",
    \"serviceType\": \"Recolhimento\",
    \"payload\": \"{\\\"pickup_address\\\": \\\"Rua Augusta, 1500 - Sao Paulo\\\", \\\"vehicle_plate\\\": \\\"ABC1D23\\\", \\\"contact_phone\\\": \\\"11999998888\\\"}\"
  }"
```

**Resposta (202 Accepted):**

```json
{
  "id": "f1e2d3c4-b5a6-7890-abcd-ef1234567890",
  "externalId": "MONITOR-OS-001",
  "partnerId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "partnerName": "TransLog Express",
  "serviceType": "Recolhimento",
  "status": "Solicitado",
  "partnerExternalId": null,
  "createdAt": "2026-03-10T23:05:00Z",
  "updatedAt": "2026-03-10T23:05:00Z"
}
```

**O que acontece nos bastidores:**

O Background Job (`ServiceOrderProcessorJob`) busca ordens com status `Solicitado` a cada 5 segundos, aplica o de-para e envia ao parceiro:

```
Payload do Monitor:
{
  "pickup_address": "Rua Augusta, 1500 - Sao Paulo",
  "vehicle_plate": "ABC1D23",
  "contact_phone": "11999998888"
}

          ↓ de-para outbound ↓

Payload enviado ao parceiro TransLog:
{
  "endereco_retirada": "Rua Augusta, 1500 - Sao Paulo",
  "placa_veiculo": "ABC1D23",
  "telefone_contato": "11999998888"
}
```

### 3.6 Consultar status da solicitacao

```bash
ORDER_ID="f1e2d3c4-b5a6-7890-abcd-ef1234567890"

curl http://localhost:5000/api/service-orders/$ORDER_ID
```

### 3.7 Listar todas as solicitacoes

```bash
curl http://localhost:5000/api/service-orders
```

---

## 4. Receber webhook do parceiro

Quando o parceiro atualiza o status (ex: recolhimento concluido), ele envia um POST para o webhook do Hub:

```bash
curl -X POST http://localhost:5000/api/webhooks/$PARTNER_ID \
  -H "Content-Type: application/json" \
  -d '{
    "id_pedido": "MONITOR-OS-001",
    "estado": "Concluido",
    "observacao": "Recolhimento realizado com sucesso"
  }'
```

**O que acontece:**

1. O Hub recebe o payload do parceiro
2. Aplica o de-para reverso (Inbound): `estado` → `status`, `id_pedido` → `order_id`
3. Atualiza o status da service order para `Concluido`
4. Notifica o Monitor Field Service via HTTP

---

## 5. Referencia de enums

### ServiceType (tipos de servico)
| Valor | Descricao |
|-------|-----------|
| `Recolhimento` | Recolhimento de veiculo |
| `FreteMoto` | Frete de moto |
| `FretePecas` | Frete de pecas |

### ServiceOrderStatus (ciclo de vida)
| Valor | Descricao |
|-------|-----------|
| `Solicitado` | Solicitacao recebida, aguardando processamento |
| `Aceito` | Parceiro aceitou a solicitacao |
| `EmAndamento` | Servico em execucao |
| `Concluido` | Servico finalizado |
| `Cancelado` | Servico cancelado ou falha definitiva |

### AuthType (tipo de autenticacao)
| Valor | Descricao |
|-------|-----------|
| `ApiKey` | Header customizavel com chave de API |
| `BasicAuth` | HTTP Basic Authentication |
| `OAuth2` | OAuth2 Client Credentials flow |

### MappingDirection (direcao do mapeamento)
| Valor | Descricao |
|-------|-----------|
| `Outbound` | Monitor → Parceiro (envio) |
| `Inbound` | Parceiro → Hub (webhook) |

---

## 6. Referencia de endpoints

| Metodo | Rota | Descricao |
|--------|------|-----------|
| `POST` | `/api/partners` | Cadastra parceiro |
| `PUT` | `/api/partners/{id}` | Atualiza parceiro |
| `GET` | `/api/partners` | Lista parceiros |
| `GET` | `/api/partners/{id}` | Detalhe do parceiro |
| `POST` | `/api/partners/{id}/field-mappings` | Cria mapeamento de campos |
| `PUT` | `/api/partners/{id}/field-mappings/{mappingId}` | Atualiza mapeamento |
| `GET` | `/api/partners/{id}/field-mappings` | Lista mapeamentos |
| `POST` | `/api/partners/{id}/endpoints` | Cria endpoint do parceiro |
| `PUT` | `/api/partners/{id}/endpoints/{endpointId}` | Atualiza endpoint |
| `GET` | `/api/partners/{id}/endpoints` | Lista endpoints |
| `POST` | `/api/service-orders` | Cria solicitacao (202 Accepted) |
| `GET` | `/api/service-orders` | Lista solicitacoes |
| `GET` | `/api/service-orders/{id}` | Detalhe da solicitacao |
| `POST` | `/api/webhooks/{partnerId}` | Recebe webhook do parceiro |

---

## 7. Fluxo de resiliencia

Quando o envio ao parceiro falha:

1. **Retry com Polly** — 3 tentativas com backoff exponencial (1s, 5s, 25s)
2. **Fila de retry** — se todas tentativas falharem, a ordem e enfileirada para retry posterior (polling a cada 30s)
3. **Notificacao de falha** — se o TTL da fila expirar (1h) ou exceder 5 retries, o Monitor e notificado e o status muda para `Cancelado`

---

## 8. Exemplo completo: Configurar um segundo parceiro

Configurando o parceiro "RapidLog" com autenticacao Basic e servico de frete de moto:

```bash
# 1. Cadastrar parceiro
curl -X POST http://localhost:5000/api/partners \
  -H "Content-Type: application/json" \
  -d '{
    "name": "RapidLog",
    "baseUrl": "https://rapidlog.io/api",
    "authType": "BasicAuth",
    "authConfig": "{\"username\": \"hub_user\", \"password\": \"s3cr3t\"}"
  }'
# Anote o id retornado -> RAPIDLOG_ID

# 2. Mapeamentos outbound
RAPIDLOG_ID="<id-retornado>"

curl -X POST http://localhost:5000/api/partners/$RAPIDLOG_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$RAPIDLOG_ID\",
    \"direction\": \"Outbound\",
    \"sourceField\": \"origin_address\",
    \"targetField\": \"from\",
    \"serviceType\": \"FreteMoto\"
  }"

curl -X POST http://localhost:5000/api/partners/$RAPIDLOG_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$RAPIDLOG_ID\",
    \"direction\": \"Outbound\",
    \"sourceField\": \"destination_address\",
    \"targetField\": \"to\",
    \"serviceType\": \"FreteMoto\"
  }"

curl -X POST http://localhost:5000/api/partners/$RAPIDLOG_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$RAPIDLOG_ID\",
    \"direction\": \"Outbound\",
    \"sourceField\": \"package_description\",
    \"targetField\": \"desc\",
    \"serviceType\": \"FreteMoto\"
  }"

# 3. Mapeamentos inbound (webhook)
curl -X POST http://localhost:5000/api/partners/$RAPIDLOG_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$RAPIDLOG_ID\",
    \"direction\": \"Inbound\",
    \"sourceField\": \"delivery_status\",
    \"targetField\": \"status\",
    \"serviceType\": \"FreteMoto\"
  }"

curl -X POST http://localhost:5000/api/partners/$RAPIDLOG_ID/field-mappings \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$RAPIDLOG_ID\",
    \"direction\": \"Inbound\",
    \"sourceField\": \"ref\",
    \"targetField\": \"order_id\",
    \"serviceType\": \"FreteMoto\"
  }"

# 4. Endpoint
curl -X POST http://localhost:5000/api/partners/$RAPIDLOG_ID/endpoints \
  -H "Content-Type: application/json" \
  -d "{
    \"partnerId\": \"$RAPIDLOG_ID\",
    \"serviceType\": \"FreteMoto\",
    \"httpMethod\": \"POST\",
    \"path\": \"/v2/deliveries\"
  }"

# 5. Enviar solicitacao
curl -X POST http://localhost:5000/api/service-orders \
  -H "Content-Type: application/json" \
  -d "{
    \"externalId\": \"MONITOR-OS-002\",
    \"partnerId\": \"$RAPIDLOG_ID\",
    \"serviceType\": \"FreteMoto\",
    \"payload\": \"{\\\"origin_address\\\": \\\"Av Paulista, 1000\\\", \\\"destination_address\\\": \\\"Rua Oscar Freire, 500\\\", \\\"package_description\\\": \\\"Peca motor 250cc\\\"}\"
  }"
```

**Transformacao que o Hub fara automaticamente:**

```
Monitor envia:                    Parceiro RapidLog recebe:
{                                 {
  "origin_address": "...",   →      "from": "...",
  "destination_address": "...",→    "to": "...",
  "package_description": "..."→     "desc": "..."
}                                 }
```

Tudo configurado via API, sem deploy.
