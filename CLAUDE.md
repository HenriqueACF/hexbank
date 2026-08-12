# HexBank

Backend bancário em arquitetura hexagonal (Ports & Adapters).
Projeto de aprendizado — Claude apenas planeja e explica; o código é escrito pelo Henrique.

## Stack
- .NET 10 / C# 14 · ASP.NET Core 10 · EF Core 10
- PostgreSQL 16 (Docker) · MediatR · FluentValidation · Serilog
- xUnit + Moq + FluentAssertions + Testcontainers

## Estrutura
src/BankingBackend.Core           → Domain puro + interfaces (ports)
src/BankingBackend.Application    → Casos de uso (CQRS handlers)
src/BankingBackend.Infrastructure → Adapters (EF Core, JWT, bcrypt)
src/BankingBackend.API            → Controllers + Composition Root
tests/BankingBackend.Tests.Unit
tests/BankingBackend.Tests.Integration

## Direção das dependências
Core ← Application ← Infrastructure ← API

Regra inviolável: **Core não referencia ninguém.**
`BankingBackend.Core.csproj` deve ter ZERO PackageReference.

## Regras por camada

### Core (Domain)
- Entities com setters privados
- ValueObjects imutáveis e auto-validados
- Criação só por factory method estático (`User.Create(...)`)
- Regras de negócio moram aqui, sempre
- Proibido: atributos de EF Core, `DbContext`, qualquer I/O

### Application
- Commands e Queries como `record` (imutáveis)
- Um handler por caso de uso (MediatR)
- Validação de entrada com FluentValidation
- Retorna DTOs, nunca entidades de domínio
- Proibido: regra de negócio (delega ao Domain)

### Infrastructure
- Implementa as interfaces declaradas no Core
- Mapeamento via Fluent API, um arquivo por entidade
- Proibido: expor `IQueryable` para fora

### API
- Controllers finos: recebe HTTP, chama MediatR, devolve resposta
- Tratamento de erro centralizado em middleware
- `[Authorize]` por atributo

## Ambiente
docker compose up -d      # sobe o PostgreSQL
dotnet build
dotnet test

Connection string local:
Host=localhost;Port=5432;Database=hexbank;Username=hexbank;Password=hexbank123