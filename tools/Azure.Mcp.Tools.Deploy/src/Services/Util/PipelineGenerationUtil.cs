// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Models.Templates;
using Azure.Mcp.Tools.Deploy.Options.Pipeline;
using Azure.Mcp.Tools.Deploy.Services.Templates;

namespace Azure.Mcp.Tools.Deploy.Services.Util;

/// <summary>
/// Utility class for generating pipeline guidelines using embedded template resources.
/// </summary>
public static class PipelineGenerationUtil
{
    /// <summary>
    /// Generates pipeline guidelines based on the provided options.
    /// </summary>
    /// <param name="options">The guidance options containing pipeline configuration.</param>
    /// <returns>A formatted pipeline guidelines string.</returns>
    public static string GeneratePipelineGuidelines(GuidanceGetOptions options)
    {
        var parameters = CreatePipelineParameters(options);
        return TemplateService.ProcessTemplate("Pipeline/azcli-pipeline", parameters.ToDictionary());
    }

    /// <summary>
    /// Creates pipeline template parameters from the provided options.
    /// </summary>
    private static PipelineTemplateParameters CreatePipelineParameters(GuidanceGetOptions options)
    {
        const string defaultEnvironment = "dev";
        var environmentNamePrompt = !string.IsNullOrEmpty(options.GithubEnvironmentName)
            ? $"Use {options.GithubEnvironmentName} for environment name of the deployment job."
            : $"Use '{defaultEnvironment}' for the $environment for the deployment job.";

        var subscriptionIdPrompt = !string.IsNullOrEmpty(options.Subscription) && CheckGUIDFormat(options.Subscription)
            ? $"User is deploying to subscription {options.Subscription}"
            : "Use \"az account show --query id -o tsv\" as default subscription ID.";

        var organizationName = !string.IsNullOrEmpty(options.OrganizationName) ? options.OrganizationName : "{$organization-of-repo}";
        var repositoryName = !string.IsNullOrEmpty(options.RepositoryName) ? options.RepositoryName : "{$repository-name}";
        var environmentName = !string.IsNullOrEmpty(options.GithubEnvironmentName) ? options.GithubEnvironmentName : defaultEnvironment;

        var subjectConfig = $"repo:{organizationName}/{repositoryName}:environment:{environmentName}";
        var environmentArg = !string.IsNullOrEmpty(options.GithubEnvironmentName) ? $"--env {options.GithubEnvironmentName}" : "--env dev";
        var environmentCreateCommand = $"gh api --method PUT -H \"Accept: application/vnd.github+json\" repos/{organizationName}/{repositoryName}/environments/{environmentName}";
        var jsonParameters = $"{{\"name\":\"github-federated\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"{subjectConfig}\",\"audiences\":[\"api://AzureADTokenExchange\"]}}";

        return new PipelineTemplateParameters
        {
            EnvironmentNamePrompt = environmentNamePrompt,
            SubscriptionIdPrompt = subscriptionIdPrompt,
            EnvironmentCreateCommand = environmentCreateCommand,
            JsonParameters = jsonParameters,
            EnvironmentArg = environmentArg
        };
    }

    /// <summary>
    /// Checks if the provided string is a valid GUID format.
    /// </summary>
    private static bool CheckGUIDFormat(string input)
    {
        return Guid.TryParse(input, out _);
    }
}
