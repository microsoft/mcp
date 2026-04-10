namespace Azure.Mcp.Tools.Deploy.Models;


public static class DeploymentTool
{
    public const string Azd = "AZD";
    public const string AzCli = "AzCli";
}

public static class SourceType
{
    public const string FromAzure = "from-azure";
    public const string FromProject = "from-project";
    public const string FromContext = "from-context";
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
    public const string AzureKubernetesService = "aks";
    public const string AzureDatabaseForPostgreSql = "azuredatabaseforpostgresql";
    public const string AzureDatabaseForMySql = "azuredatabaseformysql";
    public const string AzureSqlDatabase = "azuresqldatabase";
    public const string AzureCosmosDb = "azurecosmosdb";
    public const string AzureStorageAccount = "azurestorageaccount";
    public const string AzureKeyVault = "azurekeyvault";
}
