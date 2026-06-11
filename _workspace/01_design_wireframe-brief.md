# Support Ticket Pixel-Match Design Brief

## Primary Goal

Recover the Support Tickets route so the WPF runtime can be defended as a strict visual match to the canonical ticket reference, with the recent-list card as the highest-priority surface.

## Canonical Visual Sources

- `specs/014-ticket-screen-fidelity/spec.md`
- `stitch-downloads/Dorm/f800aa1e608c47bba0667fef296a6832/Quan-ly-yeu-cau-ho-tro-DormManagement.html`
- `stitch-downloads/Dorm/f800aa1e608c47bba0667fef296a6832/Quan-ly-yeu-cau-ho-tro-DormManagement.png`
- QA baseline: `.ai/artifacts/support-ticket-strong-qa-report.md`

## Primary Viewport Contract

Visible order must remain:
1. title + subtitle
2. top-right CTA
3. 3 summary cards
4. recent-list card

## Measurement-Sensitive Surfaces

### Critical: recent-list card

- card outer width and top margin
- header band height and padding
- table column widths and header/body alignment
- row height, divider rhythm, text density, icon spacing
- badge width/height/padding/corner radius/foreground/background
- footer padding and pager rhythm

### High: title + summary strip

- title baseline, weight, and line-height
- CTA size/padding/icon-text gap
- summary card height, internal gap, icon well size, label/value weight

## Known Mismatch Drivers From QA

- screenshot fixture data does not match canonical sample
- viewport/capture size not normalized to canonical PNG
- WPF type reads heavier than browser export
- delete action affordance weaker than ref
- secondary states still contain English copy

## Non-Goals

- backend workflow redesign
- role/auth changes
- broader shell redesign outside route-body parity
