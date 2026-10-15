using Azure.Identity;
using Azure.Messaging.ServiceBus;
using MicroERP.Application.Common;
using MicroERP.Infrastructure.Persistence;
using MicroERP.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddStockAlertPublisher(configuration);

        return services;
    }

    private static void AddStockAlertPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var fullyQualifiedNamespace = configuration["AzureServiceBus:FullyQualifiedNamespace"];
        var connectionString = configuration["AzureServiceBus:ConnectionString"];

        // Without Azure (docker compose, CI, fresh clone) alerts are only logged.
        if (string.IsNullOrWhiteSpace(fullyQualifiedNamespace) && string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton<IStockAlertPublisher, LoggingStockAlertPublisher>();
            return;
        }

        var queueName = configuration["AzureServiceBus:QueueName"]
            ?? throw new InvalidOperationException("AzureServiceBus:QueueName is not configured.");

        // Identity-based connection (Managed Identity in Azure, developer account locally) takes precedence over SAS.
        services.AddSingleton(_ => string.IsNullOrWhiteSpace(fullyQualifiedNamespace)
            ? new ServiceBusClient(connectionString)
            : new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential()));
        services.AddSingleton<IStockAlertPublisher>(sp =>
            new ServiceBusStockAlertPublisher(sp.GetRequiredService<ServiceBusClient>(), queueName));
    }
}
