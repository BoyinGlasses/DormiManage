# Support Ticket Pixel-Match Contract

## Success Bar

The work is only `PASS` when all of the following hold:

1. review artifact uses canonical sample data matching the ref layout sample
2. review artifact is captured at normalized ref viewport/aspect
3. recent-list card is visually aligned with ref in structure, density, badges, actions, and footer
4. focused SupportTicket tests pass
5. solution build passes
6. remaining differences are limited to unavoidable platform font/icon rendering and are explicitly documented

## Severity Rules

- `Blocker`: any issue that prevents claiming strict pixel match
- `Major`: visible mismatch that weakens parity but can be fixed in the same pass
- `Minor`: polish/localization/dead-code item that does not block sign-off

## Required Code Surfaces

- `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs`
- `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml`
- `src/DormitoryManagement.WPF/Resources/SupportTickets.xaml`
- `src/DormitoryManagement.WPF/Converters/SupportTicketValueConverter.cs` only if label or category text must be normalized to ref sample

## Evidence Artifacts Required

- canonicalized WPF runtime artifact at ref-sized viewport
- diff artifact for full screen
- diff artifact for recent-list card
- final QA note with strict pass/fix judgment

## Guardrails

- no Application/Infrastructure contract changes unless a real blocker proves impossible otherwise
- no auth/audit regressions
- no unrelated refactors riding along with pixel work
