# Scope And Targets

## Scope

- WPF Support Tickets route-body fidelity only.
- Strong focus: recent-list card, header/body alignment, spacing, badges, actions, footer, summary cards, title/CTA density.
- Review includes artifact-generation/test harness quality because sign-off depends on deterministic evidence.

## Targets

- `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml`
- `src/DormitoryManagement.WPF/Resources/SupportTickets.xaml`
- `tests/DormitoryManagement.WPF.Tests/SupportTicketViewTests.cs`
- `.ai/artifacts/support-tickets-wpf-recovered.png`
- `stitch-downloads/Dorm/f800aa1e608c47bba0667fef296a6832/Quan-ly-yeu-cau-ho-tro-DormManagement.html`
- `stitch-downloads/Dorm/f800aa1e608c47bba0667fef296a6832/Quan-ly-yeu-cau-ho-tro-DormManagement.png`

## Severity Rubric

- `P1`: blocks rigorous fidelity sign-off or hides a real regression.
- `P2`: visible mismatch or QA weakness that should be fixed before calling the screen pixel-perfect.
- `P3`: maintainability/polish issue; follow-up okay.

## Excluded Surfaces

- Backend ticket workflow redesign.
- Non-ticket routes.
- App shell redesign outside the visible route-body relationship to the ticket screen.

## Evidence Commands

```powershell
dotnet test tests\DormitoryManagement.WPF.Tests\DormitoryManagement.WPF.Tests.csproj --filter "FullyQualifiedName~SupportTicket"
dotnet build src\DormitoryManagement.sln
```
