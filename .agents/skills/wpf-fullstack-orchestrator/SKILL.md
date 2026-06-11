---
name: wpf-fullstack-orchestrator
description: Coordinate wireframe-to-deployment delivery for the DormitoryManagement WPF stack across design, XAML, backend, and QA.
---

# WPF Fullstack Orchestrator

## When to Use
- Use this skill when a request spans multiple WPF delivery surfaces: wireframe/design, XAML UI, application/infrastructure behavior, and QA/release verification.
- Use it for feature work that needs deterministic handoffs instead of ad hoc implementation.
- Do not use it for a single-file fix that stays entirely in one layer.

## Required Inputs
- User request or feature brief.
- Relevant repo docs: `AGENTS.md`, `.ai/*` when active, `docs/Architecture.md`, `docs/DevelopmentGuide.md`, `docs/Security.md`, `docs/UseCases.md` as needed.
- Visual source when design fidelity matters: wireframe, Stitch HTML/PNG, screenshot, or explicit copy/layout notes.
- Success criteria: user-visible behavior, authorization rules, and verification bar.

## Workflow
1. Frame the job in `_workspace/00_input/request-summary.md`:
   - scope
   - target roles
   - explicit non-goals
   - acceptance checks
2. Create the design brief in `_workspace/01_design_wireframe-brief.md` using `.agents/skills/wpf-design-analyst/SKILL.md`.
3. Create the shared implementation contract in `_workspace/02_contract_feature-slice.md`:
   - screen states and copy
   - ViewModel/service boundaries
   - DTO/request/command changes if any
   - auth/audit constraints
   - tests required
4. Fan out bounded implementation from the same contract snapshot:
   - XAML/UI role writes `_workspace/03_frontend_xaml-plan.md`
   - backend role writes `_workspace/03_backend_service-plan.md`
5. Merge into `_workspace/04_integration_merge.md`:
   - changed files by layer
   - dependency order
   - unresolved boundary risks
6. Run QA/release verification via `.agents/skills/wpf-qa-release/SKILL.md` and write `_workspace/05_qa_release-report.md`.
7. Emit final delivery summary in `_workspace/final/deployment-ready-summary.md`.

## Outputs
- `docs/harness/wpf-fullstack/team-spec.md`
- `_workspace/00_input/request-summary.md`
- `_workspace/01_design_wireframe-brief.md`
- `_workspace/02_contract_feature-slice.md`
- `_workspace/03_frontend_xaml-plan.md`
- `_workspace/03_backend_service-plan.md`
- `_workspace/04_integration_merge.md`
- `_workspace/05_qa_release-report.md`
- `_workspace/final/deployment-ready-summary.md`

## Phase Rules
- Keep the outer pattern as `Pipeline`.
- Use bounded `Fan-out/Fan-in` only inside implementation after the shared contract is written.
- Frontend and backend roles must read the same contract before editing code.
- QA reads the original request, the contract, and the actual diffs; never only one side.

## Failure Policy
- Missing wireframe/reference: design role records assumptions in `_workspace/01_design_wireframe-brief.md`; no silent guessing.
- Design/backend conflict: stop fan-out, update `_workspace/02_contract_feature-slice.md`, then resume.
- Build/test failure: QA reports the exact failing command and owning layer in `_workspace/05_qa_release-report.md`.
- Authorization or audit uncertainty: escalate before merge; do not resolve with UI-only gates.

## Validation
- Every phase writes a named artifact.
- XAML work stays in WPF layer; backend work stays in Application/Infrastructure boundaries from `docs/Architecture.md`.
- Verification must include build plus focused tests, then broader regression tests when shared surfaces changed.
- Release summary must state deployability, known deltas, and rollback surface.

## References
- `docs/harness/wpf-fullstack/team-spec.md`
- `.agents/skills/wpf-design-analyst/SKILL.md`
- `.agents/skills/wpf-xaml-builder/SKILL.md`
- `.agents/skills/wpf-backend-builder/SKILL.md`
- `.agents/skills/wpf-qa-release/SKILL.md`
