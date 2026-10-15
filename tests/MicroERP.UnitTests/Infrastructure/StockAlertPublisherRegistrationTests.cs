using Azure.Messaging.ServiceBus;
using FluentAssertions;
using MicroERP.Application.Common;
using MicroERP.Infrastructure;
using MicroERP.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroERP.UnitTests.Infrastructure;

public class StockAlertPublisherRegistrationTests
{
    private const string IdentityNamespace = "identity-namespace.servicebus.windows.net";
    private const string SasNamespace = "sas-namespace.servicebus.windows.net";

    // Fake SAS connection string — ServiceBusClient only parses it, no connection is opened.
    private const string ConnectionString =
        $"Endpoint=sb://{SasNamespace}/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";

    [Fact]
    public async Task AddInfrastructure_NoServiceBusConfig_RegistersLoggingPublisher()
    {
        await using var provider = BuildProvider([]);

        provider.GetRequiredService<IStockAlertPublisher>()
            .Should().BeOfType<LoggingStockAlertPublisher>();
    }

    [Fact]
    public async Task AddInfrastructure_ConnectionString_UsesSasClient()
    {
        await using var provider = BuildProvider(new()
        {
            ["AzureServiceBus:ConnectionString"] = ConnectionString
        });

        provider.GetRequiredService<IStockAlertPublisher>()
            .Should().BeOfType<ServiceBusStockAlertPublisher>();
        provider.GetRequiredService<ServiceBusClient>().FullyQualifiedNamespace
            .Should().Be(SasNamespace);
    }

    [Fact]
    public async Task AddInfrastructure_FullyQualifiedNamespace_UsesIdentityBasedClient()
    {
        await using var provider = BuildProvider(new()
        {
            ["AzureServiceBus:FullyQualifiedNamespace"] = IdentityNamespace
        });

        provider.GetRequiredService<IStockAlertPublisher>()
            .Should().BeOfType<ServiceBusStockAlertPublisher>();
        provider.GetRequiredService<ServiceBusClient>().FullyQualifiedNamespace
            .Should().Be(IdentityNamespace);
    }

    [Fact]
    public async Task AddInfrastructure_BothConfigured_PrefersIdentityBasedClient()
    {
        await using var provider = BuildProvider(new()
        {
            ["AzureServiceBus:FullyQualifiedNamespace"] = IdentityNamespace,
            ["AzureServiceBus:ConnectionString"] = ConnectionString
        });

        provider.GetRequiredService<ServiceBusClient>().FullyQualifiedNamespace
            .Should().Be(IdentityNamespace);
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        settings["AzureServiceBus:QueueName"] = "stock-alerts";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);

        return services.BuildServiceProvider();
    }
}
