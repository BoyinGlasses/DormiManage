using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.Security;
using DormitoryManagement.Application.Services.Auth;
using DormitoryManagement.Application.Services.Billing;
using DormitoryManagement.Application.Services.Contracts;
using DormitoryManagement.Application.Services.Dashboard;
using DormitoryManagement.Application.Services.Forum;
using DormitoryManagement.Application.Services.Payments;
using DormitoryManagement.Application.Services.Registrations;
using DormitoryManagement.Application.Services.Rooms;
using DormitoryManagement.Application.Services.Settings;
using DormitoryManagement.Application.Services.Students;
using DormitoryManagement.Application.Services.SupportTickets;
using DormitoryManagement.Application.Services.Users;
using DormitoryManagement.Application.Services.Vehicles;
using DormitoryManagement.Infrastructure;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels;
using DormitoryManagement.WPF.ViewModels.Audit;
using DormitoryManagement.WPF.ViewModels.Auth;
using DormitoryManagement.WPF.ViewModels.Billing;
using DormitoryManagement.WPF.ViewModels.Dashboard;
using DormitoryManagement.WPF.ViewModels.Forum;
using DormitoryManagement.WPF.ViewModels.Registrations;
using DormitoryManagement.WPF.ViewModels.Rooms;
using DormitoryManagement.WPF.ViewModels.Settings;
using DormitoryManagement.WPF.ViewModels.Students;
using DormitoryManagement.WPF.ViewModels.SupportTickets;
using DormitoryManagement.WPF.ViewModels.Vehicles;
using DormitoryManagement.WPF.Views.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DormitoryManagement.WPF.Bootstrapper;

public static class DependencyInjection
{
    public static IServiceCollection AddWpfServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton(_ =>
        {
            var options = new SecurityOptions();
            configuration.GetSection(SecurityOptions.SectionName).Bind(options);
            return options;
        });
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddSingleton<IOtpGenerator, NumericOtpGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountRegistrationService, AccountRegistrationService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IRoomRegistrationService, RoomRegistrationService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentExtensionService, PaymentExtensionService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ISupportTicketService, SupportTicketService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IFeeTypeService, FeeTypeService>();
        services.AddScoped<IForumService, ForumService>();

        services.AddSingleton<NavigationStore>();
        services.AddSingleton<SessionState>();
        services.AddSingleton<PaymentNavigationState>();
        services.AddSingleton<ILoginPrefillState, LoginPrefillState>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<AuditLogListViewModel>();
        services.AddTransient<AdminDashboardViewModel>();
        services.AddTransient<StudentDashboardViewModel>();
        services.AddTransient<StudentListViewModel>();
        services.AddTransient<StudentDetailViewModel>();
        services.AddTransient<RoomListViewModel>();
        services.AddTransient<RoomDetailViewModel>();
        services.AddTransient<RoomRegistrationViewModel>();
        services.AddTransient<RegistrationApprovalViewModel>();
        services.AddTransient<InvoiceListViewModel>();
        services.AddTransient<InvoiceDetailViewModel>();
        services.AddTransient<PaymentViewModel>();
        services.AddTransient<DebtListViewModel>();
        services.AddTransient<VehicleRegistrationViewModel>();
        services.AddTransient<SupportTicketListViewModel>();
        services.AddTransient<SupportTicketDetailViewModel>();
        services.AddTransient<ForumHomeViewModel>();
        services.AddTransient<ForumPostDetailViewModel>();
        services.AddTransient<UserManagementViewModel>();
        services.AddTransient<RoleManagementViewModel>();
        services.AddTransient<FeeTypeViewModel>();

        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}

