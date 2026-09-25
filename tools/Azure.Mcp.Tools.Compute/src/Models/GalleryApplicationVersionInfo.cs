// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record GalleryApplicationVersionInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("sourceMediaLink")] string? SourceMediaLink,
    [property: JsonPropertyName("excludeFromLatest")] bool? ExcludeFromLatest,
    [property: JsonPropertyName("endOfLifeOn")] string? EndOfLifeOn,
    [property: JsonPropertyName("targetRegions")] IReadOnlyList<string>? TargetRegions,
    [property: JsonPropertyName("tags")] IReadOnlyDictionary<string, string>? Tags,
    [property: JsonPropertyName("defaultConfigurationLink")] string? DefaultConfigurationLink = null,
    [property: JsonPropertyName("replicaCount")] int? ReplicaCount = null,
    [property: JsonPropertyName("publishedOn")] string? PublishedOn = null,
    [property: JsonPropertyName("storageAccountType")] string? StorageAccountType = null,
    [property: JsonPropertyName("replicationMode")] string? ReplicationMode = null,
    [property: JsonPropertyName("targetExtendedLocations")] IReadOnlyList<string>? TargetExtendedLocations = null,
    [property: JsonPropertyName("manageActionInstall")] string? ManageActionInstall = null,
    [property: JsonPropertyName("manageActionRemove")] string? ManageActionRemove = null,
    [property: JsonPropertyName("manageActionUpdate")] string? ManageActionUpdate = null,
    [property: JsonPropertyName("packageFileName")] string? PackageFileName = null,
    [property: JsonPropertyName("configFileName")] string? ConfigFileName = null,
    [property: JsonPropertyName("scriptBehaviorAfterReboot")] string? ScriptBehaviorAfterReboot = null,
    [property: JsonPropertyName("advancedSettings")] IReadOnlyDictionary<string, string>? AdvancedSettings = null,
    [property: JsonPropertyName("enableHealthCheck")] bool? EnableHealthCheck = null,
    [property: JsonPropertyName("customActionNames")] IReadOnlyList<string>? CustomActionNames = null);
