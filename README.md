# FGC Payments API

Microsserviço de Pagamentos da plataforma FIAP Cloud Games (FCG).

## 📋 Descrição

Este microsserviço é responsável pelo processamento de pagamentos na plataforma FCG.

## 🏗️ Arquitetura

```
FGC.Payments.Domain/          → Entidades, Enums, Eventos, Interfaces
FGC.Payments.Application/     → DTOs, Use Cases
FGC.Payments.Infrastructure/  → Repositórios, DbContext, Configurações
FGC.Payments.Presentation/    → Controllers, Models, Program.cs
```

## 🚀 Endpoints

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/api/payments` | Criar pagamento | User |
| GET | `/api/payments/{id}` | Buscar pagamento | User |
| GET | `/api/payments/{id}/status` | Status do pagamento | User |
| GET | `/api/payments/user/{userId}` | Pagamentos do usuário | User |
| POST | `/api/payments/{id}/process` | Processar pagamento | User |
| POST | `/api/payments/{id}/refund` | Reembolsar pagamento | Admin |
| POST | `/api/payments/{id}/cancel` | Cancelar pagamento | User |

## 💳 Métodos de Pagamento

| Valor | Método |
|-------|--------|
| 0 | CreditCard |
| 1 | DebitCard |
| 2 | Pix |
| 3 | BankSlip |
| 4 | PayPal |
| 5 | ApplePay |
| 6 | GooglePay |

## 📊 Status do Pagamento

| Status | Descrição |
|--------|-----------|
| Pending | Aguardando processamento |
| Processing | Em processamento |
| Completed | Concluído com sucesso |
| Failed | Falhou |
| Refunded | Reembolsado |
| Cancelled | Cancelado |

## 🔧 Configuração Local

### Pré-requisitos
- .NET 8.0 SDK
- SQL Server (local ou Azure)

### Executar
```bash
cd FGC.Payments.Presentation
dotnet restore
dotnet run
```

### Migrations
```bash
dotnet ef migrations add InitialCreate -p FGC.Payments.Infrastructure -s FGC.Payments.Presentation
dotnet ef database update -p FGC.Payments.Infrastructure -s FGC.Payments.Presentation
```

## 🐳 Docker

```bash
docker build -t fgc-payments-api .
docker run -p 8080:8080 fgc-payments-api
```

## 📦 Variáveis de Ambiente

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__DefaultConnection` | Connection string do SQL Server |
| `Jwt__SecretKey` | Chave secreta do JWT (min 32 chars) |
| `Jwt__Issuer` | Emissor do token |
| `Jwt__Audience` | Audiência do token |
| `Jwt__ExpireMinutes` | Tempo de expiração em minutos |

## 🔗 Integração

Este microsserviço se comunica com:
- **FGC Users API** → Validação de usuários
- **FGC Games API** → Validação de jogos

## 📄 Licença

FIAP - Pós-Graduação em Arquitetura de Software .NET