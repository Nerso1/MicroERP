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
    var table = ProcessedMessagesTableFactory.Create(builder.Configuration);
    table.CreateIfNotExists();
    return table;
});

builder.Services.AddSingleton<IProcessedMessageStore, TableProcessedMessageStore>();

builder.Build().Run();
