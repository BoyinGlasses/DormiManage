---
name: wpf-qa-release
description: Verify cross-layer coherence, WPF runtime quality, and deployment readiness for a full-stack WPF feature.
---

# WPF QA Release

## When to Use
- Use this skill after implementation or at a release gate for a WPF feature that spans UI and backend.
- Use it when cross-boundary mismatches are a bigger risk than isolated syntax errors.

## Required Inputs
- original request
- `_workspace/02_contract_feature-slice.md`
- `_workspace/03_frontend_xaml-plan.md`
- `_workspace/03_backend_service-plan.md`
- actual changed files and test outputs
- relevant build/run/test commands from `docs/DevelopmentGuide.md`

## Workflow
1. Compare both sides of each boundary:
   - View/XAML bindings vs ViewModel properties/commands
   - ViewModel requests vs Application service contract
   - Application DTO/state shape vs Infrastructure persistence behavior
2. Run validation in widening order:
   - targeted tests
   - solution build
   - broader affected test suites
   - manual runtime smoke path when the feature is UI-heavy
3. Review deployment readiness:
   - config prerequisites
   - migration steps if any
   - rollback surface
   - known residual risk
4. Classify result as `pass`, `fix`, or `redo`.

## Outputs
- `_workspace/05_qa_release-report.md`
- `_workspace/final/deployment-ready-summary.md`

## Validation
- Report exact commands and results.
- Separate blocking defects from follow-up polish.
- Call out boundary mismatches explicitly, not just missing files.
