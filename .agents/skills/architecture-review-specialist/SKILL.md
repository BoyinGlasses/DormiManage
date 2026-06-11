---
name: architecture-review-specialist
description: Review layering, dependency direction, boundary ownership, and design coherence for the assigned scope.
---

# Architecture Review Specialist

## When to Use
- Use this skill for architecture and maintainability review.
- Focus on layering, dependency rules, ownership drift, duplication, and incoherent abstraction.

## Required Inputs
- `_workspace/review/01_scope-and-targets.md`
- target files
- `docs/Architecture.md` and relevant feature docs

## Workflow
1. Read the scope and dependency rules.
2. Review the target for:
   - forbidden cross-layer references
   - leakage of infra concerns upward
   - UI/business/data responsibility mixing
   - needless abstraction or duplication
   - missing tests around shared behavior
3. Write findings with severity, impact, and smallest plausible fix path.

## Outputs
- `_workspace/review/10_architecture-findings.md`

## Validation
- Cite exact files.
- Prefer behavior/regression risk over style commentary.
