# Pixel-Match Recovery Plan - Support Tickets

Branch: `015-ticket-screen-fidelity`
Active spec: `specs/014-ticket-screen-fidelity/spec.md`
Base plan: `specs/014-ticket-screen-fidelity/plan.md`
Input QA: `.ai/artifacts/support-ticket-strong-qa-report.md`

## Objective

Reach a defensible strict pixel-match sign-off for the Support Tickets route, with recent-list parity as priority #1.

## Technical Context

- UI stack: WPF MVVM
- Primary files:
  - `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml`
  - `src/DormitoryManagement.WPF/Resources/SupportTickets.xaml`
  - `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs`
- Canonical ref: local Stitch HTML + PNG
- Verification path:
  - `dotnet test tests\DormitoryManagement.WPF.Tests\DormitoryManagement.WPF.Tests.csproj --filter "FullyQualifiedName~SupportTicket"`
  - `dotnet build src\DormitoryManagement.sln`

## Constitution Check

- Layered MVVM boundaries: PASS, planned work stays in WPF + WPF tests
- Security/auth/audit: PASS, no service-boundary redesign planned
- Test-first delivery: PASS, fixture/capture tests lead before XAML tuning
- Product scope discipline: PASS, route-body fidelity only

## Root-Cause Summary From QA

1. Evidence gate is weak:
   - screenshot fixture data differs from ref sample
   - capture viewport differs from ref PNG
2. Recent-list still misses literal parity:
   - typography too heavy
   - badge/action density still off
   - footer and row rhythm still slightly heavier than ref
3. Minor UX debt remains:
   - delete affordance weaker than ref
   - some secondary-state copy still English
   - dead DataGrid styling still in file

## Strategy

Do not start with visual nibbling.
First make the evidence deterministic, then tighten the actual pixels, then re-run QA.

## Phase 0 - Lock The Measurement Harness

### Goal
Make the screenshot comparison trustworthy before further UI tuning.

### Tasks
1. Add a canonical screenshot-fixture dataset in `SupportTicketViewTests.cs` matching the HTML sample:
   - counts `12 / 3 / 9`
   - visible 4 rows matching sample order and labels
   - sample categories aligned with ref text
2. Add a dedicated capture path for ref-sized viewport/aspect.
3. Generate dedicated artifacts for:
   - full screen
   - recent-list card
   - recent-list rows
4. Tighten tests so artifact generation fails if the canonical fixture or viewport drifts.

### Exit Criteria
- Canonical artifact content matches the ref sample content contract.
- Ref-sized artifact exists.
- SupportTicket tests stay green.

## Phase 1 - Recent-List Pixel Match

### Goal
Bring `Danh sách yêu cầu gần đây` to strict parity first.

### Tasks
1. Re-measure list geometry against the ref:
   - card bounds
   - header height
   - row height
   - footer height
   - cell insets
   - column width ratios
2. Reduce type density where needed:
   - header label weight/size
   - row text size/line-height
   - badge label density
3. Tighten badge parity:
   - min height
   - horizontal padding
   - color contrast
   - corner radius
4. Tighten action parity:
   - view/delete icon size
   - icon spacing
   - destructive hover treatment for delete
5. Tighten footer parity:
   - summary text baseline
   - pager button size, border, padding, disabled state
6. Remove dead DataGrid-only style surface if unused after shared-column table validation.

### Exit Criteria
- list diff is visually narrow enough that only unavoidable font/icon rendering remains
- no header/body drift
- no horizontal overflow on narrow host
- destructive affordance parity present

## Phase 2 - First-Viewport Density Match

### Goal
Match title/CTA/summary-card density after recent-list is stable.

### Tasks
1. Tune title weight/line-height and subtitle spacing.
2. Tune CTA height, padding, icon gap, and radius.
3. Tune summary-card:
   - min height
   - internal left padding
   - icon well fill and size
   - label/value weight and spacing
4. Re-check vertical rhythm between title, cards, and list card.

### Exit Criteria
- first viewport reads as the same composition as ref, not just the same sections
- no obvious heavy-type / oversized-card feel remains

## Phase 3 - Secondary-State UX Cleanup

### Goal
Remove polish debt that hurts confidence after pixel work is done.

### Tasks
1. Localize remaining English copy in empty/error/create/staff states.
2. Re-check create/open/empty/error views for spacing consistency with route tokens.
3. Keep these changes scoped; no flow redesign.

### Exit Criteria
- secondary states no longer break Vietnamese UX consistency

## Phase 4 - Final QA Gate

### Goal
Re-run strict evidence path and decide pass/fix.

### Tasks
1. Run focused SupportTicket WPF tests.
2. Run solution build.
3. Regenerate artifacts.
4. Compare against ref again using:
   - visual inspection
   - normalized diff images
   - list-first sign-off judgment
5. Update final QA report with one of two outcomes only:
   - `PASS`: strict pixel-match defensible
   - `FIX`: exact blockers listed

### Exit Criteria
- strict conclusion upgraded from `FIX` to `PASS`
- if not, report names the remaining blocker with exact surface and delta

## Task Order

1. measurement harness
2. recent-list pixel match
3. first-viewport density match
4. secondary-state cleanup
5. final QA gate

## Recommended First Implementation Slice

Start with Phase 0 + the minimum list-only deltas from Phase 1. Do not touch title/summary cards until the list artifact is deterministic.

## Risks

- Chasing pixel polish before fixing fixture/viewport will create false positives
- Real WPF-vs-browser font metrics may require settling for explicitly documented platform residuals
- Over-editing route resources can disturb already-correct summary/title surfaces; keep list-first isolation
