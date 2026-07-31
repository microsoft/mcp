// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Deploy.Options.Pipeline;

public sealed class GuidanceGetOptions
{
    [Option(Description = "Whether to use azd tool in the deployment pipeline. Set to true ONLY if azure.yaml is provided or the context suggests AZD tools.")]
    public bool IsAzdProject { get; set; }

    [Option(Description = "The platform for the deployment pipeline. Valid values: github-actions, azure-devops.", DefaultValue = "github-actions")]
    public string? PipelinePlatform { get; set; }

    [Option(Description = "Valid values: deploy-only, provision-and-deploy. Default to deploy-only. Set to 'provision-and-deploy' ONLY WHEN user explicitly wants infra provisioning pipeline using local provisioning scripts.", DefaultValue = "deploy-only")]
    public string? DeployOption { get; set; }
}
