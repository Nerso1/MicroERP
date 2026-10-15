using FluentAssertions;
using MicroERP.Functions.Processing;
using Microsoft.Extensions.Configuration;

namespace MicroERP.UnitTests.Functions;

public class ProcessedMessagesTableFactoryTests
{
    [Fact]
    public void Create_ConnectionString_UsesConnectionString()
    {
        var table = ProcessedMessagesTableFactory.Create(BuildConfiguration(new()
        {
            ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true"
        }));

        table.AccountName.Should().Be("devstoreaccount1");
        table.Name.Should().Be(ProcessedMessagesTableFactory.TableName);
    }

    [Fact]
    public void Create_AccountName_UsesIdentityBasedEndpoint()
    {
        var table = ProcessedMessagesTableFactory.Create(BuildConfiguration(new()
        {
            ["AzureWebJobsStorage:accountName"] = "microerpstorage"
        }));

        table.Uri.Host.Should().Be("microerpstorage.table.core.windows.net");
        table.Name.Should().Be(ProcessedMessagesTableFactory.TableName);
    }

    [Fact]
    public void Create_NoStorageConfig_Throws()
    {
        var act = () => ProcessedMessagesTableFactory.Create(BuildConfiguration([]));

        act.Should().Throw<InvalidOperationException>();
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> settings) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
}
