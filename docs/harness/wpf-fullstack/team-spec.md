# WPF Fullstack Team Spec

## Goal
Provide a reusable harness for DormitoryManagement feature delivery from wireframe to deployment across design, XAML frontend, backend services, and QA/release validation.

The harness is optimized for this repository:
- WPF desktop app, not ASP.NET Core API
- Layered MVVM
- Application Services enforce authorization
- EF Core + SQL Server backend
- xUnit-based validation

## Domain Summary
- Design source may be a wireframe, Stitch artifact, screenshot, or explicit UX brief.
- Frontend output is WPF XAML, route-local resources, bindings, commands, and presentation ViewModel state.
- Backend output is Application/Infrastructure behavior that the screen or workflow consumes.
- QA verifies boundary coherence, not only isolated correctness.
- Deployment means the feature is buildable, tested, and locally runnable with documented config/migration steps.

## Architecture Pattern
Outer pattern: `Pipeline`.

Inner pattern during implementation: bounded `Fan-out/Fan-in` after a shared contract exists.

Reason: design must stabilize the feature slice before frontend/backend work can proceed independently. Once that contract is written, XAML and backend can move in parallel. QA then performs a single synthesis and release gate.

## Roles
| Role | Responsibility | Skill | Writes |
| --- | --- | --- | --- |
| Orchestrator | owns scope, phase order, merge, and release decision | `.agents/skills/wpf-fullstack-orchestrator/SKILL.md` | `_workspace/00_input/request-summary.md`, `_workspace/04_integration_merge.md` |
| Design Analyst | turns wireframe/reference into a WPF-ready contract | `.agents/skills/wpf-design-analyst/SKILL.md` | `_workspace/01_design_wireframe-brief.md` |
| XAML Builder | owns WPF views/resources/presentation state and related tests | `.agents/skills/wpf-xaml-builder/SKILL.md` | `_workspace/03_frontend_xaml-plan.md` |
| Backend Builder | owns Application/Infrastructure changes and related tests | `.agents/skills/wpf-backend-builder/SKILL.md` | `_workspace/03_backend_service-plan.md` |
| QA Release | owns cross-boundary checks, command verification, runtime smoke, deployability gate | `.agents/skills/wpf-qa-release/SKILL.md` | `_workspace/05_qa_release-report.md`, `_workspace/final/deployment-ready-summary.md` |

## Phase Order

### Phase 0: Intake
Inputs:
- user request
- `AGENTS.md`
- active `.ai/*` feature state when the repo is already executing a planned feature

Actions:
- isolate the feature slice
- list non-goals
- identify whether design reference exists

Outputs:
- `_workspace/00_input/request-summary.md`

Completion criteria:
- feature scope, constraints, and success checks are explicit

### Phase 1: Wireframe to Contract
Inputs:
- request summary
- visual reference
- target views/resources/tests

Actions:
- extract screen hierarchy, states, copy, controls, and backend needs
- write shared contract for both implementation branches

Outputs:
- `_workspace/01_design_wireframe-brief.md`
- `_workspace/02_contract_feature-slice.md`

Completion criteria:
- frontend/backend can work from one artifact without inventing behavior

### Phase 2: Parallel Implementation
Inputs:
- shared contract

Actions:
- XAML role implements view/resource/binding/test work
- backend role implements service/data/test work

Outputs:
- `_workspace/03_frontend_xaml-plan.md`
- `_workspace/03_backend_service-plan.md`

Completion criteria:
- both branches cite exact changed files and explicit boundary assumptions

### Phase 3: Integration Merge
Inputs:
- both implementation branch artifacts
- working tree diff

Actions:
- reconcile naming, state shape, commands, DTOs, and validation flow
- list dependency order for any remaining fixes

Outputs:
- `_workspace/04_integration_merge.md`

Completion criteria:
- no known unresolved boundary mismatch remains unreported

### Phase 4: QA and Release Gate
Inputs:
- original request
- shared contract
- implementation artifacts
- changed files

Actions:
- run targeted tests, build, broader tests, manual smoke where needed
- verify config, migration, runtime, and rollback notes

Outputs:
- `_workspace/05_qa_release-report.md`
- `_workspace/final/deployment-ready-summary.md`

Completion criteria:
- result labeled `pass`, `fix`, or `redo`
- deployment prerequisites documented

## Handoff Files
| From | To | File | Purpose |
| --- | --- | --- | --- |
| Orchestrator | Design Analyst | `_workspace/00_input/request-summary.md` | bounded scope and acceptance bar |
| Design Analyst | XAML Builder | `_workspace/02_contract_feature-slice.md` | UI contract and state map |
| Design Analyst | Backend Builder | `_workspace/02_contract_feature-slice.md` | service/data expectations |
| XAML Builder | Orchestrator | `_workspace/03_frontend_xaml-plan.md` | changed views/resources/tests |
| Backend Builder | Orchestrator | `_workspace/03_backend_service-plan.md` | changed services/data/tests |
| Orchestrator | QA Release | `_workspace/04_integration_merge.md` | final boundary checklist |

## Artifact Naming Convention
- `_workspace/00_input/request-summary.md`
- `_workspace/01_design_wireframe-brief.md`
- `_workspace/02_contract_feature-slice.md`
- `_workspace/03_frontend_xaml-plan.md`
- `_workspace/03_backend_service-plan.md`
- `_workspace/04_integration_merge.md`
- `_workspace/05_qa_release-report.md`
- `_workspace/final/deployment-ready-summary.md`

## Repo-Specific Guardrails
- Do not add REST controllers, JWT flows, or web API assumptions.
- ViewModels never call `DormitoryDbContext` directly.
- Application never references Infrastructure.
- UI visibility is not authorization.
- Keep secrets in environment variables or local config, never committed files.
- Prefer focused WPF/Application/Infrastructure tests over vague end-state claims.

## Validation Commands
From `src/`:

```powershell
dotnet restore DormitoryManagement.sln
dotnet build DormitoryManagement.sln
dotnet test ..\\tests\\DormitoryManagement.Application.Tests\\DormitoryManagement.Application.Tests.csproj
dotnet test ..\\tests\\DormitoryManagement.Infrastructure.Tests\\DormitoryManagement.Infrastructure.Tests.csproj
dotnet test ..\\tests\\DormitoryManagement.WPF.Tests\\DormitoryManagement.WPF.Tests.csproj
dotnet run --project DormitoryManagement.WPF
```

Add feature-focused filters before the broad suite when the changed surface is small.

## Failure Policy
- `Design blocked`: record missing reference or contradictory acceptance criteria in `_workspace/01_design_wireframe-brief.md`.
- `Frontend blocked`: record exact binding/resource/state gap in `_workspace/03_frontend_xaml-plan.md`.
- `Backend blocked`: record exact service/data/auth gap in `_workspace/03_backend_service-plan.md`.
- `QA blocked`: report failing command, affected files, and smallest plausible owner/fix path in `_workspace/05_qa_release-report.md`.
- If auth/audit behavior is uncertain, stop and escalate rather than inferring from UI.

## Normal Flow
1. User gives wireframe + requested behavior.
2. Design role creates brief and shared contract.
3. Frontend/backend implement from same contract snapshot.
4. Orchestrator merges and resolves naming/state drift.
5. QA runs tests, smoke path, deployment checks.
6. Final summary marks feature ready or lists blockers.

## Failure Flow
1. Design contract says button triggers `CreateTicketCommand`.
2. XAML branch binds `SubmitTicketCommand` instead.
3. QA reads both sides, catches mismatch, marks `fix`.
4. Orchestrator routes fix to owning branch, updates merge notes, reruns QA.
