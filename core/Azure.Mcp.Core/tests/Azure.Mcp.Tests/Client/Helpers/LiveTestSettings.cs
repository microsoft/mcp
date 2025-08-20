// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tests.Client.Helpers;

public class LiveTestSettings
{
    public string PrincipalName { get; set; } = string.Empty;
    public bool IsServicePrincipal { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public string SubscriptionName { get; set; } = string.Empty;
    public string ResourceGroupName { get; set; } = string.Empty;
    public string ResourceBaseName { get; set; } = string.Empty;
    public string SettingsDirectory { get; set; } = string.Empty;
    public string TestPackage { get; set; } = string.Empty;
    public bool DebugOutput { get; set; }
}
