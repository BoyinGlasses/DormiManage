---
name: performance-review-specialist
description: Review the assigned scope for avoidable latency, excess allocation, repeated database/UI work, and scaling bottlenecks.
---

# Performance Review Specialist

## When to Use
- Use this skill for performance and scaling review.
- Focus on bottlenecks that are plausible in the target scope, not speculative micro-optimizations.

## Required Inputs
- `_workspace/review/01_scope-and-targets.md`
- target files
- runtime/build/test context when available

## Workflow
1. Read the scope and identify likely hot paths.
2. Review for:
   - N+1 or repeated query patterns
   - expensive UI rerender/binding churn
   - unnecessary repeated allocations or conversions
   - blocking work on UI thread
   - avoidable full-scan or repeated enumeration paths
3. Estimate impact and trigger conditions.

## Outputs
- `_workspace/review/10_performance-findings.md`

## Validation
- State why the path is plausibly hot.
- Distinguish measured evidence from code-path inference.
