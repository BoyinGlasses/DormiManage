---
name: code-review-orchestrator
description: Coordinate parallel code review across architecture, security, performance, and style, then merge findings into one report.
---

# Code Review Orchestrator

## When to Use
- Use this skill for comprehensive repo or feature review where multiple quality lenses must run in parallel.
- Use it when the output must be one merged report instead of separate scattered notes.
- Do not use it for single-lens review requests that fit one specialist.

## Required Inputs
- review target: diff, branch, files, module, or feature area
- review scope and non-goals
- acceptance bar for findings: bugs only vs broader maintainability/perf/style issues
- relevant docs: `AGENTS.md`, `docs/Architecture.md`, `docs/Security.md`, `docs/DevelopmentGuide.md`

## Workflow
1. Write intake in `_workspace/review/00_request-summary.md`.
2. Freeze scope in `_workspace/review/01_scope-and-targets.md`:
   - exact files or modules
   - commands allowed
   - severity rubric
   - excluded surfaces
3. Fan out the same scope snapshot to four specialists:
   - architecture -> `_workspace/review/10_architecture-findings.md`
   - security -> `_workspace/review/10_security-findings.md`
   - performance -> `_workspace/review/10_performance-findings.md`
   - style -> `_workspace/review/10_style-findings.md`
4. Hand all four artifacts to the synthesizer.
5. Merge duplicates, normalize severity, and publish:
   - `_workspace/review/20_merged-findings.md`
   - `_workspace/review/final/code-review-report.md`

## Outputs
- `docs/harness/code-review/team-spec.md`
- `_workspace/review/00_request-summary.md`
- `_workspace/review/01_scope-and-targets.md`
- `_workspace/review/10_architecture-findings.md`
- `_workspace/review/10_security-findings.md`
- `_workspace/review/10_performance-findings.md`
- `_workspace/review/10_style-findings.md`
- `_workspace/review/20_merged-findings.md`
- `_workspace/review/final/code-review-report.md`

## Failure Policy
- If scope is fuzzy, narrow it in `_workspace/review/01_scope-and-targets.md` before fan-out.
- If one specialist cannot verify a claim, mark it as `unverified` instead of inflating severity.
- If two specialists disagree, synthesizer records the conflict and picks the stricter defensible interpretation.

## Validation
- Every specialist must read the same scope artifact.
- Findings must include file references.
- Final report must deduplicate overlapping findings and state residual review gaps.

## References
- `docs/harness/code-review/team-spec.md`
- `.agents/skills/architecture-review-specialist/SKILL.md`
- `.agents/skills/security-review-specialist/SKILL.md`
- `.agents/skills/performance-review-specialist/SKILL.md`
- `.agents/skills/style-review-specialist/SKILL.md`
- `.agents/skills/review-findings-synthesizer/SKILL.md`
