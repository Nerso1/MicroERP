using Azure;
using Azure.Data.Tables;

namespace MicroERP.Functions.Processing;

public class TableProcessedMessageStore : IProcessedMessageStore
{
    private const string PartitionKey = "stock-alerts";

    private readonly TableClient _table;

    public TableProcessedMessageStore(TableClient table) => _table = table;

    public async Task<bool> TryMarkProcessedAsync(string messageId, CancellationToken cancellationToken)
    {
        try
        {
            await _table.AddEntityAsync(new TableEntity(PartitionKey, messageId), cancellationToken);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            return false;
        }
    }

    public Task UnmarkAsync(string messageId, CancellationToken cancellationToken)
        => _table.DeleteEntityAsync(PartitionKey, messageId, cancellationToken: cancellationToken);
}
