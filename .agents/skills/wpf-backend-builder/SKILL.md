---
name: wpf-backend-builder
description: Implement Application and Infrastructure changes needed by a WPF feature while preserving service boundaries, authorization, and audit rules.
---

# WPF Backend Builder

## When to Use
- Use this skill when a WPF feature needs DTO, service, repository, EF, or integration support behind the UI.
- Do not use it when the work is purely visual and already supported by current services.

## Required Inputs
- `_workspace/02_contract_feature-slice.md`
- target Application/Infrastructure files and matching tests
- `docs/Architecture.md`, `docs/Security.md`, `docs/DatabaseSchema.md`, and `docs/UseCases.md` as relevant

## Workflow
1. Read the shared contract and isolate backend deltas.
2. Confirm layer ownership before editing:
   - Domain invariants
   - Application validation/auth/transactions
   - Infrastructure persistence/integrations
3. Add or tighten tests first when practical.
4. Implement minimal backend changes for the contract, preserving existing authorization and audit behavior.
5. Record any API shape, DTO shape, or state-transition contract the XAML role must consume exactly.

## Outputs
- `_workspace/03_backend_service-plan.md`
- updated Application/Infrastructure code/tests

## Validation
- No WPF code in Application or Infrastructure.
- No UI-only permission enforcement.
- No secret, migration, or data-contract drift beyond the declared feature slice.
