using ConfigurationReader.Domain.Interfaces;
using ConfigurationReader.Persistence.Context;
using ConfigurationReader.Persistence.Repositories;
using ConfigurationReader.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationReader.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext - SQL Server
        //services.AddDbContext<ConfigurationDbContext>(options =>
        //    options.UseSqlServer(
        //        configuration.GetConnectionString("DefaultConnection"),
        //        b => b.MigrationsAssembly(typeof(ConfigurationDbContext).Assembly.FullName)));

         services.AddDbContext<ConfigurationDbContext>(options =>
             options.UseNpgsql(
                 configuration.GetConnectionString("PostgreSQL"),
                 b => b.MigrationsAssembly(typeof(ConfigurationDbContext).Assembly.FullName)));

        // Repository
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        
        // For seed data
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}