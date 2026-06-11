# Code Review Report

## Verdict

- Code review status: `fix before calling it pixel-perfect`
- Product UI status: `near-pass`
- Evidence/sign-off status: `not strict-pass yet`

## Findings

### P1 - Screenshot evidence is not deterministic against the canonical ticket sample

- Files: `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs:157`, `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs:518`
- Why it matters: the generated WPF artifact is used as fidelity evidence, but its seed data does not mirror the HTML sample rows or summary counts. That weakens any claim of precise visual parity because reviewers are comparing changed UI plus changed content at the same time.

### P2 - Pixel gate is not normalized to the canonical viewport

- Files: `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs:161`, `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs:168`
- Why it matters: the artifact capture size is fixed at `1440x860`, while the canonical reference PNG is `512x377`. The comparison is still useful, but it is a proportional QA read, not a literal pixel-for-pixel gate.

### P2 - Delete action styling still under-signals destructive intent

- File: `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:326`
- Why it matters: the HTML ref gives the delete action a distinct destructive hover treatment. The current WPF row actions look effectively equivalent until hover-state inspection, so interactive parity is weaker than the static screenshot suggests.

### P3 - Unused DataGrid style block is still in the view file

- File: `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:75`
- Why it matters: the real recent-list surface is the shared-column `ItemsControl` table, not `DataGrid`. Keeping the old block raises review noise and future maintenance cost.

### P3 - Secondary ticket states still mix English UI copy into a Vietnamese route

- Files: `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:167`, `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:383`, `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:393`
- Why it matters: not a first-viewport blocker, but it does lower overall UX consistency once the user opens error/create/empty states.

## Open Questions

- None for security or backend boundaries.

## Residual Gaps With No Confirmed Bug

- WPF typography still reads heavier than the browser ref, especially title/card density.
- MahApps glyphs differ slightly from Material Symbols in the HTML export.
- Dynamic runtime data is allowed by the feature spec, but it still reduces literal visual comparability unless the screenshot fixture is frozen to the ref sample.
