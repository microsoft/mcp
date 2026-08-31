// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Commands.HealthCheck;
using Azure.Mcp.Tools.Adme.Commands.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddHttpClient(AdmeServiceHelper.HttpClientName);
        services.AddSingleton<IHealthService, HealthService>();
        services.AddSingleton<ISchemaService, SchemaService>();
        services.AddSingleton<HealthCheckCommand>();
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
                + "OSDU schema discovery.",
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

        return adme;
    }
}
