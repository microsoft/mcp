// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Options.Webapp.Deployment;

public class DeploymentGetOptions : BaseAppServiceOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.DeploymentIdName)]
    public string? DeploymentId { get; set; }
}
