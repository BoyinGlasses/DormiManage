# Code Review Team Spec

## Goal
Provide a reusable harness for comprehensive code review using parallel specialist agents for architecture, security, performance, and style, then merge all findings into a single report.

## Architecture Pattern
Outer pattern: `Fan-out/Fan-in`.

Reason: all four review lenses can inspect the same frozen scope independently, then a synthesizer can merge and rank the results without serializing the whole review.

## Roles
| Role | Responsibility | Skill | Writes |
| --- | --- | --- | --- |
| Review Orchestrator | freeze scope, launch specialists, manage merge | `.agents/skills/code-review-orchestrator/SKILL.md` | `_workspace/review/00_request-summary.md`, `_workspace/review/01_scope-and-targets.md` |
| Architecture Reviewer | layering, dependency, ownership, design coherence | `.agents/skills/architecture-review-specialist/SKILL.md` | `_workspace/review/10_architecture-findings.md` |
| Security Reviewer | auth/authz, secrets, trust boundaries, audit exposure | `.agents/skills/security-review-specialist/SKILL.md` | `_workspace/review/10_security-findings.md` |
| Performance Reviewer | hot paths, query churn, UI thread blocking, scaling risks | `.agents/skills/performance-review-specialist/SKILL.md` | `_workspace/review/10_performance-findings.md` |
| Style Reviewer | readability, naming, local consistency, test hygiene | `.agents/skills/style-review-specialist/SKILL.md` | `_workspace/review/10_style-findings.md` |
| Findings Synthesizer | deduplicate and merge into one review report | `.agents/skills/review-findings-synthesizer/SKILL.md` | `_workspace/review/20_merged-findings.md`, `_workspace/review/final/code-review-report.md` |

## Phase Order

### Phase 0: Intake
- capture target scope in `_workspace/review/00_request-summary.md`
- record whether review target is a diff, feature, directory, or whole repo

### Phase 1: Scope Freeze
- create `_workspace/review/01_scope-and-targets.md`
- list exact files/modules, commands, exclusions, and severity rubric
- all reviewers use this artifact unchanged

### Phase 2: Parallel Specialist Review
- architecture reviewer writes `_workspace/review/10_architecture-findings.md`
- security reviewer writes `_workspace/review/10_security-findings.md`
- performance reviewer writes `_workspace/review/10_performance-findings.md`
- style reviewer writes `_workspace/review/10_style-findings.md`

### Phase 3: Merge
- synthesizer merges duplicates and conflicting framings into `_workspace/review/20_merged-findings.md`

### Phase 4: Final Report
- synthesizer writes `_workspace/review/final/code-review-report.md`
- findings ordered by severity, then open questions, then residual gaps

## Handoff Files
| From | To | File | Purpose |
| --- | --- | --- | --- |
| Orchestrator | All reviewers | `_workspace/review/01_scope-and-targets.md` | single frozen review scope |
| All reviewers | Synthesizer | `_workspace/review/10_*` | raw findings per lens |
| Synthesizer | User | `_workspace/review/final/code-review-report.md` | single merged report |

## Severity Rubric
- `critical`: exploitable security issue, data loss, authorization bypass, or severe correctness risk
- `high`: likely regression, strong architectural break, serious performance bottleneck on plausible hot path
- `medium`: maintainability or performance issue with meaningful impact but not immediately dangerous
- `low`: clarity, consistency, or hygiene improvement

## Review Rules
- Specialists lead with findings, not summaries.
- File references required.
- Avoid duplicate low-value findings across lenses.
- If no issues are found in one lens, say that explicitly.
- Mark speculative concerns as `unverified` or `needs confirmation`.

## Failure Policy
- If target scope is too large, narrow by diff, module, or feature before fan-out.
- If one specialist stalls, synthesizer still merges completed lenses and states the missing coverage.
- If findings conflict, synthesizer preserves the stricter defensible interpretation and notes disagreement.

## Normal Flow
1. User requests a broad code review.
2. Orchestrator freezes scope.
3. Four specialists review in parallel.
4. Synthesizer merges and ranks findings.
5. Final report delivered as one artifact.

## Failure Flow
1. Security flags an auth bypass.
2. Style flags only naming issues in same file.
3. Synthesizer keeps the security finding first and folds any style note into a lower-severity follow-up instead of repeating context.
