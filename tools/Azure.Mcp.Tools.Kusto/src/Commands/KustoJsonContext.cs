// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Azure.ResourceManager.Kusto;

namespace Azure.Mcp.Tools.Kusto.Commands;

[JsonSerializable(typeof(ClusterListCommand.ClusterListCommandResult))]
[JsonSerializable(typeof(ClusterGetCommand.ClusterGetCommandResult))]
[JsonSerializable(typeof(DatabaseListCommand.DatabaseListCommandResult))]
[JsonSerializable(typeof(TableListCommand.TableListCommandResult))]
[JsonSerializable(typeof(TableSchemaCommand.TableSchemaCommandResult))]
[JsonSerializable(typeof(QueryCommand.QueryCommandResult))]
[JsonSerializable(typeof(SampleCommand.SampleCommandResult))]
[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(List<KustoClusterResourceProxy>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class KustoJsonContext : JsonSerializerContext
{
}

public sealed record KustoClusterResourceProxy()
{
    public required string ClusterUri { get; set; }
    public required string ClusterName { get; set; }
    public required string Location { get; set; }
    public required string ResourceGroupName { get; set; }
    public required string SubscriptionId { get; set; }
    public required string Sku { get; set; }
    public required string Zones { get; set; }
    public required string Identity { get; set; }
    public required string ETag { get; set; }
    public required string State { get; set; }
    public required string ProvisioningState { get; set; }
    public required string DataIngestionUri { get; set; }
    public required string StateReason { get; set; }
    public required bool IsStreamingIngestEnabled { get; set; }
    public required string EngineType { get; set; }
    public required bool IsAutoStopEnabled { get; set; }

    [SetsRequiredMembers]
    public KustoClusterResourceProxy(KustoClusterResource kustoClusterResource) : this()
    {
        ClusterName = kustoClusterResource.Data.Name;
        Location = kustoClusterResource.Data.Location.ToString();
        ResourceGroupName = kustoClusterResource.Id.ResourceGroupName ?? string.Empty;
        SubscriptionId = kustoClusterResource.Id.SubscriptionId ?? string.Empty;
        Sku = kustoClusterResource.Data.Sku.Capacity.ToString() ?? string.Empty;
        Zones = string.Join(",", kustoClusterResource.Data.Zones.ToList()) ?? string.Empty;
        Identity = kustoClusterResource.Data.Identity?.ManagedServiceIdentityType.ToString() ?? string.Empty;
        ETag = kustoClusterResource.Data.ETag?.ToString() ?? string.Empty;
        State = kustoClusterResource.Data.State?.ToString() ?? string.Empty;
        ProvisioningState = kustoClusterResource.Data.ProvisioningState?.ToString() ?? string.Empty;
        ClusterUri = kustoClusterResource.Data.ClusterUri?.ToString() ?? string.Empty;
        DataIngestionUri = kustoClusterResource.Data.DataIngestionUri?.ToString() ?? string.Empty;
        StateReason = kustoClusterResource.Data.StateReason ?? string.Empty;
        IsStreamingIngestEnabled = kustoClusterResource.Data.IsStreamingIngestEnabled ?? false;
        EngineType = kustoClusterResource.Data.EngineType?.ToString() ?? string.Empty;
        IsAutoStopEnabled = kustoClusterResource.Data.IsAutoStopEnabled ?? false;
    }
}
