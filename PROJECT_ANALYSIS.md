# Personal Finance — Project Analysis

_Generated 2026-08-14 from a read-through of the codebase at commit `dc9ad43`._

## 1. What this project is

Two things layered on top of each other:

1. **A Clean Architecture / DDD template for .NET 8.** The README says as much — it's set up as a `dotnet new` template (`dotnet new cleanarch`) that replaces `PersonalFinance` with a target project name. Health checks, Serilog + Seq, Docker/Podman, CI, and three kinds of tests (unit, architecture, smoke) are all scaffolded.
2. **A real domain being built inside that scaffold: a small double-entry bookkeeping / ledger system**, plus a leftover/parallel **ToDo** feature that looks like the original template's "hello world" vertical slice, not yet removed.

The finance domain models **Ledgers → Accounts → Journal Entries (with debit/credit Lines)** — i.e. classic double-entry accounting, not a "track my expenses" app. A `JournalEntry` can only be posted when its lines balance (`debitTotal == creditTotal`), every account referenced must belong to the same ledger, and the posting user must be a ledger member. This is the most developed, best-tested slice of the app.

## 2. Architecture

Four-project Clean Architecture, enforced by actual tests (not just convention):

```
src/
 ├─ PersonalFinance.Domain          → Entities, value objects, domain events. No dependencies on anything else.
 ├─ PersonalFinance.Application     → Use cases (commands + handlers), validators, repository interfaces.
 ├─ PersonalFinance.Infrastructure  → EF Core + Npgsql, repositories, Wolverine event dispatch, DI wiring.
 ├─ PersonalFinance.WebApi          → Minimal API endpoints, Program.cs composition root.
 └─ PersonalFinance.BuildingBlocks  → Shared kernel: AggregateRoot/Entity/ValueObject, Result<T>, errors.

tests/
 ├─ PersonalFinance.ArchitectureTests   → NetArchTest rules that fail the build if layering is violated.
 ├─ PersonalFinance.BuildingBlocks.Tests
 └─ PersonalFinance.WebApi.Tests
```

`tests/PersonalFinance.ArchitectureTests/LayeringTests.cs` asserts Domain/Application must not depend on Infrastructure or WebApi — this is a real, enforced boundary, which is a strong signal of engineering discipline for a project this young.

### Request flow (finance slice, the intended pattern)

```
HTTP request (Minimal API endpoint)
  → build a Command record
  → Wolverine IMessageBus.InvokeAsync(command)   ← in-process mediator, not a message queue here
  → *Handler.HandleAsync (Application layer)
      → FluentValidation validator
      → load aggregates via repository interfaces
      → call domain methods that return Result / Result<T>
      → repository.Add(...) + IUnitOfWork.SaveChangesAsync
  → Result<T> mapped to an HTTP response (result.ToHttp())
```

`PostJournalEntryEndpoint` (`src/PersonalFinance.WebApi/Endpoints/JournalEntries/PostJournalEntryEndpoint.cs`) follows this exactly, going through Wolverine's bus. `CreateToDoEndpoint` does **not** — it resolves `CreateToDoHandler` directly via `[FromServices]` and calls `HandleAsync` itself, bypassing the bus. Two different invocation styles exist side-by-side; see §4.

## 3. Key patterns in use

- **DDD building blocks** (`PersonalFinance.BuildingBlocks/Domain`): `Entity`, `AggregateRoot` (with private `_domainEvents` list, `AddDomainEvent`/`ClearDomainEvents`), `ValueObject`. Standard, minimal, well-tested (`AggregateRootTests.cs`, `ValueObjectTests.cs`).
- **Result pattern instead of exceptions for expected failures** (`BuildingBlocks/Results/Result.cs`, `ResultError.cs`, `ErrorType.cs`). Domain methods (`Account.Create`, `JournalEntry.Post`, `Ledger.RemoveMember`, ...) return `Result`/`Result<T>` with a typed `ErrorType` (Validation/NotFound/Conflict/Problem/Failure), which `Common.cs` in WebApi maps to HTTP status codes via `ToHttp()`. `DomainException` (`BuildingBlocks/Exceptions`) exists as a fallback for truly exceptional cases, caught in endpoints and turned into `Results.Problem`.
- **Rich domain model, not anemic**: aggregates enforce their own invariants through private setters and factory methods (`Account.Create` rejects a `DueDate` on non-Liability accounts; `JournalEntry.Post` checks line count, ledger membership, account/ledger consistency, and balance). Business rules live in the domain, not in handlers.
- **Domain events, dispatched two different ways**:
  - `JournalEntryPostedDomainEvent` / `ToDoItemAddedDomainEvent` are raised inside aggregates via `AddDomainEvent`.
  - `WolverineDomainEventDispatcher` (Infrastructure) publishes them onto Wolverine's in-memory bus via `IDomainEventDispatcher`.
  - **But** `PostJournalEntryHandler` never calls the dispatcher or `ClearDomainEvents()` — the event is created and left sitting on the aggregate, going nowhere. `CreateToDoHandler` does call the dispatcher correctly. See §4.
- **CQRS-flavored vertical slices**: each use case lives in its own folder as `{Verb}{Noun}/` with `Command`, `Handler`, `Validator`, `Response`/`Dto` co-located (e.g. `Application/Finance/JournalEntries/PostJournalEntry/`). Only one slice per aggregate exists so far.
- **Repository + Unit of Work**, interfaces in Application (`IAccountRepository`, `ILedgerRepository`, `IJournalEntryRepository`, `IUnitOfWork`), EF Core implementations in Infrastructure. Thin repositories — no generic base class, each one is hand-written.
- **FluentValidation** for input validation, resolved via DI and run explicitly inside the handler (`PostJournalEntryHandler`), not via a Wolverine/pipeline behavior.
- **EF Core + Npgsql**, snake_case naming convention (`EFCore.NamingConventions`), `IEntityTypeConfiguration<T>` classes per aggregate, code-first migrations already applied (`InitialCreate`, `ChangingEnumTypes`). `PersonalFinanceDbContextFactory` supports `dotnet ef` design-time tooling. Migrations run automatically on startup (`dbContext.Database.MigrateAsync()` in `Program.cs`) — fine for a template/small app, worth revisiting before a real deployment.
- **Minimal APIs**, one static class per endpoint with a `Map*` extension method, composed in `Program.cs`. No versioning, no route grouping yet.
- **Serilog** (console always, Seq in Development via `docker-compose.yml`), health checks at `/health` and `/health/ready`, basic security headers middleware added by hand in `Program.cs` (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`), `UseHsts` outside Development, `AddProblemDetails()`.
- **Centralized package management** (`Directory.Packages.props` + `Directory.Build.props`) — versions pinned in one place across all projects.
- **CI** (`.github/workflows/ci.yml`): restore/build/test against a real Postgres 16 service container on every push/PR to `main`.

## 4. Things worth flagging (not just praise)

These are concrete, verifiable issues found while reading the code — useful to know before building more on top:

1. **The ToDo feature is very likely broken at runtime.** `InfrastructureDependencyInjection.AddInfrastructure` registers `IUnitOfWork`, `ILedgerRepository`, `IAccountRepository`, `IJournalEntryRepository` — but never `IToDoListRepository`, and `CreateToDoHandler` itself is never registered in DI, yet `CreateToDoEndpoint` resolves it via `[FromServices] CreateToDoHandler handler`. `InMemoryToDoListRepository` and `InMemoryUnitOfWork` exist in `Infrastructure/Persistence/InMemory/` but are registered nowhere (`grep` confirms zero references outside their own files and the interface/usage). Calling `POST /api/to-do` will most likely throw a DI resolution error. No test exercises this endpoint (only `RootEndpointTests` — a smoke test for `/`).
2. **Domain events raised in `JournalEntry.Post` are never dispatched.** `PostJournalEntryHandler` calls `journalEntryRepository.Add(entry)` and `unitOfWork.SaveChangesAsync(ct)` but never calls `IDomainEventDispatcher.DispatchAsync` or `entry.ClearDomainEvents()`. Compare with `CreateToDoHandler`, which does both correctly. `JournalEntryPostedDomainEvent` is currently dead — nothing will ever observe a posted journal entry.
3. **Two persistence strategies live side by side** (Postgres/EF for Finance, in-memory dictionary for ToDos) with no shared abstraction or clear rationale documented — reads like the ToDo slice is the original template example that hasn't been deleted now that a real domain (Finance) exists.
4. **Two different in-process invocation styles**: Journal Entry goes through Wolverine's `IMessageBus.InvokeAsync` (so Wolverine's handler-discovery convention is doing real work there); ToDo calls the handler directly. Only one pattern should survive.
5. **No authentication/authorization wired up** despite `Microsoft.AspNetCore.Authentication.JwtBearer` being a pinned package — it's not referenced in `Program.cs` or anywhere else yet. `PostJournalEntryCommand` takes a raw `CreatedByUserId` in the request body, which the caller can set to anyone.
6. **Only one Application-layer use case exists** (`PostJournalEntry`) plus the template's `CreateToDo`. There's no way via the API yet to create a Ledger or an Account — `PostJournalEntryHandler` depends on `ILedgerRepository`/`IAccountRepository` to *read* them, but nothing writes them. The domain models are ahead of the API surface.
7. **Test coverage is thin and lopsided**: solid coverage of `BuildingBlocks` (Result, ValidationError, AggregateRoot, ValueObject) and the architecture rules, but zero tests for the Domain finance entities (`JournalEntry.Post`'s balancing/membership logic is exactly the kind of logic that should have unit tests) and zero tests for `PostJournalEntryHandler` or its endpoint.
8. **`Result<TValue>` has an implicit conversion operator from `TValue?`** (`Result.cs:55`) that treats a `null` value as `Failure<TValue>(ResultError.NullValue)` — easy to trip over if a legitimately-nullable value type is ever wrapped in a `Result<T>` down the line; worth being deliberate about where this operator gets used.
9. **CI runs against Postgres but there don't appear to be any integration tests that actually use it** — the Postgres service container in `ci.yml` is currently paying for infrastructure that isn't being exercised (unless `PersonalFinanceDbContextFactory`/migrations are implicitly checked at `dotnet build`/`dotnet test` time, which wouldn't touch business logic).
10. **README is template documentation, not project documentation** — it explains the `dotnet new` mechanics but says nothing about what "Personal Finance" actually does, its API, or its domain rules. Fine for a template repo, less fine once real domain work has started (see §1).

## 5. Suggested next steps

Roughly in priority order — fix what's silently broken, then decide what the project actually is, then build outward from a solid base.

### A. Fix or remove what's already broken
- Either wire up `IToDoListRepository`/`CreateToDoHandler` in DI properly, or **delete the ToDo slice entirely** if it was only ever the template's example feature. Keeping unregistered, untested, dead-looking code invites confusion for anyone new to the repo (including future-you).
- Fix the missing domain-event dispatch in `PostJournalEntryHandler` (call `dispatcher.DispatchAsync(entry.DomainEvents, ct)` then `entry.ClearDomainEvents()`, matching `CreateToDoHandler`) — or decide domain events aren't needed yet and remove `JournalEntryPostedDomainEvent`/the dispatcher machinery until there's an actual consumer.
- Pick **one** invocation style for Application handlers (Wolverine bus vs. direct DI resolution) and apply it consistently. Given Wolverine is already a dependency and used for the Finance slice, standardizing on `IMessageBus.InvokeAsync` everywhere is the more consistent choice.

### B. Decide the project's identity
- Commit to this being **the real finance domain**, not a template anymore (or keep both, but split them: publish the template separately and let this repo diverge). The mixed README (dotnet-new instructions) plus a growing accounting domain is confusing for contributors.
- If it stays a template-with-an-example, rename the "ToDo" slice to something that clearly signals "this is example code, delete before real use" rather than looking like an abandoned second feature.

### C. Grow the finance domain to be usable end-to-end
- Add the missing write use cases: `CreateLedger`, `AddLedgerMember`, `CreateAccount`, `DeactivateAccount` — right now you can post journal entries but never create the ledgers/accounts they reference through the API.
- Add read/query endpoints (account balances, ledger listing, journal entry history) — everything so far is command-only (write side); there's no way to see the data you just posted.
- Add authentication (the JWT Bearer package is already pinned) and derive `CreatedByUserId` from the authenticated principal instead of trusting the request body — this is a real security gap for a finance app.

### D. Strengthen the foundation before it gets harder to change
- Unit-test the domain layer directly, especially `JournalEntry.Post` (balance check, cross-ledger account rejection, non-member rejection, already-posted rejection) and `Ledger`'s member invariants (owner can't be removed, last member can't be removed) — this is business-critical logic with zero direct coverage today.
- Add integration tests against the real Postgres instance already provisioned in CI (e.g. `WebApplicationFactory` + Testcontainers or the CI Postgres service) covering `POST /api/ledgers/{id}/journal-entry/post` end-to-end, since that's the one fully-wired feature.
- Introduce a consistent validation pipeline (Wolverine supports middleware/behaviors) instead of validators being called by hand inside each handler, so new use cases get validation "for free."
- Replace the manual startup `dbContext.Database.MigrateAsync()` with an explicit migration step in CI/deploy before a real environment is at stake — auto-migrating on every app boot is convenient for a template but risky once there's real user data.
- Write a proper project README once the domain direction is settled: what the ledger/account/journal-entry model represents, example requests, and how it differs from the generic template it started as.
