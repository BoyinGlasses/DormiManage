using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DormitoryManagement.Infrastructure.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DormitoryDbContext>
{
    public DormitoryDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile(Path.Combine("..", "DormitoryManagement.WPF", "appsettings.Development.json"), optional: true)
            .AddJsonFile(Path.Combine("..", "DormitoryManagement.WPF", "appsettings.example.json"), optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DormitoryDb")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=DormitoryManagementDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<DormitoryDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new DormitoryDbContext(options);
    }
}
