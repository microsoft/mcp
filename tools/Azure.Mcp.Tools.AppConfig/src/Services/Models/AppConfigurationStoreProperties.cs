// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppConfig.Services.Models;

/// <summary>
/// A class representing the AppConfigurationStore properties data model.
/// </summary>
internal sealed class AppConfigurationStoreProperties
{
    /// <summary> The provisioning state of the configuration store. </summary>
    public string? ProvisioningState { get; set; }
    /// <summary> The creation date of configuration store. </summary>
    [JsonPropertyName("creationDate")]
    public DateTimeOffset? CreatedOn { get; set; }
    /// <summary> The DNS endpoint where the configuration store API will be available. </summary>
    public string? Endpoint { get; set; }
    /// <summary> The encryption settings of the configuration store. </summary>
    public AppConfigurationStoreEncryptionProperties? Encryption { get; set; }
    /// <summary> Control permission for data plane traffic coming from public networks while private endpoint is enabled. </summary>
    public string? PublicNetworkAccess { get; set; }
    /// <summary> Disables all authentication methods other than AAD authentication. </summary>
    public bool? DisableLocalAuth { get; set; }
    /// <summary> The amount of time in days that the configuration store will be retained when it is soft deleted. </summary>
    public int? SoftDeleteRetentionInDays { get; set; }
    /// <summary> Property specifying whether protection against purge is enabled for this configuration store. </summary>
    public bool? EnablePurgeProtection { get; set; }
    /// <summary> Indicates whether the configuration store need to be recovered. </summary>
    public string? CreateMode { get; set; }
}
