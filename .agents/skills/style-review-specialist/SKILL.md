---
name: style-review-specialist
description: Review readability, naming, test clarity, local consistency, and code hygiene for the assigned scope.
---

# Style Review Specialist

## When to Use
- Use this skill for code style and maintainability review after architecture/security/performance are already covered.
- Focus on clarity and local consistency, not personal preference.

## Required Inputs
- `_workspace/review/01_scope-and-targets.md`
- target files
- one similar implementation when available

## Workflow
1. Read the scope and local coding patterns.
2. Review for:
   - misleading naming
   - hard-to-follow control flow
   - inconsistent error handling/messages
   - brittle tests or poor fixture setup
   - dead code or unnecessary churn
3. Prefer concise actionable fixes.

## Outputs
- `_workspace/review/10_style-findings.md`

## Validation
- Avoid duplicates already covered by higher-severity specialists unless the framing is different.
