# Bản đồ codebase Dormitory

Tài liệu này được tạo từ phân tích CodeGraph/MCP cho repo `D:\Dormitory` vào ngày 2026-06-01.

## 1. Tổng quan dự án

- Sản phẩm chính: ứng dụng desktop WPF để quản lý ký túc xá.
- Solution chính: `src/DormitoryManagement.sln`
- Điểm chạy đầu tiên: `src/DormitoryManagement.WPF/App.xaml.cs:14`
- Cấu hình và DI: `src/DormitoryManagement.WPF/Bootstrapper/AppHost.cs:8`, `src/DormitoryManagement.WPF/Bootstrapper/DependencyInjection.cs:38`
- Điều hướng: dùng điều hướng view-model trong WPF thông qua `INavigationService`, không phải route HTTP.
- Lưu trữ dữ liệu: EF Core 10 + SQL Server thông qua `src/DormitoryManagement.Infrastructure/Data/DormitoryDbContext.cs`
- Các lớp chính:
  - `Domain` -> entity, enum, hằng số, quy tắc cơ bản
  - `Application` -> service contract, DTO, validation, auth, permission
  - `Infrastructure` -> EF Core, repository, seed/init, adapter hạ tầng
  - `WPF` -> view, view-model, shell, navigation, giao diện desktop
- Test:
  - `tests/DormitoryManagement.Application.Tests`
  - `tests/DormitoryManagement.Infrastructure.Tests`
  - `tests/DormitoryManagement.WPF.Tests`
- Phần phụ:
  - `dmforum/` là prototype/demo nhỏ bằng Vite/React.
  - `.graphify_vendor/` và `graphify-out/` là dữ liệu phục vụ phân tích, không phải runtime chính của app.

## 2. Cây thư mục / module chính

```text
D:\Dormitory
├── src/
│   ├── DormitoryManagement.sln
│   ├── DormitoryManagement.Domain/
│   │   ├── Common/
│   │   ├── Constants/
│   │   ├── Entities/
│   │   └── Enums/
│   ├── DormitoryManagement.Application/
│   │   ├── Abstractions/
│   │   ├── Common/
│   │   ├── DTOs/
│   │   ├── Security/
│   │   ├── Services/
│   │   └── Validation/
│   ├── DormitoryManagement.Infrastructure/
│   │   ├── Data/
│   │   ├── Migrations/
│   │   ├── Repositories/
│   │   └── Services/
│   └── DormitoryManagement.WPF/
│       ├── Bootstrapper/
│       ├── Common/
│       ├── Converters/
│       ├── Navigation/
│       ├── Resources/
│       ├── ViewModels/
│       └── Views/
├── tests/
│   ├── DormitoryManagement.Application.Tests/
│   ├── DormitoryManagement.Infrastructure.Tests/
│   └── DormitoryManagement.WPF.Tests/
└── dmforum/
    └── src/
```

## 3. Điểm vào chính

### Khởi động ứng dụng WPF

1. `src/DormitoryManagement.WPF/App.xaml.cs:14` -> `App.OnStartup`
2. `src/DormitoryManagement.WPF/Bootstrapper/AppHost.cs:8` -> `Build()`
3. `Build()` đọc cấu hình từ `appsettings.json`, `appsettings.Development.json`, biến môi trường
4. `Build()` gọi `AddWpfServices(...)`
5. `AddWpfServices(...)` gọi tiếp `AddInfrastructure(...)`
6. Lúc khởi động, app resolve `DbInitializer`, seed/khởi tạo DB, rồi resolve `MainWindow`
7. `MainWindow` hiển thị shell và view-model hiện tại

### Điểm vào điều hướng desktop

- `src/DormitoryManagement.WPF/Navigation/NavigationService.cs:18` -> `NavigateTo<TViewModel>()`
- Mỗi lần chuyển màn hình sẽ tạo một DI scope.
- View-model cần mở được resolve từ DI.
- View-model đó được gán vào `NavigationStore.CurrentViewModel`.

### Điểm vào prototype frontend

- `dmforum/src/main.tsx`
- `dmforum/src/App.tsx`

Phần này tách khỏi runtime chính của WPF app.

## 4. Class / function / component quan trọng

### Bootstrap và DI

- `App` - vòng đời WPF, start/stop host, init DB, mở window.
- `AppHost.Build()` - nơi dựng host và đọc cấu hình.
- `DependencyInjection.AddWpfServices()` - đăng ký service, state, navigation, view-model, shell.
- `Infrastructure.DependencyInjection.AddInfrastructure()` - đăng ký DbContext, repository, password/session/audit/email infra.

### Navigation và shell

- `NavigationStore` - giữ view-model hiện tại.
- `NavigationService` - resolve view-model theo scope và chuyển màn hình.
- `ShellViewModel` - menu shell, trạng thái chrome, notification, logout, điều hướng.
- `ShellViewModel.NavigateByKey()` - bảng route trung tâm của app desktop.

Các key route/menu trong `ShellViewModel`:

- `Login`
- `AdminDashboard`
- `StudentDashboard`
- `Students`
- `Rooms`
- `RoomRegistration`
- `RegistrationApproval`
- `Invoices`
- `InvoiceDetail`
- `Payments`
- `AuditLogs`
- `Vehicles`
- `Tickets`
- `TicketDetail`
- `Topics`
- `Users`
- `FeeTypes`

### Auth và session

- `LoginViewModel` - validate form đăng nhập, gọi `IAuthService.LoginAsync`, chuyển màn hình theo role.
- `AuthService` - đăng nhập, đăng xuất, đổi mật khẩu, khóa tài khoản, audit.
- `SessionService` / `SessionState` - cầu nối trạng thái người dùng hiện tại giữa các layer.
- `PermissionService` - kiểm tra quyền ở application service.

### Dashboard và luồng sinh viên

- `StudentDashboardViewModel`
  - command: làm mới, đăng ký phòng, thanh toán hóa đơn, tạo ticket, mở forum
  - gọi `IDashboardService.GetStudentDashboardAsync(...)`
- `DashboardService`
  - `GetAdminDashboardAsync(...)`
  - `GetStudentDashboardAsync(...)`
  - `ResolveStudentId(...)`
  - `ResolveRoomLabel(...)`

### Dữ liệu và lưu trữ

- `DormitoryDbContext` - gốc EF Core.
- `DbInitializer` / `SeedData` - tạo dữ liệu demo/bootstrap.
- `EfRepository<T>` + repository chuyên biệt.
- `UnitOfWork`.

### Forum

- `ForumHomeViewModel` - giao diện forum home dùng dữ liệu preview.
- `ForumPostDetailViewModel` - giao diện chi tiết bài forum.
- `ForumHomePreviewFactory` / `ForumPostDetailPreviewFactory` - tạo dữ liệu mẫu ổn định cho màn hình forum trong WPF.

## 5. Graph quan hệ module / function

### Quan hệ layer cấp cao

```text
DormitoryManagement.WPF
  -> DormitoryManagement.Application
  -> DormitoryManagement.Infrastructure
  -> DormitoryManagement.Domain

DormitoryManagement.Infrastructure
  -> DormitoryManagement.Application
  -> DormitoryManagement.Domain

DormitoryManagement.Application
  -> DormitoryManagement.Domain

DormitoryManagement.Domain
  -> không phụ thuộc project nào khác
```

### Graph runtime chính

```text
App.OnStartup
  -> AppHost.Build
  -> AddWpfServices
  -> AddInfrastructure
  -> DbInitializer.InitializeAsync
  -> MainWindow.Show

ShellViewModel
  -> NavigationService.NavigateTo<T>
  -> NavigationStore.CurrentViewModel

LoginViewModel
  -> IAuthService.LoginAsync
  -> SessionState.NotifyChanged
  -> NavigateTo<StudentDashboardViewModel | SupportTicketListViewModel | AdminDashboardViewModel>

StudentDashboardViewModel.LoadAsync
  -> IServiceScopeFactory.CreateScope
  -> IDashboardService.GetStudentDashboardAsync
  -> ApplyDashboard

DashboardService.GetStudentDashboardAsync
  -> ResolveStudentId
  -> UnitOfWork.Repository<Student>
  -> UnitOfWork.Repository<RoomAssignment>
  -> ResolveRoomLabel
```

## 6. API / routes

### Trạng thái HTTP/API

- Không thấy ASP.NET controller.
- Không thấy attribute `ApiController`.
- Không thấy dùng `RouteAttribute`.
- App chính là desktop app, không phải REST API.

### Route thực tế = key điều hướng WPF

Từ `src/DormitoryManagement.WPF/ViewModels/ShellViewModel.cs:145` trở đi:

- `Login` -> `LoginViewModel`
- `AdminDashboard` -> `AdminDashboardViewModel`
- `StudentDashboard` -> `StudentDashboardViewModel`
- `Students` -> `StudentListViewModel`
- `Rooms` -> `RoomListViewModel`
- `RoomRegistration` -> `RoomRegistrationViewModel`
- `RegistrationApproval` -> `RegistrationApprovalViewModel`
- `Invoices` -> `InvoiceListViewModel`
- `InvoiceDetail` -> `InvoiceDetailViewModel`
- `Payments` -> `PaymentViewModel`
- `AuditLogs` -> `AuditLogListViewModel`
- `Vehicles` -> `VehicleRegistrationViewModel`
- `Tickets` -> `SupportTicketListViewModel`
- `TicketDetail` -> `SupportTicketDetailViewModel`
- `Topics` -> `ForumHomeViewModel`
- `Users` -> `UserManagementViewModel`
- `FeeTypes` -> `FeeTypeViewModel`

### Route từ quick action của sinh viên

Từ `StudentDashboardViewModel`:

- Đăng ký phòng -> `RoomRegistrationViewModel`
- Thanh toán hóa đơn -> `PaymentViewModel`
- Tạo yêu cầu hỗ trợ -> `SupportTicketListViewModel`
- Mở forum -> `ForumHomeViewModel`

## 7. Luồng xử lý chính

### Luồng A - Khởi động app

1. WPF chạy `App.OnStartup`
2. Host builder đọc cấu hình
3. DI đăng ký infrastructure, app service và WPF view-model
4. DB initializer chạy
5. `MainWindow` được resolve và hiển thị
6. `ShellViewModel` mở màn hình `LoginViewModel` đầu tiên

### Luồng B - Đăng nhập

1. `LoginViewModel.LoginAsync()` validate form
2. Gọi `IAuthService.LoginAsync(...)`
3. `AuthService` validate request
4. Tìm user theo email/username/mã sinh viên
5. Kiểm tra trạng thái khóa/vô hiệu hóa
6. Verify password hash
7. Cập nhật số lần đăng nhập sai, audit, session
8. Trả về `LoginResult`
9. `LoginViewModel` chuyển màn hình theo role:
   - student -> `StudentDashboardViewModel`
   - staff -> `SupportTicketListViewModel`
   - role khác -> `AdminDashboardViewModel`

### Luồng C - Làm mới dashboard sinh viên

1. `StudentDashboardViewModel.LoadAsync()` chạy
2. Mở DI scope
3. Resolve `IDashboardService`
4. Lấy student id từ session/current user
5. `DashboardService.GetStudentDashboardAsync(...)` tính phòng, nợ, ticket, notification
6. View-model đưa DTO vào các property hiển thị

### Luồng D - Điều hướng trong shell

1. Menu shell gọi `NavigateByKey(key)`
2. `ShellViewModel` map key sang view-model đích
3. `NavigationService.NavigateTo<T>()` resolve view-model mới theo scope
4. `NavigationStore.CurrentViewModel` thay đổi
5. Shell phản ứng qua `OnNavigationStoreChanged`

### Luồng E - Seed/demo data

1. Startup resolve `DbInitializer`
2. `DbInitializer` tạo hoặc cập nhật user, student, contract, payment, ticket, audit log demo
3. App có sẵn dữ liệu để chạy thử/review

## 8. Nên đọc file nào trước

Thứ tự đọc khuyến nghị cho người mới vào repo:

1. `src/DormitoryManagement.WPF/App.xaml.cs`
2. `src/DormitoryManagement.WPF/Bootstrapper/AppHost.cs`
3. `src/DormitoryManagement.WPF/Bootstrapper/DependencyInjection.cs`
4. `src/DormitoryManagement.Infrastructure/DependencyInjection.cs`
5. `src/DormitoryManagement.WPF/Navigation/NavigationService.cs`
6. `src/DormitoryManagement.WPF/ViewModels/ShellViewModel.cs`
7. `src/DormitoryManagement.WPF/ViewModels/Auth/LoginViewModel.cs`
8. `src/DormitoryManagement.Application/Services/Auth/AuthService.cs`
9. `src/DormitoryManagement.WPF/ViewModels/Dashboard/StudentDashboardViewModel.cs`
10. `src/DormitoryManagement.Application/Services/Dashboard/DashboardService.cs`
11. `src/DormitoryManagement.Infrastructure/Data/DormitoryDbContext.cs`
12. `src/DormitoryManagement.Infrastructure/Data/DbInitializer.cs`

Nếu đang debug theo khu vực:

- Auth/session: `LoginViewModel` -> `AuthService` -> `SessionService`
- Navigation: `ShellViewModel` -> `NavigationService` -> `NavigationStore`
- Dashboard sinh viên: `StudentDashboardViewModel` -> `DashboardService`
- Billing/payment: `PaymentViewModel` -> `PaymentService` / `BillingService`
- Forum UI: `ForumHomeViewModel` -> các preview factory

## 9. Tín hiệu đáng chú ý

- Repo này thiên về desktop app, dù có một vài folder web/demo.
- Điều hướng view-model là phần gần giống route graph nhất.
- Nhiều behavior được gom ở service interface trong `Application`, rồi WPF resolve qua DI.
- `Infrastructure` giữ DB và adapter bên ngoài; `Application` giữ logic use-case.
- Forum có vẻ đang dùng nhiều dữ liệu preview/factory, chưa hoàn toàn là luồng backend-driven.

## 10. Nguồn phân tích đã dùng

Nguồn CodeGraph chính:

- `codegraph_status`
- `codegraph_files`
- `codegraph_context`
- `codegraph_search`
- `codegraph_explore`
- `codegraph_trace`
- `codegraph_node`

Symbol chính đã kiểm tra:

- `App.OnStartup`
- `AppHost.Build`
- `DependencyInjection.AddWpfServices`
- `Infrastructure.DependencyInjection.AddInfrastructure`
- `NavigationService.NavigateTo`
- `ShellViewModel.NavigateByKey`
- `LoginViewModel.LoginAsync`
- `AuthService.LoginAsync`
- `StudentDashboardViewModel.LoadAsync`
- `DashboardService.GetStudentDashboardAsync`
