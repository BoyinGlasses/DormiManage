# Use Cases

## Core Dormitory Management

- Manage students, accounts, roles, and permissions.
- Manage buildings, floors, rooms, room capacity, gender policy, and status.
- Students register for rooms. Students without a room can start registration directly from the Dashboard `Phòng của tôi` card, see pending room state there, and managers approve, reject, or cancel registration requests.
- Create room assignments and contracts after approval.
- Generate monthly invoices from room fees, fee rates, and utility readings.
- Record mock payments and allocate them to invoices.
- Admins, managers, and building managers enter room utility readings by billing period, preview electricity, water, internet, and total charges, then confirm monthly utility invoices.
- Monthly utility invoices use the `MonthlyUtility` kind, are due on day 5 of the month after the billing period, include mandatory internet for every active room member, split the room utility total equally per member, and are protected from duplicate student-room-period utility billing.
- Students pay monthly utility charges from the generated due date. Unpaid monthly utility invoices may request one due-date extension of up to 5 extra days, capped at day 15 of the invoice due-date month.
- Staff can generate QR banking instructions for an unpaid invoice. Students view their own invoice amount, transfer content, QR image, due date, and payment status from Payments, then refresh status after transferring.
- Verified bank transfer notifications are matched by exact remaining amount and transfer content in the transaction description; one confident match marks the invoice paid and records payment history.
- Students open Payments from Billing row actions only; the direct student Payments menu item is hidden while contextual payment navigation keeps the selected own invoice.
- Students register vehicle parking with normalized plates and month options `1/2/3/6/9/12`.
- Vehicle registration creates an unpaid Billing invoice due 2 calendar days after registration; overdue unpaid invoices show as `Overdue`.
- Submit, assign, update, and close support tickets.
- View dashboard reports for revenue, occupancy, debt, and tickets.

## Forum Module

Forum is currently being rebuilt as a UI-first WPF surface. Dormitory operations remain the primary domain.

- Shell `Forum` and student dashboard `Open Forum` land on the dedicated `ForumHome` rebuild surface.
- `ForumHome` is currently UI-first: it uses deterministic preview cards, local filter selection, local search text, preview dropdowns, empty/reset states, and non-destructive placeholder feedback rather than live forum mutations.
- `ForumHome` Phase 1 review starts from the exact reference baseline only: first-load screenshot state keeps preview-only dropdowns, filter summaries, empty-state cards, and emergency preview panels hidden unless the reviewer explicitly triggers them.
- `ForumHome` search/category/tag/area filters are preview-only. They update the visible feed/activity/support slices locally, expose a reset path, and can intentionally show an empty-result review state without changing stored forum data.
- `ForumHome` message, notification, and profile surfaces open as dismissible local preview dropdowns. Feed likes, activity interest toggles, and emergency-contact previews also stay local to the review session and do not persist counts, joins, comments, or call state.
- ForumPostDetail opens from the rebuilt ForumHome feed and keeps breadcrumb, related-post, category, tag, reply, report, like, comment-draft, and create-post interactions inside deterministic review-only WPF state. Only approved shell navigation back to ForumHome is allowed; no live forum service calls, persistence writes, or authorization bypass paths are reintroduced.
- Compose and student-activity actions stay local to the preview surface while the new forum flow is being rebuilt.
- The old topic list, thread create, thread detail, live forum service, forum repository, forum DTOs, and forum persistence entities have been removed from the active code path.


## Login Route Fidelity

- The unauthenticated login route is a WPF-first high-fidelity rebuild anchored to the approved Stitch HTML reference under .ai/artifacts/stitch/5015874631646835452/8a582a7afabb40ceb7a4478cdd6fda95/.
- Đăng ký tại đây must keep the current registration handoff intact.
- Quên mật khẩu? and footer links are visual and click-safe only in this phase; they do not open new backend or recovery workflows.
- Login review uses deterministic local assets, route-scoped WPF resources, and screenshot comparison against the approved reference artifact.