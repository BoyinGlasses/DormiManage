# Integration Merge

## Frontend / Backend Agreement Check
- commands vs handlers:
  - `ToggleCreateFormCommand`, `ToggleFiltersCommand`, `SelectTicketCommand`, `SecondaryTicketActionCommand`, and `UpdateStatusCommand` remain unchanged at the binding surface.
- ViewModel vs service DTO shape:
  - no service signature changes; ViewModel now patches `SupportTicketDto` copies locally after successful status updates.
- status / enum mappings:
  - existing `SupportTicketValueConverter` mappings preserved for status/category/reference formatting.
- error handling:
  - existing WPF error/success/loading surfaces preserved.

## Remaining Fixes
- none blocking after the pixel-match pass.

## Merge Decision
- ready for qa: yes
