# Development Guide

## Requirements

- Windows with .NET 10 runtime/SDK support for WPF.
- SQL Server or LocalDB.
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`.

## Configuration

Copy `src/DormitoryManagement.WPF/appsettings.example.json` to `appsettings.Development.json` and edit the connection string. Do not commit personal connection strings.

### PayOS

PayOS credentials must be supplied through environment variables or another local secret store, not committed JSON files. The WPF host loads environment variables after `appsettings*.json`, so these values override the placeholder `PayOs` section:

```powershell
pwsh .\scripts\configure-payos.ps1
```

Equivalent manual variables:

```powershell
$env:PayOs__ClientId = "<payos-client-id>"
$env:PayOs__ApiKey = "<payos-api-key>"
$env:PayOs__ChecksumKey = "<payos-checksum-key>"
$env:PayOs__BaseUrl = "https://api-merchant.payos.vn"
$env:PayOs__ReturnUrl = "http://localhost/payos/return"
$env:PayOs__CancelUrl = "http://localhost/payos/cancel"
$env:PayOs__WebhookListenPrefix = "http://localhost:5126/payos/webhook/"
```

For automatic bank-transfer confirmation, expose the local listener with a public HTTPS tunnel, set `PayOs__WebhookUrl` to the public URL ending in `/payos/webhook/`, then set `PayOs__AutoConfirmWebhook=true`. Keep it `false` until the public URL is stable.

## Build

```powershell
cd src
dotnet restore DormitoryManagement.sln
dotnet build DormitoryManagement.sln
```

## Validation

```powershell
cd src
dotnet test ..\tests\DormitoryManagement.WPF.Tests\DormitoryManagement.WPF.Tests.csproj
dotnet run --project DormitoryManagement.WPF
```

Focused login-fidelity validation during the WPF login rebuild:

```powershell
cd src
dotnet test ..\tests\DormitoryManagement.WPF.Tests\DormitoryManagement.WPF.Tests.csproj --filter "FullyQualifiedName~Login|FullyQualifiedName~ShellViewModelTests|FullyQualifiedName~WpfResourceTests"
```

## Migrations

```powershell
dotnet ef migrations add InitialCreate --project DormitoryManagement.Infrastructure --startup-project DormitoryManagement.WPF
dotnet ef database update --project DormitoryManagement.Infrastructure --startup-project DormitoryManagement.WPF
```

## Seed Data

`SeedData` and `DbInitializer` provide demo login accounts for local development. All demo accounts use password `123456`; passwords are stored as PBKDF2 hashes, never plain text.

| Role | Login |
| --- | --- |
| Admin | `admin@ktx.local` or `admin` |
| Manager | `manager@ktx.local` or `manager` |
| Manager | `building.manager@ktx.local` or `building.manager` |
| Manager | `staff@ktx.local` or `staff` |
| Student | `student01@ktx.local`, `student01`, or student code `SV001` |
