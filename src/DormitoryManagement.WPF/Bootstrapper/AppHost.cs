using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DormitoryManagement.WPF.Bootstrapper;

public static class AppHost
{
    public static IHost Build()
    {
        return Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                builder.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) => services.AddWpfServices(context.Configuration))
            .Build();
    }
}
