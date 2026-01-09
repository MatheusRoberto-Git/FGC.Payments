# 💳 FGC Payments API

Microsserviço de Pagamentos da plataforma **FIAP Cloud Games (FCG)**.

## 📋 Descrição

Este microsserviço é responsável pelo processamento de pagamentos, gerenciamento de transações e controle de status de compras na plataforma FCG. Integra com **Azure Service Bus** para comunicação assíncrona e notificações de pagamentos processados.

---

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture**, separando responsabilidades em camadas:

```
┌──────────────────────────────────────────────────────────-───┐
│                  FGC.Payments.Presentation                   │
│              (Controllers, Models, Program.cs)               │
├───────────────────────────────────────────────────────────-──┤
│                  FGC.Payments.Application                    │
│                    (DTOs, Use Cases)                         │
├────────────────────────────────────────────────────────────-─┤
│                 FGC.Payments.Infrastructure                  │
│         (Repositories, DbContext, Messaging)                 │
├─────────────────────────────────────────────────────────────-┤
│                    FGC.Payments.Domain                       │
│             (Entities, Enums, Interfaces)                    │
└─────────────────────────────────────────────────────────────-┘
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
│   ├── Interfaces/
│   │   └── IMessagePublisher.cs
│   └── UseCases/
├── FGC.Payments.Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   └── Context/
│   ├── Messaging/
│   │   └── ServiceBusPublisher.cs
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
                    │  📬 Service Bus
                    │  (Notificação)
                    ▼
             ┌─────────────┐
             │  Refunded   │
             └─────────────┘
```

---

## 📬 Azure Service Bus - Mensageria Assíncrona (Fase 4)

Na Fase 4, implementamos comunicação assíncrona usando **Azure Service Bus** para notificar outros serviços quando um pagamento é processado.

### Fluxo de Mensageria

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                         FLUXO DE MENSAGERIA ASSÍNCRONA                                  │
│                                                                                         │
│    ┌──────────┐      POST /api/payments/{id}/process      ┌──────────────────┐          │
│    │  Client  │ ─────────────────────────────────────────►│  Payments API    │          │
│    └──────────┘                                           └────────┬─────────┘          │
│                                                                    │                    │
│                                                         1. Processa pagamento           │
│                                                         2. Atualiza status              │
│                                                         3. Publica mensagem             │
│                                                                    │                    │
│                                                                    ▼                    │
│                                                           ┌──────────────────┐          │
│                                                           │  Azure Service   │          │
│                                                           │      Bus         │          │
│                                                           │                  │          │
│                                                           │  Queue:          │          │
│                                                           │  payment-        │          │
│                                                           │  notifications   │          │
│                                                           └────────┬─────────┘          │
│                                                                    │                    │
│                                                                    │ Consome mensagem   │
│                                                                    ▼                    │
│                                                           ┌──────────────────┐          │
│                                                           │  Azure Function  │          │
│                                                           │   (Consumer)     │          │
│                                                           │                  │          │
│                                                           │  • Notificações  │          │
│                                                           │  • E-mail        │          │
│                                                           │  • Webhooks      │          │
│                                                           │  • Integrações   │          │
│                                                           └──────────────────┘          │
│                                                                                         │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

### Interface IMessagePublisher

```csharp
public interface IMessagePublisher
{
    Task PublishPaymentProcessedAsync(PaymentProcessedMessage message);
}

public record PaymentProcessedMessage(
    Guid PaymentId,
    Guid UserId,
    Guid GameId,
    decimal Amount,
    string Status,
    DateTime ProcessedAt
);
```

### Implementação ServiceBusPublisher

```csharp
public class ServiceBusPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public async Task PublishPaymentProcessedAsync(PaymentProcessedMessage message)
    {
        var json = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(json);
        
        await _sender.SendMessageAsync(serviceBusMessage);
        
        _logger.LogInformation(
            "✅ Mensagem publicada no Service Bus. PaymentId: {PaymentId}, Status: {Status}",
            message.PaymentId, message.Status);
    }
}
```

### Exemplo de Mensagem Publicada

```json
{
  "PaymentId": "52453aa9-a78e-47ed-a831-9fa1326a6a00",
  "UserId": "541d9aca-0619-47d4-bc6d-cf64ba74f4fe",
  "GameId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Amount": 99.90,
  "Status": "Completed",
  "ProcessedAt": "2026-01-08T22:43:37.5861524Z"
}
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
    "SecretKey": "FGC_SuperSecretKey_2024_MinLength32Chars!",
    "Issuer": "FGC.Users.API",
    "Audience": "FGC.Client",
    "ExpireMinutes": 120
  }
}
```

> ⚠️ **Importante**: A `SecretKey` deve ser **idêntica** à configurada no Users API.

---

## 🐳 Docker - Imagem Otimizada (Fase 4)

O Dockerfile foi otimizado na **Fase 4** com as seguintes melhorias:

| Melhoria | Antes | Depois |
|----------|-------|--------|
| **Tamanho da imagem** | ~93 MB | ~65 MB |
| **Imagem base** | aspnet:8.0 | aspnet:8.0-alpine |
| **Usuário** | root | appuser (non-root) |
| **Health check** | Não tinha | Integrado |

### Dockerfile Otimizado

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
# ... build steps

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime (Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
RUN addgroup -g 1000 appgroup && adduser -u 1000 -G appgroup -D appuser
USER appuser
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1
```

### Build & Run

```bash
# Build
docker build -t fgc-payments-api .

# Run
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="sua_connection_string" \
  -e Jwt__SecretKey="sua_secret_key" \
  -e ServiceBus__ConnectionString="sua_servicebus_connection" \
  fgc-payments-api
```

---

## ☸️ Kubernetes (AKS) - Fase 4

Na Fase 4, o microsserviço foi migrado para **Azure Kubernetes Service (AKS)**.

### Recursos Kubernetes

| Recurso | Descrição |
|---------|-----------|
| **Deployment** | Gerencia os pods da aplicação |
| **Service (ClusterIP)** | Exposição interna |
| **Service (LoadBalancer)** | Exposição externa com IP público |
| **HPA** | Auto scaling baseado em CPU (1-5 pods) |
| **ConfigMap** | Configurações não sensíveis |
| **Secret** | Dados sensíveis (connection strings, JWT, Service Bus) |

### HPA (Horizontal Pod Autoscaler)

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: payments-api-hpa
  namespace: fgc
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: payments-api
  minReplicas: 1
  maxReplicas: 5
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

### Comandos Úteis

```bash
# Ver pods
kubectl get pods -n fgc

# Ver logs
kubectl logs -n fgc deployment/payments-api

# Ver logs do Service Bus
kubectl logs -n fgc deployment/payments-api | grep "Service Bus"

# Ver HPA
kubectl get hpa -n fgc

# Escalar manualmente
kubectl scale deployment/payments-api --replicas=3 -n fgc
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
| `ServiceBus__ConnectionString` | Connection string do Azure Service Bus | ✅ |
| `ServiceBus__QueueName` | Nome da fila (payment-notifications) | ✅ |
| `ApplicationInsights__ConnectionString` | APM monitoring | ⬜ |

---

# 📐 Arquitetura FIAP Cloud Games (FCG) - Fase 4

## 🏛️ Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                                      CLIENTES                                           │
│    ┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐      │
│    │   Web App    │     │  Mobile App  │     │   Swagger    │     │   Postman    │      │
│    └──────┬───────┘     └──────┬───────┘     └──────┬───────┘     └──────┬───────┘      │
└───────────┼────────────────────┼────────────────────┼────────────────────┼──────────────┘
            └────────────────────┴────────────────────┴────────────────────┘
                                          │
                                          ▼
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              AZURE CLOUD INFRASTRUCTURE                                 │
│                                                                                         │
│  ┌───────────────────────────────────────────────────────────────────────────────────┐  │
│  │                        AZURE KUBERNETES SERVICE (AKS)                             │  │
│  │                            fgc-aks-cluster                                        │  │
│  │                                                                                   │  │
│  │   ┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐       │  │
│  │   │  🔐 FGC Users API   │  │  🎮 FGC Games API   │  │  💳 FGC Payments API│      │  │
│  │   │      (Pod)          │  │      (Pod)          │  │      (Pod)          │       │  │
│  │   │   HPA: 1-5 pods     │  │   HPA: 1-5 pods     │  │   HPA: 1-5 pods     │       │  │
│  │   │   CPU target: 70%   │  │   CPU target: 70%   │  │   CPU target: 70%   │       │  │
│  │   │                     │  │                     │  │                     │       │  │
│  │   │  📍 LoadBalancer    │  │  📍 ClusterIP       │  │  📍 LoadBalancer   │       │  │
│  │   │  68.220.143.16      │  │  (interno)          │  │  128.85.227.213     │       │  │
│  │   └──────────┬──────────┘  └──────────┬──────────┘  └──────────┬──────────┘       │  │
│  └──────────────┼────────────────────────┼────────────────────────┼──────────────────┘  │
│                 │                        │                        │                     │
│                 └────────────────────────┼────────────────────────┘                     │
│                                          │                                              │
│                            ┌─────────────┴─────────────┐                                │
│                            ▼                           ▼                                │
│  ┌─────────────────────────────────────┐  ┌─────────────────────────────────────-────┐  │
│  │        AZURE SQL DATABASE           │  │         AZURE SERVICE BUS                │  │
│  │   📍 fgc-sql-server.database.       │  │   📍 fgc-servicebus                     │  │
│  │      windows.net                    │  │   📬 Queue: payment-notifications        │  │
│  │   📁 fgc-database                   │  │                                          │  │
│  │                                     │  │   Payments API ──► Service Bus ──►       │  │
│  │   ┌─────────┐ ┌─────────┐ ┌───────┐ │  │                    Azure Function        │  │
│  │   │ Users   │ │ Games   │ │Payments│ │  │                    (Consumer)           │  │
│  │   └─────────┘ └─────────┘ └───────┘ │  │                                          │  │
│  └─────────────────────────────────────┘  └─────────────────────────────────────────-┘  │
│                                                                                         │
│  ┌─────────────────────────────────────┐  ┌─────────────────────────────────────────-┐  │
│  │      AZURE CONTAINER REGISTRY       │  │       APPLICATION INSIGHTS (APM)         │  │
│  │   🐳 fgcregistry.azurecr.io         │  │   📊 Métricas de performance            │  │
│  │   • fgc-users-api:latest            │  │   📈 Logs e traces distribuídos          │  │
│  │   • fgc-games-api:latest            │  │   🔍 Monitoramento em tempo real         │  │
│  │   • fgc-payments-api:latest         │  │                                          │  │
│  └─────────────────────────────────────┘  └─────────────────────────────────────────-┘  │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔗 Comunicação entre Microsserviços

```
                    ┌─────────────────────┐
                    │    FGC Users API    │
                    │   (Autenticação)    │
                    │  68.220.143.16      │
                    └─────────┬───────────┘
                              │
                    Gera Token JWT
                              │
           ┌──────────────────┼──────────────────┐
           │                  │                  │
           ▼                  ▼                  ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│  FGC Games API  │ │FGC Payments API │ │  Outros Clients │
│   (Catálogo)    │ │  (Transações)   │ │   (Frontend)    │
│  ClusterIP      │ │ 128.85.227.213  │ │                 │
└─────────────────┘ └────────┬────────┘ └─────────────────┘
                             │
                             │ Publica mensagem
                             ▼
                   ┌─────────────────┐
                   │  Service Bus    │
                   │  (Mensageria)   │
                   │                 │
                   │  payment-       │
                   │  notifications  │
                   └────────┬────────┘
                            │
                            │ Consome
                            ▼
                   ┌─────────────────┐
                   │ Azure Function  │
                   │  (Consumer)     │
                   └─────────────────┘
```

### Este microsserviço:
- ✅ **Valida** tokens JWT do Users API
- ✅ **Referencia** jogos do Games API (via GameId)
- ✅ **Processa** transações de pagamento
- ✅ **Publica** mensagens no Azure Service Bus
- ✅ **Gerencia** status e histórico de pagamentos

### Dependências:
- 🔵 **Azure SQL Database** - Armazenamento de dados
- 🔵 **Azure Kubernetes Service** - Orquestração de containers
- 🔵 **Azure Service Bus** - Mensageria assíncrona
- 🔵 **Application Insights** - Monitoramento APM

---

## 🔧 Pipeline CI/CD

```
┌───────────────────────────────────────────────────────────────────────────────────────-──┐
│                                    CI/CD PIPELINE                                        │
│                                                                                          │
│    ┌──────────────────────────────────────────────────────────────────────────────────-┐ │
│    │                              GITHUB REPOSITORIES                                  │ │
│    │   ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                   │ │
│    │   │ fgc-users-api   │  │ fgc-games-api   │  │fgc-payments-api │                   │ │
│    │   └────────┬────────┘  └────────┬────────┘  └────────┬────────┘                   │ │
│    └────────────┼────────────────────┼────────────────────┼───────────────────────────-┘ │
│                 └────────────────────┼────────────────────┘                              │
│                                      ▼                                                   │
│    ┌──────────────────────────────────────────────────────────────────────────────────-┐ │
│    │                            GITHUB ACTIONS                                         │ │
│    │                                                                                   │ │
│    │   ┌────────────────────────────────────────────────────────────────────────-─┐    │ │
│    │   │  CI (Pull Requests)                                                      │    │ │
│    │   │  📥 Checkout ──► 🔧 Setup .NET ──► 📦 Restore ──► 🏗️ Build ──► 🧪 Test │    │ │
│    │   └─────────────────────────────────────────────────────────────────────────-┘    │ │
│    │                                                                                   │ │
│    │   ┌─────────────────────────────────────────────────────────────────────────-┐    │ │
│    │   │  CD (Push to master)                                                     │    │ │
│    │   │  📥 Checkout ──► 🏗️ Build ──► 🧪 Test ──► 🔐 Azure Login                │    │ │
│    │   │       │                                         │                        │    │ │
│    │   │       ▼                                         ▼                        │    │ │
│    │   │  🐳 Docker Build ──► 📤 Push ACR ──► 🚀 Deploy ──► 🏥 Health Check      │    │ │
│    │   └─────────────────────────────────────────────────────────────────────────-┘    │ │
│    └──────────────────────────────────────────────────────────────────────────────-────┘ │
└────────────────────────────────────────────────────────────────────────────────────-─────┘
```

---

## 📊 Recursos Azure - Fase 4

| Recurso | Nome | Tipo | Região |
|---------|------|------|--------|
| Resource Group | `rg-fgc-api` | Resource Group | East US 2 |
| AKS Cluster | `fgc-aks-cluster` | Azure Kubernetes Service | East US 2 |
| SQL Server | `fgc-sql-server` | Azure SQL Server | East US 2 |
| Database | `fgc-database` | Azure SQL Database | East US 2 |
| Container Registry | `fgcregistry` | Azure Container Registry | East US 2 |
| Service Bus | `fgc-servicebus` | Azure Service Bus | East US 2 |
| Service Bus Queue | `payment-notifications` | Queue | East US 2 |
| App Insights | `fgc-appinsights` | Application Insights | East US 2 |

---

## 🌐 URLs de Produção (Kubernetes)

| Microsserviço | URL | Swagger |
|---------------|-----|---------|
| **Users API** | http://68.220.143.16 | ✅ |
| **Games API** | Interno (ClusterIP) | - |
| **Payments API** | http://128.85.227.213 | ✅ |

---

## 🧪 Exemplos de Uso

### 1. Obter Token (no Users API)

```bash
curl -X POST http://68.220.143.16/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@fgc.com",
    "password": "Admin@123456"
  }'
```

### 2. Criar Pagamento

```bash
curl -X POST http://128.85.227.213/api/payments \
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
    "createdAt": "2024-12-08T10:30:00Z"
  }
}
```

### 3. Processar Pagamento (dispara mensagem no Service Bus)

```bash
curl -X POST http://128.85.227.213/api/payments/{id}/process \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

### Resposta (Sucesso)

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

> 📬 **Nota**: Após o processamento, uma mensagem é automaticamente publicada no Azure Service Bus na fila `payment-notifications`.

### 4. Verificar Mensagem no Service Bus

No Portal Azure:
1. Acesse **Service Bus** → `fgc-servicebus`
2. Vá em **Queues** → `payment-notifications`
3. Clique em **Service Bus Explorer**
4. Visualize as mensagens na fila

### 5. Consultar Status

```bash
curl -X GET http://128.85.227.213/api/payments/{id}/status \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

### 6. Reembolsar (Admin)

```bash
curl -X POST http://128.85.227.213/api/payments/{id}/refund \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

---

## 🔮 Funcionalidades Implementadas

### ✅ Fase 3
- Clean Architecture
- CI/CD com GitHub Actions
- Deploy em Azure Container Instances
- Autenticação JWT compartilhada

### ✅ Fase 4
- Dockerfile otimizado (Alpine, non-root, health check)
- Migração para Azure Kubernetes Service (AKS)
- HPA (Horizontal Pod Autoscaler)
- **Azure Service Bus** - Mensageria assíncrona
- Application Insights (APM)

---

## 📋 Checklist da Fase 4

| Requisito | Status |
|-----------|--------|
| ✅ Dockerfiles otimizados (Alpine, non-root) | Implementado |
| ✅ Cluster Kubernetes (AKS) | Implementado |
| ✅ Deployments e Services | Implementado |
| ✅ HPA (Auto Scaling 1-5 pods, CPU 70%) | Implementado |
| ✅ **Comunicação Assíncrona (Azure Service Bus)** | **Implementado** |
| ✅ APM (Application Insights) | Implementado |

---

## 📄 Licença

FIAP - Pós-Graduação em Arquitetura de Software .NET

**Tech Challenge - Fase 4**