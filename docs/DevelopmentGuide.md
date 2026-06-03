# Development Guide

## Requirements

- Windows with .NET 10 runtime/SDK support for WPF.
- SQL Server or LocalDB.
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`.

## Configuration

Copy `src/DormitoryManagement.WPF/appsettings.example.json` to `appsettings.Development.json` and edit the connection string. Do not commit personal connection strings.

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
