---
name: review-findings-synthesizer
description: Merge parallel review findings into one deduplicated report ordered by severity and grounded in file references.
---

# Review Findings Synthesizer

## When to Use
- Use this skill after the parallel review specialists have written their artifacts.
- Use it when the final output must be one coherent code review report.

## Required Inputs
- `_workspace/review/01_scope-and-targets.md`
- `_workspace/review/10_architecture-findings.md`
- `_workspace/review/10_security-findings.md`
- `_workspace/review/10_performance-findings.md`
- `_workspace/review/10_style-findings.md`

## Workflow
1. Read all specialist artifacts.
2. Merge overlapping findings.
3. Normalize severity with this bias:
   - security exploit/regression risk first
   - architecture/data correctness next
   - performance bottlenecks next
   - style/maintainability last
4. Write:
   - `_workspace/review/20_merged-findings.md` for raw merged inventory
   - `_workspace/review/final/code-review-report.md` for the user-facing report
5. Preserve open questions and explicit review gaps.

## Outputs
- `_workspace/review/20_merged-findings.md`
- `_workspace/review/final/code-review-report.md`

## Validation
- Final report must be deduplicated.
- Every finding must cite source files.
- State clearly when no issues are found in a given lens.
