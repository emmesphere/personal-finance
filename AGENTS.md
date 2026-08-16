# AGENTS.md

## Project overview



## General principles

* Prefer small, focused changes.
* Follow existing patterns before introducing new abstractions.
* Do not refactor unrelated code.
* Do not rename or move files unless necessary for the requested change.
* Preserve existing public APIs unless a breaking change is explicitly requested.
* Avoid speculative abstractions.
* Prefer readable code over clever code.

## Backend

The backend owns:

* business rules
* authorization
* data persistence
* database access
* validation of untrusted input
* integration with external services

### Architecture philosophy

This project is currently an MVP.

Optimize for:

1. correctness
2. simplicity
3. maintainability
4. development speed
5. observability

Do not optimize prematurely for hypothetical scale.

Use a modular monolith by default.

Preserve clear module boundaries so individual capabilities can be extracted
into services later if there is a concrete reason.

Do not introduce microservices solely for architectural purity.

A service extraction should be justified by at least one concrete requirement,
such as:

- independent scaling
- independent deployment
- strong isolation requirement
- different reliability requirements
- asynchronous workload
- clear team ownership boundary
- significantly different technology requirements

### Engineering principles

Apply these principles pragmatically:

1. KISS
2. YAGNI
3. SOLID
4. DRY

Prefer the simplest design that correctly solves the current requirement.

Do not introduce abstractions for hypothetical future requirements.

Create interfaces when they provide a real boundary, multiple implementations,
testability benefit, or architectural value.

Do not create interfaces mechanically for every class.

Prefer duplication of a small amount of simple code over a premature
abstraction coupling unrelated concepts.

Refactor when repeated patterns reveal a stable abstraction.

### Abstractions

Do not create abstractions based only on code similarity.

An abstraction should represent a meaningful domain or architectural concept.

Avoid:

- generic repositories without a concrete need
- generic services
- base controllers
- unnecessary wrapper classes
- one-implementation interfaces created only for convention

Prefer concrete implementations until an abstraction provides clear value.

### Architectural decisions

- Use PostgreSQL as the primary database.
- Use EF Core for persistence.
- Use ASP.NET Core Minimal APIs.
- Use Serilog for structured logging.
- Use OpenTelemetry for telemetry.
- Start as a modular monolith.
- Do not introduce Kubernetes during the MVP phase.

## Shared contracts

When API contracts change:

1. Update the backend contract.
2. Update affected frontend types or generated clients.
3. Check all consumers.
4. Update tests.
5. Avoid silently introducing breaking changes.

Do not duplicate domain concepts unnecessarily between frontend and backend.

## Dependencies

Before adding a new dependency:

1. Check whether the repository already contains a dependency that solves the problem.
2. Prefer platform/framework functionality when appropriate.
3. Avoid adding dependencies for trivial functionality.
4. Avoid replacing existing libraries without an explicit reason.

Do not perform major framework or dependency upgrades unless explicitly requested.

## Configuration and secrets

Never commit:

* passwords
* database connection strings containing credentials
* API keys
* access tokens
* private certificates
* production secrets

Use environment variables or the existing configuration mechanism.

When adding a new configuration value:

* document it
* provide a safe development example when appropriate
* fail clearly when required configuration is missing

## Database safety

Never:

* delete production data
* drop databases
* reset databases
* remove existing migrations
* rewrite migration history

unless explicitly requested.

Schema changes must use the project's migration mechanism.

For destructive migrations, call attention to potential data loss.

## Git

Do not:

* force push
* rewrite Git history
* delete branches
* commit secrets
* modify unrelated files

unless explicitly requested.

Keep changes scoped to the task.

## Definition of done

Before considering a task complete:

* relevant code builds successfully
* affected tests pass
* static analysis/linting passes where configured
* no unrelated files were changed
* public contracts remain compatible unless explicitly changed
* new behavior has appropriate tests
* database schema changes include migrations
* configuration changes are documented

If a validation command cannot be run, state which command was not executed and why.
