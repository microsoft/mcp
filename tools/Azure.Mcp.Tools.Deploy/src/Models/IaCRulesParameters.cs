namespace Azure.Mcp.Tools.Deploy.Models;


public static class DeploymentTool
{
    public const string Azd = "AZD";
    public const string AzCli = "AzCli";
}

public static class IacType
{
    public const string Bicep = "bicep";
    public const string Terraform = "terraform";
}

public static class AzureServiceNames
{
    public const string AzureContainerApp = "containerapp";
    public const string AzureAppService = "appservice";
    public const string AzureFunctionApp = "function";
    public const string AzureStorage = "storage";
}
