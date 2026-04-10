// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public static class AzureTerraformTelemetryTags
{
    private static string AddPrefix(string tagName) => $"azureterraform/{tagName}";

    public static readonly string ToolArea = AddPrefix("ToolArea");
    public static readonly string ResourceType = AddPrefix("ResourceType");
    public static readonly string ModuleName = AddPrefix("ModuleName");
    public static readonly string Provider = AddPrefix("Provider");
    public static readonly string PolicySet = AddPrefix("PolicySet");
}
