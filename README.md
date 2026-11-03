# HexBank

Backend bancário construído como projeto de aprendizado, usando arquitetura hexagonal (Ports & Adapters) e DDD em C# / .NET 10.

O objetivo não é o banco em si — é praticar separação de responsabilidades: o domínio (regras de negócio) nunca depende de infraestrutura (banco, JWT, HTTP), e essa direção é garantida pelo compilador, não por disciplina.

## Status

| Etapa | Descrição | Situação |
|---|---|---|
| 1 | Setup da solution, Docker, PostgreSQL | ✅ |
| 2 | Base classes do domínio (`Entity`, `ValueObject`, `Result`, `Error`, `DomainEvent`) | ✅ |
| 3 | Agregado `User`, ValueObjects `Cpf` e `Email` | ✅ |
| 4 | Persistência (EF Core) + autenticação (bcrypt, JWT) | ✅ |
| 5 | `POST /api/v1/auth/login` | ✅ |
| 6 | Cadastro de usuário (`POST /api/v1/auth/register`) | ✅ |
| 7 | Agregado `Account`, ValueObject `Money` | ✅ |
| 8 | Persistência de `Account` + endpoints (abrir conta, depósito, saque) | ✅ |
| 9 | Transferência entre contas (domain service, atomicidade no `IUnitOfWork`) | ✅ |
| 10 | Refresh token (`POST /api/v1/auth/refresh`, rotação e revogação) | ✅ |
| 11 | Índice único em `Account.Number`, erro centralizado, middleware e Serilog | ✅ |

148 testes automatizados (131 unit + 17 integração), todos passando.

## Arquitetura

```
API  →  Application  →  Core (Domain)
                ↖  Infrastructure  ↗
```

A seta aponta para quem se depende. `Core` não referencia nenhum outro projeto — `BankingBackend.Core.csproj` tem **zero** `<PackageReference>`. Isso não é uma promessa, é um fato mecânico: se alguém tentar usar EF Core dentro do domínio, o projeto simplesmente não compila.

- **Core** — entidades, ValueObjects, regras de negócio e as interfaces (`ports`) que descrevem o que o domínio precisa do mundo externo.
- **Application** — casos de uso (CQRS via MediatR), orquestra o domínio e os ports.
- **Infrastructure** — implementa os ports: EF Core, bcrypt, JWT.
- **API** — controllers finos e o *Composition Root* (`Program.cs`), onde as interfaces são finalmente ligadas às implementações.

## Stack

- .NET 10 · C# 14 · ASP.NET Core 10 · Entity Framework Core 10
- PostgreSQL 16 (Docker)
- MediatR (CQRS) · FluentValidation
- BCrypt.Net-Next (hash de senha) · JWT Bearer (autenticação)
- xUnit · FluentAssertions · Moq

## Estrutura

```
src/
├── BankingBackend.Core            → Domain puro + ports
│   ├── Common/                    → Entity, ValueObject, Result, Error, DomainEvent
│   ├── Users/                     → User, Cpf, Email, RefreshToken, interfaces
│   └── Accounts/                  → Account, Money, IAccountRepository
├── BankingBackend.Application     → Casos de uso (handlers MediatR)
│   ├── Users/
│   │   ├── Login/
│   │   ├── Register/
│   │   └── Refresh/
│   └── Accounts/
│       ├── OpenAccount/
│       ├── Deposit/
│       ├── Withdraw/
│       └── Transfer/
├── BankingBackend.Infrastructure  → EF Core, JWT, bcrypt
│   ├── Authentication/
│   └── Persistence/
│       ├── Configurations/        → Fluent API (User, Account, RefreshToken)
│       ├── Repositories/
│       └── Migrations/
└── BankingBackend.API             → Controllers, Middleware, Extensions, Program.cs

tests/
├── BankingBackend.Tests.Unit
└── BankingBackend.Tests.Integration
```

## Como rodar

**Pré-requisitos:** .NET 10 SDK, Docker.

```bash
# 1. Subir o PostgreSQL
docker compose up -d

# 2. Aplicar as migrations
dotnet ef database update -p src/BankingBackend.Infrastructure -s src/BankingBackend.API

# 3. Rodar a API
dotnet run --project src/BankingBackend.API
```

Swagger disponível em `https://localhost:<porta>/swagger` no ambiente de desenvolvimento.

## Testes

```bash
dotnet test
```

## Configuração

As credenciais de desenvolvimento (connection string, chave JWT) ficam em `src/BankingBackend.API/appsettings.Development.json` — não versionado com segredos reais, só valores de uso local.
