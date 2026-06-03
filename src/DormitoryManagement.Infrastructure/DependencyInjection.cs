using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Payments;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Infrastructure.Data;
using DormitoryManagement.Infrastructure.Repositories;
using DormitoryManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DormitoryManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DormitoryDb")
            ?? throw new InvalidOperationException("ConnectionStrings:DormitoryDb is missing.");

        services.AddDbContext<DormitoryDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<DbInitializer>();

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IPendingAccountRegistrationRepository, PendingAccountRegistrationRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<SessionService>();
        services.AddSingleton<ISessionService>(sp => sp.GetRequiredService<SessionService>());
        services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<SessionService>());
        services.AddSingleton<IRememberedLoginService, RememberedLoginService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton(EmailOptions.FromConfiguration(configuration));
        services.AddSingleton(_ =>
        {
            var section = configuration.GetSection(QrBankingOptions.SectionName);
            return new QrBankingOptions
            {
                BankAccountNo = section[nameof(QrBankingOptions.BankAccountNo)] ?? string.Empty,
                BankAccountName = section[nameof(QrBankingOptions.BankAccountName)] ?? string.Empty,
                BankAcqId = section[nameof(QrBankingOptions.BankAcqId)] ?? string.Empty,
                VietQrClientId = section[nameof(QrBankingOptions.VietQrClientId)] ?? string.Empty,
                VietQrApiKey = section[nameof(QrBankingOptions.VietQrApiKey)] ?? string.Empty,
                WebhookSecret = section[nameof(QrBankingOptions.WebhookSecret)] ?? string.Empty
            };
        });
        services.AddTransient<IEmailSender, MailKitEmailSender>();
        services.AddScoped<IVietQrService, VietQrService>();
        services.AddSingleton<MockPaymentGateway>();

        return services;
    }
}
