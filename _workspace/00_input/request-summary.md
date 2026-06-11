# Request Summary

## Feature
- Support Tickets pixel-match remediation on top of the responsiveness fixes.

## User Outcome
- Support Tickets stays readable on narrower desktop widths.
- Support Tickets route body also matches the HTML ref more closely in density, casing, chrome, and affordance sizing.

## Visual Source
- `stitch-downloads/Dorm/f800aa1e608c47bba0667fef296a6832/Quan-ly-yeu-cau-ho-tro-DormManagement.html`
- `stitch-downloads/Dorm/f800aa1e608c47bba0667fef296a6832/Quan-ly-yeu-cau-ho-tro-DormManagement.png`
- `_workspace/review/final/pixel-match-review-report.md`
- existing Support Tickets WPF screen/resources/tests

## Non-Goals
- No Application/Infrastructure/schema/auth redesign.
- No REST/JWT/web additions.
- No pagination feature buildout beyond fixing misleading button state.

## Acceptance Checks
- Root content no longer clips on narrower desktop hosts.
- Ticket table/header/body stay aligned with long content and constrained widths.
- Ticket list keeps the HTML ref's route-body visual hierarchy more closely: compact chrome, white list-card header/footer, uppercase summary/header labels, neutral default row actions, and tighter affordance sizing.
- Focused WPF tests cover narrow layout, overflow/clipping, alignment, and pixel-match contract deltas.

## Roles Needed
- design
- xaml
- qa
