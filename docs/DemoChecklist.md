# Demo Checklist

## Pre-demo

- Copy `src/DormitoryManagement.WPF/appsettings.example.json` to `src/DormitoryManagement.WPF/appsettings.Development.json`.
- Set `ConnectionStrings:DormitoryDb` for local SQL Server or LocalDB.
- From `src/`, run:

```powershell
dotnet restore DormitoryManagement.sln
dotnet ef database update --project DormitoryManagement.Infrastructure --startup-project DormitoryManagement.WPF
dotnet build DormitoryManagement.sln
dotnet test ..\tests\DormitoryManagement.Application.Tests\DormitoryManagement.Application.Tests.csproj
dotnet test ..\tests\DormitoryManagement.Infrastructure.Tests\DormitoryManagement.Infrastructure.Tests.csproj
dotnet run --project DormitoryManagement.WPF
```

## Demo Accounts

All demo accounts use password `123456`.

| Role | Login |
| --- | --- |
| Admin | `admin` or `admin@ktx.local` |
| Manager | `manager` or `manager@ktx.local` |
| Manager | `building.manager` or `building.manager@ktx.local` |
| Manager | `staff` or `staff@ktx.local` |
| Student | `student01`, `student01@ktx.local`, or `SV001` |

## Final Walkthrough

1. Sign in as `student01`.
2. Open Dashboard and confirm current room, debt, tickets, and notification counts load from seeded data.
3. If the student has no room, click `Phòng của tôi`, confirm the room-registration popup opens, submit a room request, and confirm the card shows the requested room with `(đang chờ xử lí)`.
4. Open Registrations and confirm the same pending row appears.
5. Open Billing and Payments, review invoices, create a payment for an outstanding invoice when available.
6. Open Support Tickets, create a ticket, confirm it appears in the queue.
7. Sign out, sign in as `manager`.
8. Open Registrations, approve or reject a pending request, then confirm audit/notification updates.
9. Open Billing, generate monthly invoices for a future period, review created/skipped/warning counts.
10. Open Payments, confirm a pending payment, then review updated invoice status.
11. Open Audit Logs and Notifications to show recent actions.
12. Sign out, sign in as `staff`, confirm sidebar shows Support Tickets and Forum only.
13. Update a support ticket status as staff.

## Watch Points

- WPF Output window should show no binding errors while opening Dashboard, Registrations, Billing, Payments, Tickets, Audit Logs, and Notifications.
- Empty states should appear after filters return no rows.
- Loading overlays should appear during refresh or save operations.
- Error states should appear for invalid form input or denied actions.

