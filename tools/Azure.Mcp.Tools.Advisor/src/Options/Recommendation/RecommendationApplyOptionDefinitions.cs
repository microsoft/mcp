namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public static class RecommendationApplyOptionDefinitions
{
    public const string ResourceType = "resource";

    public static readonly Option<string> Resource = new(
        $"--{ResourceType}"
    )
    {
        Description = "The Azure resource type for which to get rules to apply to IaaC file or fetch recommendations for an architecture diagram. Available options: 'accounts', 'afdendpoints', 'applicationgateways', 'applicationgatewaywebapplicationfirewallpolicies', 'caches', 'certificates', 'cloudexadatainfrastructures', 'cloudservices', 'cloudvmclusters', 'clusters', 'components', 'configurationprofiles', 'configurationstores', 'connectedclusters', 'databaseaccounts', 'disks', 'domains', 'domainservices', 'expressroute', 'expressroutecircuits', 'expressrouteports', 'extensions', 'fhirservices', 'flexibleservers', 'flowlogs', 'frontdoors', 'frontdoorwebapplicationfirewallpolicies', 'hostpools', 'imagetemplates', 'iothubs', 'kubeenvironments', 'kustopools', 'labaccounts', 'labplans', 'labs', 'loadbalancers', 'managedclusters', 'managedinstances', 'metricalerts', 'namespaces', 'networkinterfaces', 'networkwatchers', 'partnernamespaces', 'privateendpoints', 'profiles', 'publicipaddresses', 'redis', 'redisenterprise', 'registries', 'replicationprotecteditems', 'serverfarms', 'servers', 'service', 'services', 'signalr', 'sites', 'solutions', 'spring', 'staticsites', 'storageaccounts', 'streamingjobs', 'topics', 'trafficmanagerprofiles', 'vaults', 'versions', 'virtualmachines', 'virtualmachinescalesets', 'virtualnetworkgateways', 'volumes', 'webtests', 'workspaces'",
        Required = true
    };
}
