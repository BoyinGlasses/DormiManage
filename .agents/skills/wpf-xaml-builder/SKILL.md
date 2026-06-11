---
name: wpf-xaml-builder
description: Implement or refine WPF views, resources, and ViewModels from a shared UI contract without breaking MVVM boundaries.
---

# WPF XAML Builder

## When to Use
- Use this skill for WPF view delivery: XAML layout, resource dictionaries, bindings, commands, converters, and ViewModel presentation state.
- Do not use it for repository/schema work or service-policy design.

## Required Inputs
- `_workspace/02_contract_feature-slice.md`
- target WPF files and matching tests
- one similar screen in the same layer
- `docs/Architecture.md` and `docs/DevelopmentGuide.md` when boundaries or validation commands matter

## Workflow
1. Read the contract and list the WPF files to touch.
2. Add or tighten UI-facing tests first when practical.
3. Implement the smallest WPF change set that satisfies the contract:
   - `Views/*`
   - `Resources/*`
   - `ViewModels/*` presentation state only when needed
4. Keep commands/bindings deterministic and avoid direct data access from the UI.
5. Record changed files, resource keys, and boundary risks.

## Outputs
- `_workspace/03_frontend_xaml-plan.md`
- updated WPF code/tests

## Validation
- Verify buildable bindings/resources.
- Confirm text fit, layout stability, and role-safe default states.
- Note any backend contract gap instead of compensating with UI-only hacks.
