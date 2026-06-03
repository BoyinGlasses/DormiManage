# Agent Session Context

Use this file as a compact bootstrap for a fresh agent session. Keep `AGENTS.md` as the rules source; use this document as the project map and task-loading guide.

## Current Project Shape

- Product: WPF desktop app for student dormitory management.
- Runtime: .NET 10, C# nullable reference types, implicit usings.
- UI: WPF MVVM in `src/DormitoryManagement.WPF`, MahApps.Metro, ScottPlot.WPF dashboard charts.
- Data: EF Core 10 + SQL Server in `src/DormitoryManagement.Infrastructure`.
- Tests: xUnit under `tests/`, split into Application and Infrastructure projects.
- Primary domain: students, accounts, rooms, registrations, contracts, billing, payments, vehicles, support tickets, dashboards, audit logs, forum support module.

## Load Order For New Work

1. Read `AGENTS.md` first for hard rules.
2. Read `docs/AgentMemory.md` and automatically recall relevant `D:\Dormitory` memory if `agentmemory` is available.
3. Read only the relevant docs section:
   - Architecture/dependency work: `docs/Architecture.md`
   - Build/migration/seed workflow: `docs/DevelopmentGuide.md`
   - Auth/authz/audit behavior: `docs/Security.md`
   - Data model: `docs/DatabaseSchema.md`
   - Feature behavior: `docs/UseCases.md`
4. Read the file to edit.
5. Read its matching test file when one exists.
6. Read one similar implementation in the same layer.
7. If docs and code conflict, surface the conflict before changing behavior.

## Cross-Session Memory

- Memory rules live in `docs/AgentMemory.md`.
- On fresh sessions, use the memory recall step before task-specific file loading.
- When the user says `tom tat va luu lai` or equivalent, save a concise session summary through `agentmemory`.
- Save completed-task summaries when they would reduce future context recap.
- At about 80% context usage, save an in-progress session summary before continuing substantial work.
- After any context compaction or patch during active work, recall recent Dormitory summaries, reconcile with source files, and continue the task.
- If a task crossed the 80% threshold or resumed after compaction, save a final session summary after verification.
- Recalled memory is advisory; verify against current files before editing.

## Layer Map

- `src/DormitoryManagement.Domain`
  - Entities, enums, constants, basic invariants.
  - No project references.
- `src/DormitoryManagement.Application`
  - DTOs, validation, authz, service interfaces/implementations, transaction boundaries.
  - References Domain only.
  - Key folders: `Services`, `DTOs`, `Abstractions`, `Security`, `Validation`, `Common`.
- `src/DormitoryManagement.Infrastructure`
  - EF Core `DormitoryDbContext`, entity configurations, repositories, unit of work, migrations, seed data, password hashing, notification/audit persistence.
  - References Domain and Application.
- `src/DormitoryManagement.WPF`
  - Views, ViewModels, commands, converters, resources, navigation, app bootstrap, DI composition.
  - References Domain, Application, Infrastructure.

## Patterns To Follow

- Application services use `public sealed class XService : IXService`.
- Services inject Application abstractions such as repositories, `IUnitOfWork`, auth/session services, audit/notification services, and `IDateTimeProvider`.
- Request models are validated with `RequestValidator.ValidateAndThrow(...)`.
- Authorization belongs in Application services, not hidden only in WPF.
- Persisting changes usually goes through repository update/add methods plus `IUnitOfWork.SaveChangesAsync(ct)`.
- Security/audit-sensitive actions should call `IAuditLogService.WriteAsync(...)`.
- WPF ViewModels inherit `ViewModelBase`, use `SetProperty`, expose `RelayCommand`/`AsyncRelayCommand`, and navigate through `INavigationService`.
- WPF ViewModels call Application service interfaces only; never call `DormitoryDbContext` directly.
- XAML should reuse resources in `src/DormitoryManagement.WPF/Resources`.
- Infrastructure repositories implement Application repository abstractions and stay behind DI.
- Tests for Application services usually use local fakes/test doubles; Infrastructure tests use EF context setup.

## Hard Boundaries

- This is not an ASP.NET Core API.
- Do not add REST controllers, JWT flows, API rate limiting, or web API assumptions unless explicitly requested.
- Passwords must remain PBKDF2-SHA256 hashes; never persist plain text.
- Students access only their own data.
- Building Managers are scoped to assigned buildings.
- Staff update assigned tickets only.
- Admins administer the system.
- Never commit `appsettings.Development.json` or personal connection strings.

## Commands

Run from `src/` unless noted.

```powershell
dotnet restore DormitoryManagement.sln
dotnet build DormitoryManagement.sln
dotnet test ..\tests\DormitoryManagement.Application.Tests\DormitoryManagement.Application.Tests.csproj
dotnet test ..\tests\DormitoryManagement.Infrastructure.Tests\DormitoryManagement.Infrastructure.Tests.csproj
dotnet run --project DormitoryManagement.WPF
dotnet ef database update --project DormitoryManagement.Infrastructure --startup-project DormitoryManagement.WPF
```

## Task-Specific Context Packs

- Application behavior change:
  - Load target service/interface/DTO, matching Application test, one sibling service, relevant security/use-case docs.
- WPF UI change:
  - Load target ViewModel + XAML, one sibling view in same feature, relevant resources, service interface it consumes.
- EF/data change:
  - Load entity, configuration, `DormitoryDbContext`, repository, migration snapshot, database schema docs, Infrastructure tests.
- Auth/security change:
  - Load `docs/Security.md`, `AuthService`, `PermissionService`, affected service, matching tests, audit behavior.
- Billing/payment change:
  - Load relevant `Billing`/`Payments` services, DTOs, invoice/payment entities, tests, schema/use-case docs.
- Forum change:
  - Treat forum as supporting module; dormitory operations remain primary.

## Useful Anchors

- Auth service pattern: `src/DormitoryManagement.Application/Services/Auth/AuthService.cs`
- Login ViewModel pattern: `src/DormitoryManagement.WPF/ViewModels/Auth/LoginViewModel.cs`
- Application fake/test style: `tests/DormitoryManagement.Application.Tests/AuthServiceTests.cs`
- DI composition: `src/DormitoryManagement.WPF/Bootstrapper/DependencyInjection.cs`
- EF context: `src/DormitoryManagement.Infrastructure/Data/DormitoryDbContext.cs`
- Seed/demo logins: `src/DormitoryManagement.Infrastructure/Data/SeedData.cs`
