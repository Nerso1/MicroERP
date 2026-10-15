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

builder.Services.AddSingleton(_ => ProcessedMessagesTableFactory.Create(builder.Configuration));
builder.Services.AddSingleton<IProcessedMessageStore, TableProcessedMessageStore>();

var host = builder.Build();

// Fail fast: without the deduplication table every message would fail and end up in the dead-letter queue.
host.Services.GetRequiredService<TableClient>().CreateIfNotExists();

host.Run();
