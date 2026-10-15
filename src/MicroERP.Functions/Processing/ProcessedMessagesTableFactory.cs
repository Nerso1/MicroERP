using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace MicroERP.Functions.Processing;

public static class ProcessedMessagesTableFactory
{
    public const string TableName = "ProcessedMessages";

    public static TableClient Create(IConfiguration configuration)
    {
        var storage = configuration.GetSection("AzureWebJobsStorage");

        // Azurite locally: AzureWebJobsStorage is a plain connection string.
        if (!string.IsNullOrWhiteSpace(storage.Value))
        {
            return new TableClient(storage.Value, TableName);
        }

        // Flex Consumption: identity-based connection (AzureWebJobsStorage__accountName).
        var accountName = storage["accountName"]
            ?? throw new InvalidOperationException("AzureWebJobsStorage is not configured.");

        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            // Set only for a user-assigned identity; null means system-assigned.
            ManagedIdentityClientId = storage["clientId"]
        });

        return new TableClient(new Uri($"https://{accountName}.table.core.windows.net"), TableName, credential);
    }
}
