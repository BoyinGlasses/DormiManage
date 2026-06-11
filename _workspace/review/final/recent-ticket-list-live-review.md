## Scoped Review: Recent Ticket List

Compared surfaces:
- live WPF render artifact: `.ai/artifacts/support-tickets-wpf-recovered.png`
- HTML ref: `stitch-downloads/Dorm/f800aa1e608c47bba0667fef296a6832/Quan-ly-yeu-cau-ho-tro-DormManagement.html`
- PNG ref: `stitch-downloads/Dorm/f800aa1e608c47bba0667fef296a6832/Quan-ly-yeu-cau-ho-tro-DormManagement.png`

## Findings Before Fix

1. Recent-list body cells were visibly offset from the table labels, especially under constrained width.
2. The prior layout surface was not holding the same column geometry across header and rows, so pixel match degraded sharply in the live app.
3. Long content could amplify the drift and make the table feel looser than the ref.

## Fix Applied

- Kept the 4-row paging contract already added in `SupportTicketListViewModel`.
- Rebuilt the recent-list surface as a shared-column table in `SupportTicketListView.xaml` using:
  - `SupportTicketsTableHeaderGrid`
  - `SupportTicketsTableRows`
  - consistent `SharedSizeGroup` values across all 6 columns
  - shared header/body padding tokens for ref-like insets
- Updated focused WPF regression tests to validate narrow-host behavior, long-content trimming, and header/body column alignment against the live control tree.

## Result After Fix

- Live WPF artifact now shows header labels and body content aligned on the same column tracks.
- The recent list remains four visible rows tall with correct footer paging state.
- No obvious clipping or header/body drift is visible in the refreshed artifact.
- The table now reads much closer to the HTML ref than the earlier reported ~50% state.

## Residual Deltas

- WPF typography is still slightly heavier than browser rendering.
- Material icon glyphs still differ slightly from browser symbols.
- The capture remains a desktop WPF review image, not the exact Stitch export size.