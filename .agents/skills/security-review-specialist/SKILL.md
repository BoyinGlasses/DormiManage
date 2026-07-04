---
name: security-review-specialist
description: Review the assigned scope for authorization gaps, secret handling mistakes, unsafe inputs, and sensitive-data exposure.
---

# Security Review Specialist

## When to Use
- Use this skill for security-focused review.
- Focus on real exploit or policy risk, not generic best-practice noise.

## Required Inputs
- `_workspace/review/01_scope-and-targets.md`
- target files
- `docs/Security.md`

## Workflow
1. Read the scope and repo security boundaries.
2. Review for:
   - auth/authz bypass risk
   - unsafe trust in UI visibility
   - secret or credential leakage
   - unsafe deserialization/input handling
   - sensitive diagnostics/logging exposure
   - audit gaps where the repo expects auditability
3. Separate confirmed issues from hypothetical concerns.

## Outputs
- `_workspace/review/10_security-findings.md`

## Validation
- Lead with exploitability and impact.
- Mark non-verifiable concerns as `needs confirmation`.
