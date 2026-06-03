# Database Schema

EF Core Code First creates the SQL Server schema. Important tables include:

- `roles`, `users`, `students`, `managers`
- `buildings`, `floors`, `rooms`
- `room_registrations`, `room_assignments`, `contracts`
- `fee_types`, `fee_rates`, `invoices`, `invoice_items`, `invoice_adjustments`
- `payments`, `payment_allocations`, `utility_readings`
- `vehicle_registrations` (parking month count, amount, linked invoice, payment date)
- `support_tickets`, `support_ticket_responses`
- `forum_topics`, `forum_threads`, `forum_comments`, `forum_likes`
- `notifications`, `user_notifications`, `audit_logs`

## Key Constraints

- Unique users by email and username.
- Unique student code and one user account per student.
- Unique room number within building/floor.
- One active room assignment per student.
- Unique monthly utility invoice per student/room/billing period.
- Unique invoice QR transfer content and optional invoice bank transaction ID.
- Unique payment code and optional transaction reference.
- Active vehicle license plate uniqueness is enforced in `VehicleService` so expired history rows can keep the same normalized plate.
- Unique forum like per user/thread or user/comment.
- Unique user notification per notification/user.

Money uses `decimal(18,2)`; do not use floating-point types for currency.

Vehicle parking invoices use `InvoiceKind.VehicleParking`, invoice numbers like `INV-PARK-yyyyMM-xxxxxxxx`, and one invoice item for `MonthCount * 40000`.

QR banking payments store `transfer_content`, `qr_data_url`, and optional `bank_transaction_id` on `invoices`. Successful bank matches create a `Payment` with `PaymentMethod.QrBanking`, `Status.Success`, `TransactionRef` set to the bank transaction ID, and one `payment_allocations` row for the matched invoice.
