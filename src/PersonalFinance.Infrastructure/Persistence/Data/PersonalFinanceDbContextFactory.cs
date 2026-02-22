using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PersonalFinance.Infrastructure.Persistence;

public class PersonalFinanceDbContextFactory : IDesignTimeDbContextFactory<PersonalFinanceDbContext>
{
    public PersonalFinanceDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<PersonalFinanceDbContext>();

        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new PersonalFinanceDbContext(optionsBuilder.Options);
    }
}
