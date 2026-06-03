# Architecture

DormitoryManagement uses a Layered MVVM architecture for a WPF desktop application.

## Dependency Rules

- Domain references no project.
- Application references Domain only.
- Infrastructure references Domain and Application.
- WPF references Domain, Application, and Infrastructure for DI composition.
- Application never references Infrastructure.
- ViewModels never call `DormitoryDbContext` directly.

## Layer Responsibilities

- Domain: entities, enums, constants, and basic business invariants.
- Application: DTOs, request validation, authorization checks, transaction boundaries, service interfaces, and service implementations.
- Infrastructure: EF Core DbContext, repository implementations, unit of work, migrations, seed data, password hashing, notifications, and audit persistence.
- WPF: Views, ViewModels, commands, converters, resources, navigation, and bootstrap/DI.
