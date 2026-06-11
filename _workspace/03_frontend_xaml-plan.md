# Frontend XAML Plan

## Files To Change
- `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml`
- `src/DormitoryManagement.WPF/Resources/SupportTickets.xaml`
- `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs`

## UI Contract Implemented
- Keep the responsive container and virtualized table fixes intact.
- Tighten route-body density toward the HTML ref.
- Uppercase summary labels and table headers.
- Switch list-card header/footer chrome to white/surface.
- Neutralize default row action tone and reduce affordance size/weight.

## Tests Added / Updated
- resource contract updated for pixel-match token values
- narrow-host runtime render check
- long-content/alignment table contract check
- pixel-match casing/chrome contract check

## Boundary Risks
- `DataGrid` must stay visually restrained enough to avoid reintroducing browser-vs-WPF chrome drift.
