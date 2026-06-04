using System.Windows;
using DormitoryManagement.Infrastructure.Data;
using DormitoryManagement.WPF.Bootstrapper;
using DormitoryManagement.WPF.Views.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DormitoryManagement.WPF;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _host = AppHost.Build();
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<DbInitializer>().InitializeAsync();
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Không thể khởi động ứng dụng",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }

        base.OnExit(e);
    }
}