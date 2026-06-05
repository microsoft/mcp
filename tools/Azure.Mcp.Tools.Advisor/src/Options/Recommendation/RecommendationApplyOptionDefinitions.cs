namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public static class RecommendationApplyOptionDefinitions
{
    public const string ResourceType = "resource";

    public static readonly Option<string> Resource = new(
        $"--{ResourceType}"
    )
    {
        Description = "The Azure resource type for which to get rules to apply to IaaC file. Available options: 'aad_domainservices', 'apimanagement_service', 'cognitiveservices_accounts', 'compute_virtualmachines', 'compute_virtualmachinescalesets', 'containerregistry_registries', 'containerservice_managedclusters', 'dbforpostgresql_flexibleservers', 'documentdb_databaseaccounts', 'keyvault_vaults', 'kubernetes_connectedclusters', 'kubernetesconfiguration_extensions', 'netapp_volumes', 'network_applicationgatewaywebapplicationfirewallpolicies', 'network_expressrouteports', 'network_frontdoorwebapplicationfirewallpolicies', 'sql_managedinstances', 'storage_storageaccounts', 'web_serverfarms', 'web_staticsites'",
        Required = true
    };
}
