// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Adme.Commands.HealthCheck;
using Azure.Mcp.Tools.Adme.Commands.Schema;
using Azure.Mcp.Tools.Adme.Commands.Storage;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Adme;

/// <summary>
/// Registers Azure Data Manager for Energy commands and services.
/// </summary>
public sealed class AdmeSetup : IAreaSetup
{
    public string Name => "adme";

    public string Title => "Azure Data Manager for Energy";

    /// <summary>
    /// Registers ADME commands and their services.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient(AdmeServiceHelper.HttpClientName)
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Result is not { StatusCode: HttpStatusCode.InternalServerError }
                    && HttpClientResiliencePredicates.IsTransient(args.Outcome));
            });
        services.AddSingleton<IHealthService, HealthService>();
        services.AddSingleton<ISchemaService, SchemaService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<HealthCheckCommand>();
        services.AddSingleton<RecordFetchCommand>();
        services.AddSingleton<RecordGetCommand>();
        services.AddSingleton<RecordListCommand>();
        services.AddSingleton<RecordVersionListCommand>();
        services.AddSingleton<SchemaGetCommand>();
        services.AddSingleton<SchemaListCommand>();
    }

    /// <summary>
    /// Builds the ADME command group.
    /// </summary>
    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var adme = new CommandGroup(
            Name,
            "Azure Data Manager for Energy operations for the OSDU data platform. Commands target a specific "
                + "endpoint and data partition and cover platform health checks and "
                + "OSDU schema discovery, record retrieval, and version history.",
            Title);

        var health = new CommandGroup(
            "health",
            "Verify Microsoft Entra authentication and connectivity to an endpoint and data partition. "
                + "Use these first when other commands fail.");
        health.AddCommand<HealthCheckCommand>(serviceProvider);
        adme.AddSubGroup(health);

        var schema = new CommandGroup(
            "schema",
            "Discover and inspect OSDU schemas (kinds) in a data partition. List enumerates which "
                + "kinds and versions exist; get returns a kind's full field definitions.");
        schema.AddCommand<SchemaGetCommand>(serviceProvider);
        schema.AddCommand<SchemaListCommand>(serviceProvider);
        adme.AddSubGroup(schema);

        var storage = new CommandGroup(
            "storage",
            "Read OSDU records and their version history from a data partition.");
        var record = new CommandGroup(
            "record",
            "Read OSDU records, list record ids, and inspect record versions.");
        record.AddCommand<RecordFetchCommand>(serviceProvider);
        record.AddCommand<RecordGetCommand>(serviceProvider);
        record.AddCommand<RecordListCommand>(serviceProvider);
        var version = new CommandGroup(
            "version",
            "Inspect the version history of an OSDU record.");
        version.AddCommand<RecordVersionListCommand>(serviceProvider);
        record.AddSubGroup(version);
        storage.AddSubGroup(record);
        adme.AddSubGroup(storage);

        return adme;
    }
}
