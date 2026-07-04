## Findings

### High

1. The Support Tickets route now behaves responsively, but it still misses the HTML ref's density and casing contract in several first-viewport elements. Summary labels and table headers render in title case in `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml`, while the HTML ref uses compact uppercase labels. This is a visible pixel-match miss across the first viewport.

2. The ticket-list chrome is still visually heavier than the ref. The section header and footer containers use the route canvas tone instead of the white/surface tone used by the HTML ref, which makes the list card read flatter and bluer than the reference.

### Medium

3. Action and status affordances still read oversized compared with the HTML ref. `SupportTicketsActionButtonSize`, badge sizing, and the summary-icon circle sizing in `src/DormitoryManagement.WPF/Resources/SupportTickets.xaml` do not yet line up with the reference density, so the WPF route looks roomier than the browser source.

4. The delete action defaults to a strong red icon in WPF, while the ref keeps both actions neutral until hover. This is a persistent visual mismatch, not a functional bug.

## QA bar

- Apply pixel-match rule to route-body composition, density, casing, and control chrome.
- Keep the responsiveness fixes intact.
- Do not expand scope into shell, paging, or backend redesign.
