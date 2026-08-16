# Personal Finance API

A personal finance management backend built with .NET 10, following Clean Architecture and Domain-Driven Design principles. The system is currently at MVP stage and is implemented as a modular monolith.

## Overview

The API allows a user to track accounts, categorize income and expenses, post journal entries, set monthly budgets, and retrieve financial reports. Administrative users can manage other user accounts.

## Architecture

The solution is organized into four architectural layers plus a shared building-blocks project:

```
src/
  PersonalFinance.Domain          Entities, value objects, domain events, business rules
  PersonalFinance.Application     Use cases, command/query handlers, application contracts
  PersonalFinance.Infrastructure  EF Core persistence, security, seeding, external integrations
  PersonalFinance.WebApi          Minimal API endpoints, composition root
  PersonalFinance.BuildingBlocks  Shared abstractions (Result, AggregateRoot, exceptions)

tests/
  PersonalFinance.Domain.Tests
  PersonalFinance.Application.Tests
  PersonalFinance.BuildingBlocks.Tests
  PersonalFinance.WebApi.Tests
  PersonalFinance.ArchitectureTests
```

Dependencies flow inward: `WebApi` and `Infrastructure` depend on `Application`, which depends on `Domain`. Layering rules are enforced by `PersonalFinance.ArchitectureTests` using NetArchTest.

## Technology Stack

- .NET 10 / ASP.NET Core Minimal APIs
- WolverineFX as the in-process command/query mediator
- PostgreSQL with Entity Framework Core (Npgsql provider)
- FluentValidation for input validation
- Serilog for structured logging, with Seq as the local sink
- JWT Bearer authentication and role-based authorization
- OpenAPI/Swagger for API documentation (Development only)
- xUnit and Shouldly for testing; NetArchTest for architecture enforcement

## Prerequisites

- .NET 10 SDK
- PostgreSQL 16 (or Docker/Podman to run it as a container)

## Running Locally

1. Ensure a PostgreSQL instance is reachable and update the `ConnectionStrings:DefaultConnection` value in `src/PersonalFinance.WebApi/appsettings.Development.json` if needed.
2. Restore, build, and run the API:

   ```bash
   dotnet restore
   dotnet build
   dotnet run --project src/PersonalFinance.WebApi
   ```

Database migrations are applied automatically at startup. An administrator account is seeded from the `Admin` configuration section when the configured username does not already exist.

The API listens on `http://localhost:5173` by default (see `Properties/launchSettings.json`).

## Running with Docker / Podman

```bash
podman compose up --build
```

This starts the API and a Seq instance for log inspection.

- API: `http://localhost:8080`
- Seq: `http://localhost:5341`

## Configuration

Configuration is provided through `appsettings.json`, environment-specific overrides, and environment variables. Key sections:

| Section                            | Purpose                                            |
| ----------------------------------- | --------------------------------------------------- |
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string                       |
| `Jwt`                                | Issuer, audience, signing key, and token expiry    |
| `Admin`                              | Credentials used to seed the initial administrator |
| `Serilog`                            | Logging sinks and minimum levels                   |

The values committed under `appsettings.json` and `appsettings.Development.json` are placeholders intended for local development only. Production deployments must supply their own secrets through environment variables or a secret store, and must never reuse the committed signing key or admin credentials.

## Health Checks

```
GET /health
GET /health/ready
```

## Logging

Serilog writes to the console by default. When running through Docker Compose, logs are also shipped to Seq for structured querying.

## Testing

```bash
dotnet test
```

The test suite covers domain logic, application handlers, API behavior (including integration tests against a real PostgreSQL instance), and architectural layering rules.

## Continuous Integration

GitHub Actions runs on every push to `main` and on every pull request (`.github/workflows/ci.yml`). The pipeline provisions a PostgreSQL service container and executes, in order: restore, build (Release), and test (Release).

## License

Distributed under the MIT License. See [LICENSE](LICENSE) for details.
