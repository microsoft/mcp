// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppLens.Models;

public enum AppLensResourceType
{
    [JsonStringEnumMemberName("microsoft.web/sites")]
    AppService,

    [JsonStringEnumMemberName("microsoft.containerservice/managedclusters")]
    Aks,

    [JsonStringEnumMemberName("microsoft.apimanagement/service")]
    ApiManagement
}
