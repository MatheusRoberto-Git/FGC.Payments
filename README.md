# 💳 FGC Payments API

Microsserviço de Pagamentos da plataforma **FIAP Cloud Games (FCG)**.

## 📋 Descrição

Este microsserviço é responsável pelo processamento de pagamentos, gerenciamento de transações e controle de status de compras na plataforma FCG. Futuramente integrará com Azure Functions para processamento assíncrono.

---

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture**, separando responsabilidades em camadas:

```
┌─────────────────────────────────────────────────────────────┐
│                  FGC.Payments.Presentation                   │
│              (Controllers, Models, Program.cs)               │
├─────────────────────────────────────────────────────────────┤
│                  FGC.Payments.Application                    │
│                    (DTOs, Use Cases)                         │
├─────────────────────────────────────────────────────────────┤
│                 FGC.Payments.Infrastructure                  │
│              (Repositories, DbContext)                       │
├─────────────────────────────────────────────────────────────┤
│                    FGC.Payments.Domain                       │
│             (Entities, Enums, Interfaces)                    │
└─────────────────────────────────────────────────────────────┘
```

### Estrutura de Pastas

```
FGC.Payments/
├── FGC.Payments.Domain/
│   ├── Common/
│   │   ├── Entities/
│   │   └── Events/
│   ├── Entities/
│   ├── Enums/
│   ├── Events/
│   └── Interfaces/
├── FGC.Payments.Application/
│   ├── DTOs/
│   └── UseCases/
├── FGC.Payments.Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   └── Context/
│   └── Repositories/
├── FGC.Payments.Presentation/
│   ├── Controllers/
│   ├── Models/
│   │   ├── Requests/
│   │   └── Responses/
│   └── Properties/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── cd.yml
├── Dockerfile
└── README.md
```

---

## 🚀 Endpoints

### 💳 Payments Controller (`/api/payments`)

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/payments` | Criar pagamento | User |
| GET | `/api/payments/{id}` | Buscar pagamento | User |
| GET | `/api/payments/{id}/status` | Status do pagamento | User |
| GET | `/api/payments/user/{userId}` | Pagamentos do usuário | User |
| POST | `/api/payments/{id}/process` | Processar pagamento | User |
| POST | `/api/payments/{id}/refund` | Reembolsar pagamento | Admin |
| POST | `/api/payments/{id}/cancel` | Cancelar pagamento | User |

---

## 📊 Modelos de Dados

### Payment Entity

| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único |
| UserId | Guid | ID do usuário |
| GameId | Guid | ID do jogo |
| Amount | decimal | Valor da transação |
| Status | PaymentStatus | Status do pagamento |
| Method | PaymentMethod | Método de pagamento |
| TransactionId | string | ID único da transação |
| CreatedAt | DateTime | Data de criação |
| ProcessedAt | DateTime? | Data de processamento |
| CompletedAt | DateTime? | Data de conclusão |
| FailureReason | string | Motivo da falha (se houver) |

### 💰 Métodos de Pagamento

| Valor | Método | Descrição |
|-------|--------|-----------|
| 0 | CreditCard | Cartão de Crédito |
| 1 | DebitCard | Cartão de Débito |
| 2 | Pix | PIX |
| 3 | BankSlip | Boleto Bancário |
| 4 | PayPal | PayPal |
| 5 | ApplePay | Apple Pay |
| 6 | GooglePay | Google Pay |

### 📈 Status do Pagamento

| Status | Descrição | Pode Transicionar Para |
|--------|-----------|------------------------|
| **Pending** | Aguardando processamento | Processing, Cancelled |
| **Processing** | Em processamento | Completed, Failed |
| **Completed** | Concluído com sucesso | Refunded |
| **Failed** | Falhou | - |
| **Refunded** | Reembolsado | - |
| **Cancelled** | Cancelado | - |

### Fluxo de Status

```
                    ┌─────────────┐
                    │   Pending   │
                    └──────┬──────┘
                           │
           ┌───────────────┼───────────────┐
           │               │               │
           ▼               ▼               ▼
    ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
    │  Cancelled  │ │ Processing  │ │   (wait)    │
    └─────────────┘ └──────┬──────┘ └─────────────┘
                           │
                    ┌──────┴──────┐
                    │             │
                    ▼             ▼
             ┌─────────────┐ ┌─────────────┐
             │  Completed  │ │   Failed    │
             └──────┬──────┘ └─────────────┘
                    │
                    ▼
             ┌─────────────┐
             │  Refunded   │
             └─────────────┘
```

---

## 🔒 Autenticação

Este microsserviço **valida tokens JWT** emitidos pelo **FGC Users API**.

### Endpoints Protegidos (requer User autenticado)
- `POST /api/payments` - Criar pagamento
- `GET /api/payments/{id}` - Buscar pagamento
- `GET /api/payments/{id}/status` - Status
- `GET /api/payments/user/{userId}` - Pagamentos do usuário
- `POST /api/payments/{id}/process` - Processar
- `POST /api/payments/{id}/cancel` - Cancelar

### Endpoints Admin Only
- `POST /api/payments/{id}/refund` - Reembolsar

### Configuração JWT

```json
{
  "Jwt": {
    "SecretKey": "FGC_SuperSecretKey_2024_FIAP_TechChallenge_MinimumLengthRequired32Chars",
    "Issuer": "FGC.Users.API",
    "Audience": "FGC.Client",
    "ExpireMinutes": 120
  }
}
```

> ⚠️ **Importante**: A `SecretKey` deve ser **idêntica** à configurada no Users API.

---

## 🔧 Configuração Local

### Pré-requisitos

- .NET 8.0 SDK
- SQL Server (local ou Azure)
- Visual Studio 2022 ou VS Code

### Executar

```bash
cd FGC.Payments.Presentation
dotnet restore
dotnet run
```

A API estará disponível em: `http://localhost:5003`

### Migrations

```bash
# Criar migration
dotnet ef migrations add InitialCreate -p FGC.Payments.Infrastructure -s FGC.Payments.Presentation

# Aplicar migration
dotnet ef database update -p FGC.Payments.Infrastructure -s FGC.Payments.Presentation
```

---

## 🐳 Docker

### Build

```bash
docker build -t fgc-payments-api .
```

### Run

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="sua_connection_string" \
  -e Jwt__SecretKey="sua_secret_key" \
  fgc-payments-api
```

---

## 📦 Variáveis de Ambiente

| Variável | Descrição | Obrigatório |
|----------|-----------|-------------|
| `ConnectionStrings__DefaultConnection` | Connection string do SQL Server | ✅ |
| `Jwt__SecretKey` | Chave secreta do JWT (min 32 chars) | ✅ |
| `Jwt__Issuer` | Emissor do token | ✅ |
| `Jwt__Audience` | Audiência do token | ✅ |
| `Jwt__ExpireMinutes` | Tempo de expiração em minutos | ✅ |
| `ASPNETCORE_ENVIRONMENT` | Ambiente (Development/Production) | ✅ |

---

## 🔗 Comunicação entre Microsserviços

```
┌─────────────────────┐
│    FGC Users API    │
│   (Autenticação)    │
└─────────┬───────────┘
          │
          │ Token JWT + UserId
          ▼
┌─────────────────────┐         ┌─────────────────────┐
│   FGC Games API     │────────►│  FGC Payments API   │
│    (Catálogo)       │ GameId  │    (Transações)     │
│   :8080 / :5002     │         │   :8080 / :5003     │
└─────────────────────┘         └──────────┬──────────┘
                                           │
                                           │ Futuramente
                                           ▼
                                ┌─────────────────────┐
                                │   Azure Functions   │
                                │  (Processamento     │
                                │   Assíncrono)       │
                                └─────────────────────┘
```

### Este microsserviço:
- ✅ **Valida** tokens JWT do Users API
- ✅ **Referencia** jogos do Games API (via GameId)
- ✅ **Processa** transações de pagamento
- ✅ **Gerencia** status e histórico de pagamentos

### Dependências:
- 🔵 **Azure SQL Database** - Armazenamento de dados
- 🔵 **Azure Container Instance** - Hospedagem
- 🟡 **Azure Functions** - Processamento assíncrono (futuro)

---

## 🚀 CI/CD

### Pipeline CI (Pull Requests)

```yaml
- Checkout do código
- Setup .NET 8.0
- Restore de dependências
- Build da solução
- Execução de testes
```

### Pipeline CD (Push para master)

```yaml
- Checkout do código
- Build e testes
- Login no Azure
- Build da imagem Docker
- Push para Azure Container Registry
- Deploy no Azure Container Instance
- Health check
```

---

## 📍 URLs de Produção

| Ambiente | URL |
|----------|-----|
| **Swagger** | http://fgc-payments-api.eastus2.azurecontainer.io:8080 |
| **Health Check** | http://fgc-payments-api.eastus2.azurecontainer.io:8080/health |
| **Info** | http://fgc-payments-api.eastus2.azurecontainer.io:8080/info |

---

## 🧪 Exemplos de Uso

### 1. Obter Token (no Users API)

```bash
curl -X POST http://fgc-users-api.eastus2.azurecontainer.io:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@fgc.com",
    "password": "Admin@123456"
  }'
```

### 2. Criar Pagamento

```bash
curl -X POST http://localhost:5003/api/payments \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -d '{
    "userId": "aade087c-6e81-4b4d-92cf-77663eac0600",
    "gameId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "amount": 249.90,
    "paymentMethod": 2
  }'
```

### Resposta

```json
{
  "success": true,
  "message": "Pagamento criado com sucesso",
  "data": {
    "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
    "userId": "aade087c-6e81-4b4d-92cf-77663eac0600",
    "gameId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "amount": 249.90,
    "status": "Pending",
    "method": "Pix",
    "transactionId": "TXN-20241208-A1B2C3D4",
    "createdAt": "2024-12-08T10:30:00Z",
    "processedAt": null,
    "completedAt": null,
    "failureReason": null
  }
}
```

### 3. Processar Pagamento

```bash
curl -X POST http://localhost:5003/api/payments/{id}/process \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

### Resposta (Sucesso - 90% de chance)

```json
{
  "success": true,
  "message": "Pagamento processado com sucesso",
  "data": {
    "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
    "status": "Completed",
    "transactionId": "TXN-20241208-A1B2C3D4",
    "processedAt": "2024-12-08T10:30:05Z",
    "completedAt": "2024-12-08T10:30:05Z"
  }
}
```

### Resposta (Falha - 10% de chance)

```json
{
  "success": true,
  "message": "Pagamento falhou: Transação recusada pelo gateway de pagamento",
  "data": {
    "id": "d290f1ee-6c54-4b01-90e6-d701748f0851",
    "status": "Failed",
    "transactionId": "TXN-20241208-A1B2C3D4",
    "failureReason": "Transação recusada pelo gateway de pagamento"
  }
}
```

### 4. Consultar Status

```bash
curl -X GET http://localhost:5003/api/payments/{id}/status \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

### 5. Reembolsar (Admin)

```bash
curl -X POST http://localhost:5003/api/payments/{id}/refund \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

### 6. Histórico do Usuário

```bash
curl -X GET http://localhost:5003/api/payments/user/{userId} \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

---

## 🔮 Funcionalidades

### Azure Functions Integration
- Processamento assíncrono de pagamentos
- Notificações por e-mail/push
- Webhooks para status updates
- Retry automático em falhas

### Event Sourcing
- Registro de todos os eventos de pagamento
- Auditoria completa de transações
- Replay de eventos para debugging

### Gateway de Pagamento Real
- Integração com Stripe/PagSeguro/MercadoPago
- Webhook handlers
- Reconciliação automática

---

# 📐 Arquitetura FIAP Cloud Games (FCG) - Fase 3

## 🏛️ Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                      CLIENTES                                            │
│                                                                                          │
│    ┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐      │
│    │   Web App    │     │  Mobile App  │     │   Swagger    │     │   Postman    │      │
│    └──────┬───────┘     └──────┬───────┘     └──────┬───────┘     └──────┬───────┘      │
│           │                    │                    │                    │               │
└───────────┼────────────────────┼────────────────────┼────────────────────┼───────────────┘
            │                    │                    │                    │
            └────────────────────┴────────────────────┴────────────────────┘
                                          │
                                          ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              AZURE CLOUD INFRASTRUCTURE                                  │
│                                                                                          │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐  │
│  │                           AZURE CONTAINER INSTANCES                                │  │
│  │                                                                                    │  │
│  │   ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐       │  │
│  │   │  🔐 FGC Users API   │  │  🎮 FGC Games API   │  │  💳 FGC Payments API│       │  │
│  │   │                     │  │                     │  │                     │       │  │
│  │   │  ┌───────────────┐  │  │  ┌───────────────┐  │  │  ┌───────────────┐  │       │  │
│  │   │  │ Presentation  │  │  │  │ Presentation  │  │  │  │ Presentation  │  │       │  │
│  │   │  ├───────────────┤  │  │  ├───────────────┤  │  │  ├───────────────┤  │       │  │
│  │   │  │ Application   │  │  │  │ Application   │  │  │  │ Application   │  │       │  │
│  │   │  ├───────────────┤  │  │  ├───────────────┤  │  │  ├───────────────┤  │       │  │
│  │   │  │Infrastructure │  │  │  │Infrastructure │  │  │  │Infrastructure │  │       │  │
│  │   │  ├───────────────┤  │  │  ├───────────────┤  │  │  ├───────────────┤  │       │  │
│  │   │  │    Domain     │  │  │  │    Domain     │  │  │  │    Domain     │  │       │  │
│  │   │  └───────────────┘  │  │  └───────────────┘  │  │  └───────────────┘  │       │  │
│  │   │                     │  │                     │  │                     │       │  │
│  │   │  📍 :8080           │  │  📍 :8080           │  │  📍 :8080           │       │  │
│  │   │  fgc-users-api      │  │  fgc-games-api      │  │  fgc-payments-api   │       │  │
│  │   └──────────┬──────────┘  └──────────┬──────────┘  └──────────┬──────────┘       │  │
│  │              │                        │                        │                  │  │
│  └──────────────┼────────────────────────┼────────────────────────┼──────────────────┘  │
│                 │                        │                        │                     │
│                 └────────────────────────┼────────────────────────┘                     │
│                                          │                                              │
│                                          ▼                                              │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐  │
│  │                              AZURE SQL DATABASE                                    │  │
│  │                                                                                    │  │
│  │   ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐               │  │
│  │   │  📋 Users       │    │  📋 Games       │    │  📋 Payments    │               │  │
│  │   │                 │    │                 │    │                 │               │  │
│  │   │  - Id           │    │  - Id           │    │  - Id           │               │  │
│  │   │  - Name         │    │  - Title        │    │  - UserId       │               │  │
│  │   │  - Email        │    │  - Description  │    │  - GameId       │               │  │
│  │   │  - Password     │    │  - Price        │    │  - Amount       │               │  │
│  │   │  - Role         │    │  - Category     │    │  - Status       │               │  │
│  │   │  - IsActive     │    │  - Developer    │    │  - Method       │               │  │
│  │   │  - CreatedAt    │    │  - Publisher    │    │  - TransactionId│               │  │
│  │   │  - LastLoginAt  │    │  - ReleaseDate  │    │  - CreatedAt    │               │  │
│  │   │                 │    │  - IsActive     │    │  - ProcessedAt  │               │  │
│  │   │                 │    │  - Rating       │    │  - CompletedAt  │               │  │
│  │   │                 │    │  - TotalSales   │    │  - FailureReason│               │  │
│  │   └─────────────────┘    └─────────────────┘    └─────────────────┘               │  │
│  │                                                                                    │  │
│  │   📍 fgc-sql-server.database.windows.net                                          │  │
│  │   📁 fgc-database                                                                 │  │
│  └───────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                          │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐  │
│  │                          AZURE CONTAINER REGISTRY                                  │  │
│  │                                                                                    │  │
│  │   🐳 fgcregistry.azurecr.io                                                       │  │
│  │                                                                                    │  │
│  │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                   │  │
│  │   │ fgc-users-api   │  │ fgc-games-api   │  │fgc-payments-api │                   │  │
│  │   │    :latest      │  │    :latest      │  │    :latest      │                   │  │
│  │   └─────────────────┘  └─────────────────┘  └─────────────────┘                   │  │
│  └───────────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                          │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Fluxo de Comunicação entre Microsserviços

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                          │
│                              FLUXO DE AUTENTICAÇÃO JWT                                   │
│                                                                                          │
│    ┌──────────┐         POST /api/auth/login              ┌──────────────────┐          │
│    │          │ ─────────────────────────────────────────►│                  │          │
│    │  Client  │         { email, password }               │  FGC Users API   │          │
│    │          │◄───────────────────────────────────────── │                  │          │
│    └────┬─────┘         { token: "eyJ..." }               └──────────────────┘          │
│         │                                                                                │
│         │                                                                                │
│         │  Authorization: Bearer eyJ...                                                  │
│         │                                                                                │
│         ├─────────────────────────────────────────────────────────────────┐              │
│         │                                                                 │              │
│         ▼                                                                 ▼              │
│    ┌──────────────────┐                                    ┌──────────────────┐         │
│    │                  │         Valida JWT                 │                  │         │
│    │  FGC Games API   │         (mesma SecretKey)          │FGC Payments API  │         │
│    │                  │                                    │                  │         │
│    └──────────────────┘                                    └──────────────────┘         │
│                                                                                          │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🛒 Fluxo de Compra de Jogo

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                          │
│                               FLUXO DE COMPRA DE JOGO                                    │
│                                                                                          │
│    ┌──────────┐                                                                          │
│    │  Client  │                                                                          │
│    └────┬─────┘                                                                          │
│         │                                                                                │
│         │ 1️⃣ POST /api/auth/login                                                        │
│         ▼                                                                                │
│    ┌──────────────────┐                                                                  │
│    │  FGC Users API   │  ──►  Valida credenciais                                        │
│    │                  │  ──►  Retorna JWT Token                                         │
│    └────────┬─────────┘                                                                  │
│             │                                                                            │
│             │ { token }                                                                  │
│             ▼                                                                            │
│    ┌──────────┐                                                                          │
│    │  Client  │                                                                          │
│    └────┬─────┘                                                                          │
│         │                                                                                │
│         │ 2️⃣ GET /api/games (com Bearer Token)                                           │
│         ▼                                                                                │
│    ┌──────────────────┐                                                                  │
│    │  FGC Games API   │  ──►  Valida JWT                                                │
│    │                  │  ──►  Retorna lista de jogos                                    │
│    └────────┬─────────┘                                                                  │
│             │                                                                            │
│             │ { games[] }                                                                │
│             ▼                                                                            │
│    ┌──────────┐                                                                          │
│    │  Client  │  ──►  Usuário escolhe um jogo                                           │
│    └────┬─────┘                                                                          │
│         │                                                                                │
│         │ 3️⃣ POST /api/payments (com Bearer Token)                                       │
│         │    { userId, gameId, amount, paymentMethod }                                   │
│         ▼                                                                                │
│    ┌──────────────────┐                                                                  │
│    │FGC Payments API  │  ──►  Valida JWT                                                │
│    │                  │  ──►  Cria pagamento (status: Pending)                          │
│    └────────┬─────────┘                                                                  │
│             │                                                                            │
│             │ { payment: { id, status: "Pending" } }                                     │
│             ▼                                                                            │
│    ┌──────────┐                                                                          │
│    │  Client  │                                                                          │
│    └────┬─────┘                                                                          │
│         │                                                                                │
│         │ 4️⃣ POST /api/payments/{id}/process                                             │
│         ▼                                                                                │
│    ┌──────────────────┐                                                                  │
│    │FGC Payments API  │  ──►  Processa pagamento                                        │
│    │                  │  ──►  Atualiza status (Completed/Failed)                        │
│    └────────┬─────────┘                                                                  │
│             │                                                                            │
│             │ { payment: { status: "Completed" } }                                       │
│             ▼                                                                            │
│    ┌──────────┐                                                                          │
│    │  Client  │  ──►  ✅ Compra finalizada!                                              │
│    └──────────┘                                                                          │
│                                                                                          │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Pipeline CI/CD

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                          │
│                                    CI/CD PIPELINE                                        │
│                                                                                          │
│    ┌──────────────────────────────────────────────────────────────────────────────────┐ │
│    │                              GITHUB REPOSITORIES                                  │ │
│    │                                                                                   │ │
│    │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                  │ │
│    │   │ fgc-users-api   │  │ fgc-games-api   │  │fgc-payments-api │                  │ │
│    │   └────────┬────────┘  └────────┬────────┘  └────────┬────────┘                  │ │
│    │            │                    │                    │                           │ │
│    └────────────┼────────────────────┼────────────────────┼───────────────────────────┘ │
│                 │                    │                    │                             │
│                 └────────────────────┼────────────────────┘                             │
│                                      │                                                  │
│                                      ▼                                                  │
│    ┌──────────────────────────────────────────────────────────────────────────────────┐ │
│    │                            GITHUB ACTIONS                                         │ │
│    │                                                                                   │ │
│    │   ┌─────────────────────────────────────────────────────────────────────────┐    │ │
│    │   │                        CI (Pull Requests)                                │    │ │
│    │   │                                                                          │    │ │
│    │   │   📥 Checkout  ──►  🔧 Setup .NET  ──►  📦 Restore  ──►  🏗️ Build  ──►  🧪 Test │ │
│    │   └─────────────────────────────────────────────────────────────────────────┘    │ │
│    │                                                                                   │ │
│    │   ┌─────────────────────────────────────────────────────────────────────────┐    │ │
│    │   │                        CD (Push to master)                               │    │ │
│    │   │                                                                          │    │ │
│    │   │   📥 Checkout  ──►  🏗️ Build  ──►  🧪 Test  ──►  🔐 Azure Login          │    │ │
│    │   │        │                                              │                  │    │ │
│    │   │        ▼                                              ▼                  │    │ │
│    │   │   🐳 Docker Build  ──►  📤 Push ACR  ──►  🚀 Deploy ACI  ──►  🏥 Health  │    │ │
│    │   └─────────────────────────────────────────────────────────────────────────┘    │ │
│    └──────────────────────────────────────────────────────────────────────────────────┘ │
│                                      │                                                  │
│                                      ▼                                                  │
│    ┌──────────────────────────────────────────────────────────────────────────────────┐ │
│    │                              AZURE RESOURCES                                      │ │
│    │                                                                                   │ │
│    │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                  │ │
│    │   │ Container       │  │ Container       │  │   SQL Server    │                  │ │
│    │   │ Registry (ACR)  │  │ Instances (ACI) │  │   Database      │                  │ │
│    │   └─────────────────┘  └─────────────────┘  └─────────────────┘                  │ │
│    └──────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                          │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Recursos Azure

| Recurso | Nome | Tipo | Região |
|---------|------|------|--------|
| Resource Group | `rg-fgc-api` | Resource Group | East US 2 |
| SQL Server | `fgc-sql-server` | Azure SQL Server | East US 2 |
| Database | `fgc-database` | Azure SQL Database | East US 2 |
| Container Registry | `fgcregistry` | Azure Container Registry | East US 2 |
| Container Instance | `fgc-users-container` | Azure Container Instance | East US 2 |
| Container Instance | `fgc-games-container` | Azure Container Instance | East US 2 |
| Container Instance | `fgc-payments-container` | Azure Container Instance | East US 2 |

---

## 🌐 URLs de Produção

| Microsserviço | URL | Swagger |
|---------------|-----|---------|
| **Users API** | http://fgc-users-api.eastus2.azurecontainer.io:8080 | ✅ |
| **Games API** | http://fgc-games-api.eastus2.azurecontainer.io:8080 | ✅ |
| **Payments API** | http://fgc-payments-api.eastus2.azurecontainer.io:8080 | ✅ |

---

## 🔐 Segurança

### JWT Token Flow

```
┌────────────────────────────────────────────────────────────────┐
│                        JWT TOKEN                                │
│                                                                 │
│  Header:     { "alg": "HS256", "typ": "JWT" }                  │
│                                                                 │
│  Payload:    {                                                  │
│                "sub": "user-id",                               │
│                "email": "user@email.com",                      │
│                "role": "Admin",                                │
│                "exp": 1702044800                               │
│              }                                                  │
│                                                                 │
│  Signature:  HMACSHA256(                                       │
│                base64UrlEncode(header) + "." +                 │
│                base64UrlEncode(payload),                       │
│                secret_key                                       │
│              )                                                  │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

### Mesma Secret Key em todos os microsserviços:

```
FGC_SuperSecretKey_2024_FIAP_TechChallenge_MinimumLengthRequired32Chars
```

---

## 📁 Estrutura dos Repositórios

```
GitHub Organization/User
│
├── 📁 fgc-users-api/
│   ├── 📁 .github/workflows/
│   │   ├── ci.yml
│   │   └── cd.yml
│   ├── 📁 FGC.Users.Domain/
│   ├── 📁 FGC.Users.Application/
│   ├── 📁 FGC.Users.Infrastructure/
│   ├── 📁 FGC.Users.Presentation/
│   ├── 🐳 Dockerfile
│   ├── 📄 FGC.Users.sln
│   └── 📖 README.md
│
├── 📁 fgc-games-api/
│   ├── 📁 .github/workflows/
│   │   ├── ci.yml
│   │   └── cd.yml
│   ├── 📁 FGC.Games.Domain/
│   ├── 📁 FGC.Games.Application/
│   ├── 📁 FGC.Games.Infrastructure/
│   ├── 📁 FGC.Games.Presentation/
│   ├── 🐳 Dockerfile
│   ├── 📄 FGC.Games.sln
│   └── 📖 README.md
│
└── 📁 fgc-payments-api/
    ├── 📁 .github/workflows/
    │   ├── ci.yml
    │   └── cd.yml
    ├── 📁 FGC.Payments.Domain/
    ├── 📁 FGC.Payments.Application/
    ├── 📁 FGC.Payments.Infrastructure/
    ├── 📁 FGC.Payments.Presentation/
    ├── 🐳 Dockerfile
    ├── 📄 FGC.Payments.sln
    └── 📖 README.md
```

---

## 📄 Licença

FIAP - Pós-Graduação em Arquitetura de Software .NET

**Tech Challenge - Fase 3**