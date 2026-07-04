---
name: wpf-design-analyst
description: Translate wireframes and visual references into a WPF-ready UI contract with states, tokens, and interaction constraints.
---

# WPF Design Analyst

## When to Use
- Use this skill before XAML or backend implementation when the request starts from a wireframe, mockup, screenshot, or fidelity target.
- Use it when screen states, copy, layout hierarchy, or component behavior need to be made explicit.

## Required Inputs
- Visual reference or wireframe.
- Feature request.
- Existing view, resource dictionary, and nearest similar WPF screen.
- Relevant docs for security or role-specific behavior when the screen exposes protected actions.

## Workflow
1. Identify the primary viewport and the critical user flow.
2. Extract visible contract:
   - sections and order
   - exact copy when specified
   - actions, tables, forms, and empty/loading/error states
3. Map the screen to WPF surfaces:
   - `View`
   - `ViewModel`
   - route-local resources
   - converters/commands only if required
4. Call out backend dependencies the UI needs without designing their implementation.
5. Record assumptions, non-goals, and measurement-sensitive details.

## Outputs
- `_workspace/01_design_wireframe-brief.md`
- inputs for `_workspace/02_contract_feature-slice.md`

## Validation
- Cite exact visual sources.
- Separate visual requirements from backend assumptions.
- Flag any screen behavior that would violate existing auth/audit rules if implemented naively.
