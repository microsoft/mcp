// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:words slis

using System.ClientModel.Primitives;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

internal static class DrillConfiguration
{
    public static DrillProperties Create(DrillKind kind, string? goal, string? healthModel, string? sli,
        string? chaos = null, string? recoveryPlan = null, string? monitoring = null)
    {
        DrillProperties properties = kind switch
        {
            DrillKind.Zonal => new ZonalDrillProperties(),
            DrillKind.Regional => new RegionalDrillProperties(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        if (goal is not null)
        {
            properties.GoalAssignmentProperties = Read<GoalAssignmentPropertiesOfDrill>(goal, "goal-assignment-properties", null);
        }
        if (healthModel is not null)
        {
            properties.HealthModelMonitoringProperties = Read<HealthModelMonitoringProperties>(healthModel, "health-model-monitoring-properties", "healthModelId");
        }
        if (sli is not null)
        {
            properties.SliMonitoringProperties = Read<SliMonitoringProperties>(sli, "sli-monitoring-properties", "slis");
        }
        if (chaos is not null)
        {
            properties.ChaosResourceProperties = ReadIdentities<ChaosResourcePropertiesOfDrill>(chaos, ChaosResourceOption, ChaosIdentityProperties);
        }
        if (recoveryPlan is not null)
        {
            properties.RecoveryPlanProperties = ReadIdentities<RecoveryPlanPropertiesOfDrill>(recoveryPlan, RecoveryPlanOption, IdentityOnly);
        }
        if (monitoring is not null)
        {
            properties.MonitoringProperties = ReadIdentities<MonitoringPropertiesOfDrill>(monitoring, MonitoringOption, IdentityOnly);
        }
        return properties;
    }

    public static DrillUpdateProperties Update(string? goal, string? healthModel, string? sli,
        string? chaos = null, string? recoveryPlan = null, string? monitoring = null)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            WriteUpdate(writer, "goalAssignmentProperties", goal, "goal-assignment-properties", null);
            WriteUpdate(writer, "healthModelMonitoringProperties", healthModel, "health-model-monitoring-properties", "healthModelId");
            WriteUpdate(writer, "sliMonitoringProperties", sli, "sli-monitoring-properties", "slis");
            WriteIdentitiesUpdate(writer, "chaosResourceProperties", chaos, ChaosResourceOption, ChaosIdentityProperties);
            WriteIdentitiesUpdate(writer, "recoveryPlanProperties", recoveryPlan, RecoveryPlanOption, IdentityOnly);
            WriteIdentitiesUpdate(writer, "monitoringProperties", monitoring, MonitoringOption, IdentityOnly);
            writer.WriteEndObject();
        }
        using JsonDocument document = JsonDocument.Parse(stream.ToArray());
        return new DrillConfigurationPatch(document.RootElement);
    }

    private const string ChaosResourceOption = "chaos-resource-properties";
    private const string RecoveryPlanOption = "recovery-plan-properties";
    private const string MonitoringOption = "monitoring-properties";
    private static readonly string[] ChaosIdentityProperties = ["identity", "chaosResourceIdentityForFaults"];
    private static readonly string[] IdentityOnly = ["identity"];

    /// <summary>
    /// Parses the drill's top-level managed identity: {"type": "...", "userAssignedIdentities": ["&lt;ARM ID&gt;"]}.
    /// </summary>
    public static ManagedServiceIdentity? ParseManagedIdentity(string? json, string option = "identity")
    {
        if (json is null)
        {
            return null;
        }
        JsonElement root = ParseJson(json, option);
        ValidateObject(root, option, ["type", "userAssignedIdentities"]);
        if (!root.TryGetProperty("type", out JsonElement typeElement))
        {
            throw new ArgumentException($"--{option} requires type.");
        }
        string type = RequiredString(typeElement, option);
        if (type is not ("None" or "SystemAssigned" or "UserAssigned" or "SystemAssigned, UserAssigned"))
        {
            throw new ArgumentException($"--{option} type must be None, SystemAssigned, UserAssigned, or SystemAssigned, UserAssigned.");
        }
        var ids = new List<string>();
        if (root.TryGetProperty("userAssignedIdentities", out JsonElement idsElement))
        {
            if (idsElement.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException($"--{option} userAssignedIdentities must be a JSON array of ARM ID strings.");
            }
            foreach (JsonElement id in idsElement.EnumerateArray())
            {
                ValidateResourceId(id, "Microsoft.ManagedIdentity/userAssignedIdentities", option);
                if (ids.Contains(id.GetString()!, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"--{option} userAssignedIdentities must not contain duplicates.");
                }
                ids.Add(id.GetString()!);
            }
        }
        bool userAssigned = type.Contains("UserAssigned", StringComparison.Ordinal);
        if (userAssigned != (ids.Count > 0))
        {
            throw new ArgumentException($"--{option} requires userAssignedIdentities exactly when type includes UserAssigned.");
        }
        var identity = new ManagedServiceIdentity(new ManagedServiceIdentityType(type));
        foreach (string id in ids)
        {
            identity.UserAssignedIdentities[new ResourceIdentifier(id)] = new UserAssignedIdentity();
        }
        return identity;
    }

    private static T ReadIdentities<T>(string json, string option, string[] identityProperties) where T : class
    {
        JsonElement value = ParseIdentities(json, option, identityProperties, update: false);
        return ModelReaderWriter.Read<T>(BinaryData.FromString(value.GetRawText()), ModelReaderWriterOptions.Json,
            AzureResourceManagerResilienceManagementContext.Default)
            ?? throw new ArgumentException($"--{option} could not be parsed.");
    }

    private static void WriteIdentitiesUpdate(Utf8JsonWriter writer, string property, string? json, string option, string[] identityProperties)
    {
        if (json is null)
        {
            return;
        }
        JsonElement value = ParseIdentities(json, option, identityProperties, update: true);
        writer.WritePropertyName(property);
        value.WriteTo(writer);
    }

    // Validates an object whose writable members are associated identities, e.g. {"identity": {...}}.
    private static JsonElement ParseIdentities(string json, string option, string[] identityProperties, bool update)
    {
        JsonElement root = ParseJson(json, option);
        ValidateObject(root, option, identityProperties);
        bool any = false;
        foreach (string name in identityProperties)
        {
            if (root.TryGetProperty(name, out JsonElement identity))
            {
                ValidateIdentity(identity, option, update);
                any = true;
            }
            else if (!update)
            {
                throw new ArgumentException($"--{option} requires {name}.");
            }
        }
        if (!any)
        {
            throw new ArgumentException($"--{option} must contain at least one of: {string.Join(", ", identityProperties)}.");
        }
        return root;
    }

    private static JsonElement ParseJson(string json, string option)
    {
        if (Encoding.UTF8.GetByteCount(json) > 1024 * 1024)
        {
            throw new ArgumentException($"--{option} must not exceed 1 MB.");
        }
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            throw new ArgumentException($"--{option} must be valid JSON.");
        }
    }

    private static void WriteUpdate(Utf8JsonWriter writer, string property, string? json, string option, string? selection)
    {
        if (json is null)
        {
            return;
        }
        JsonElement value = Parse(json, option, selection, true);
        writer.WritePropertyName(property);
        value.WriteTo(writer);
    }

    private static T Read<T>(string json, string option, string? selection) where T : class
    {
        JsonElement value = Parse(json, option, selection, false);
        return ModelReaderWriter.Read<T>(BinaryData.FromString(value.GetRawText()), ModelReaderWriterOptions.Json,
            AzureResourceManagerResilienceManagementContext.Default)
            ?? throw new ArgumentException($"--{option} could not be parsed.");
    }

    private static JsonElement Parse(string json, string option, string? selection, bool update)
    {
        if (Encoding.UTF8.GetByteCount(json) > 1024 * 1024)
        {
            throw new ArgumentException($"--{option} must not exceed 1 MB.");
        }
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement root = document.RootElement;
            if (update && selection is not null && root.ValueKind == JsonValueKind.Null)
            {
                return root.Clone();
            }
            ValidateObject(root, option, selection is null ? ["identity"] : ["identity", selection]);
            if (root.TryGetProperty("identity", out JsonElement identity))
            {
                ValidateIdentity(identity, option, update);
            }
            else if (!update)
            {
                throw new ArgumentException($"--{option} requires identity.");
            }
            if (selection is not null)
            {
                if (root.TryGetProperty(selection, out JsonElement value))
                {
                    if (selection == "healthModelId")
                    {
                        ValidateResourceId(value, "Microsoft.CloudHealth/healthmodels", option);
                    }
                    else
                    {
                        ValidateSlis(value, option);
                    }
                }
                else if (!update)
                {
                    throw new ArgumentException($"--{option} requires {selection}.");
                }
            }
            return root.Clone();
        }
        catch (JsonException)
        {
            throw new ArgumentException($"--{option} must be valid JSON.");
        }
    }

    private static void ValidateObject(JsonElement value, string option, string[] allowed)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException($"--{option} requires a JSON object.");
        }
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in value.EnumerateObject())
        {
            if (!allowed.Contains(property.Name, StringComparer.Ordinal) || !names.Add(property.Name))
            {
                throw new ArgumentException($"--{option} contains unsupported, read-only, or duplicate properties.");
            }
        }
    }

    private static void ValidateIdentity(JsonElement identity, string option, bool update)
    {
        ValidateObject(identity, option, ["type", "userAssignedIdentity"]);
        string? type = null;
        if (identity.TryGetProperty("type", out JsonElement typeElement))
        {
            type = RequiredString(typeElement, option);
            if (type is not ("None" or "SystemAssigned" or "UserAssigned" or "SystemAssigned, UserAssigned"))
            {
                throw new ArgumentException($"--{option} identity type must be None, SystemAssigned, UserAssigned, or SystemAssigned, UserAssigned.");
            }
        }
        else if (!update)
        {
            throw new ArgumentException($"--{option} identity requires type.");
        }
        if (identity.TryGetProperty("userAssignedIdentity", out JsonElement id))
        {
            if (type is not null && !type.Contains("UserAssigned", StringComparison.Ordinal))
            {
                throw new ArgumentException($"--{option} userAssignedIdentity is allowed only when identity type includes UserAssigned.");
            }
            ValidateResourceId(id, "Microsoft.ManagedIdentity/userAssignedIdentities", option);
        }
        else if (!update && type?.Contains("UserAssigned", StringComparison.Ordinal) == true)
        {
            throw new ArgumentException($"--{option} requires userAssignedIdentity for a user-assigned identity.");
        }
    }

    private static void ValidateSlis(JsonElement slis, string option)
    {
        if (slis.ValueKind != JsonValueKind.Array || slis.GetArrayLength() is < 1 or > 2)
        {
            throw new ArgumentException($"--{option} requires one or two SLIs; use JSON null on update to clear the selection.");
        }
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var types = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonElement sli in slis.EnumerateArray())
        {
            ValidateObject(sli, option, ["sliId", "type"]);
            if (!sli.TryGetProperty("sliId", out JsonElement id) || !sli.TryGetProperty("type", out JsonElement typeElement))
            {
                throw new ArgumentException($"--{option} requires sliId and type for each SLI.");
            }
            ValidateResourceId(id, "Microsoft.Monitor/slis", option);
            string type = RequiredString(typeElement, option);
            if (type is not ("Availability" or "Latency") || !ids.Add(id.GetString()!) || !types.Add(type))
            {
                throw new ArgumentException($"--{option} allows at most one Availability and one Latency SLI, with no duplicate IDs.");
            }
        }
    }

    private static string RequiredString(JsonElement value, string option)
    {
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw new ArgumentException($"--{option} requires non-empty string values.");
        }
        return value.GetString()!;
    }

    private static void ValidateResourceId(JsonElement value, string resourceType, string option)
    {
        string id = RequiredString(value, option);
        try
        {
            if (!id.StartsWith('/') || new ResourceIdentifier(id).ResourceType != new ResourceType(resourceType))
            {
                throw new ArgumentException();
            }
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            throw new ArgumentException($"--{option} requires a full {resourceType} ARM resource ID.");
        }
    }
}
