using Azure.Data.Tables;
using Azure.Monitor.OpenTelemetry.Exporter;
using MicroERP.Functions.Processing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration["AzureWebJobsStorage"]
        ?? throw new InvalidOperationException("AzureWebJobsStorage is not configured.");

    var table = new TableClient(connectionString, "ProcessedMessages");
    table.CreateIfNotExists();
    return table;
});
builder.Services.AddSingleton<IProcessedMessageStore, TableProcessedMessageStore>();

builder.Build().Run();
