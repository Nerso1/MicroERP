namespace MicroERP.Functions.Processing;

public interface IProcessedMessageStore
{
    Task<bool> TryMarkProcessedAsync(string messageId, CancellationToken cancellationToken);
    Task UnmarkAsync(string messageId, CancellationToken cancellationToken);
}
