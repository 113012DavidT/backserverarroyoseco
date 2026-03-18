using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace arroyoSeco.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Carga configuraci�n (appsettings.* del proyecto API si se ejecuta desde ra�z)
        var basePath = Directory.GetCurrentDirectory();
        var builderConfig = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables();

        var configuration = builderConfig.Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=arroyoSeco;Username=postgres;Password=;";
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}