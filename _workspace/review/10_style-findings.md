# Style Findings

## P2 - Delete action does not express the destructive affordance defined by the HTML ref

- File: `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:326`
- Impact: both row actions share the same neutral button style. The HTML ref gives delete a destructive hover treatment. Current WPF behavior reduces affordance clarity and weakens ref parity in interactive QA.
- Smallest fix: split the delete button into a dedicated style/trigger path with error hover foreground/background treatment.

## P3 - Legacy DataGrid styles remain in the view after the table moved to a custom shared-column grid

- File: `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:75`
- Impact: the file still carries a large unused DataGrid styling block. It adds false signals during review because the actual ticket list no longer uses `DataGrid`.
- Smallest fix: remove the dead DataGrid style block or move any still-needed shared text styles into route resources with narrower names.

## P3 - Non-default states still mix English copy into a Vietnamese route

- Files: `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:167`, `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:383`, `src/DormitoryManagement.WPF/Views/SupportTickets/SupportTicketListView.xaml:393`
- Impact: first viewport is aligned, but error/empty/create/staff-update paths still expose English strings. This breaks local consistency and hurts UX polish once reviewers leave the default state.
- Smallest fix: localize the remaining support-ticket route strings to the same Vietnamese contract level as the main viewport.
