namespace Azure.Mcp.Tools.Deploy.Models;

public static class DeployOption
{
    public const string ProvisionAndDeploy = "provision-and-deploy";
    public const string DeployOnly = "deploy-only";
    public const string ProvisionOnly = "provision-only";
}

public static class PipelinePlatform
{
    public const string GitHubActions = "github-actions";
    public const string AzureDevOps = "azure-devops";
}
