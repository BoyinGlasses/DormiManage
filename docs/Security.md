# Security

This is a WPF desktop application without REST API endpoints. It does not use JWT, API rate limiting, controllers, or ASP.NET Core Web API.

## Authentication

- Passwords are stored only as PBKDF2-SHA256 hashes.
- Plain text passwords are never persisted.
- `User` tracks `PasswordHash`, `FailedLoginCount`, `LockedUntil`, and `LastLoginAt`.
- `AuthService` handles login, logout, password change, active-account checks, and lockout after repeated failures.

## Authorization

The UI may hide buttons for usability, but Application Services enforce permissions.

- Students access their own data only.
- Students can register vehicle parking only for their own profile and only after they have a current approved room.
- Billing reads and monthly utility previews are enforced by Application Services. Student billing reads are scoped to the current student; managers with an assigned building are scoped to assigned building rooms.
- Monthly utility reading entry and invoice confirmation require billing write permission. UI visibility is not the authorization boundary.
- Payment extension requests are service-validated: only the owning student can request an extension, only for unpaid `MonthlyUtility` invoices, for at most 5 additional days, and never later than day 15 of the billing month.
- Student payment navigation is contextual from Billing rows, but payment services still validate selected invoice ownership and payable balance.
- QR banking invoice reads are service-validated: students may view QR payment details only for their own invoices. Student refresh actions cannot mark invoices paid.
- QR generation requires billing write permission. Automatic bank reconciliation accepts only validated transaction DTOs from a trusted boundary; WPF student flows never call direct payment confirmation.
- Managers with an assigned building are scoped to that building.
- Managers update assigned tickets and other management workflows.
- Admins perform system administration.
- `ForumHome` review-build entry uses bundled local preview assets/data in the WPF layer, and its placeholder compose/message/notification/emergency/like interactions do not call live forum mutation services directly.
- `ForumHome` preview filters, dropdowns, activity interest toggles, and emergency-contact dialogs remain session-local WPF state only. They do not write forum rows, localStorage-style caches, audit rows, or identity/session changes.
- `ForumHome` compose, feed, activity, and emergency actions stay in preview mode while the new forum flow is being rebuilt.
- `ForumHome` review media must stay bundled, deterministic, and replaceable from source control or local assets. Review behavior must not depend on remote image fetches, private browser cache state, or personal machine paths.
- The forum-home rebuild does not add REST endpoints, JWT/web auth flows, background sync, or new API/service permission paths.

## Auditing

Audit logs should be written for repeated failed login, account lock/unlock, room approval/rejection, monthly utility invoice generation, payment extension request/approval/rejection, invoice changes, vehicle registration invoice creation, payment confirmation, room transfer/checkout, and ticket updates.

QR banking audit events include QR generation, successful bank matches, duplicate bank transaction notifications, and unmatched bank notifications. Bank account values, provider credentials, webhook secrets, and personal connection strings must stay in local configuration and must not appear in committed logs or docs.

