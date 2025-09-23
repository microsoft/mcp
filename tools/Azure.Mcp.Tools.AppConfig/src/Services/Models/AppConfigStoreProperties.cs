// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppConfig.Services.Models;

/// <summary>
/// A class representing the AppConfig store properties model.
/// </summary>
internal sealed class AppConfigStoreProperties
{
    /// <summary> The provisioning state of the configuration store. </summary>
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }
    /// <summary> The creation date of configuration store. </summary>
    [JsonPropertyName("creationDate")]
    public DateTimeOffset? CreatedOn { get; set; }
    /// <summary> The DNS endpoint where the configuration store API will be available. </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }
    /// <summary> Key vault properties. </summary>
    [JsonPropertyName("encryption")]
    public AppConfigStoreEncryptionProperties? EncryptionProperties { get; set; }

    /// <summary> Control permission for data plane traffic coming from public networks while private endpoint is enabled. </summary>
    [JsonPropertyName("publicNetworkAccess")]
    public string? PublicNetworkAccess { get; set; }
    /// <summary> Disables all authentication methods other than AAD authentication. </summary>
    [JsonPropertyName("disableLocalAuth")]
    public bool? DisableLocalAuth { get; set; }
    /// <summary> The amount of time in days that the configuration store will be retained when it is soft deleted. </summary>
    [JsonPropertyName("softDeleteRetentionInDays")]
    public int? SoftDeleteRetentionInDays { get; set; }
    /// <summary> Property specifying whether protection against purge is enabled for this configuration store. </summary>
    [JsonPropertyName("enablePurgeProtection")]
    public bool? EnablePurgeProtection { get; set; }
    /// <summary> Indicates whether the configuration store need to be recovered. </summary>
    [JsonPropertyName("createMode")]
    public string? CreateMode { get; set; }
}