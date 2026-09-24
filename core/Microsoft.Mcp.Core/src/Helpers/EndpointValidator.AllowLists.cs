// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Frozen;
using Azure.ResourceManager;

namespace Microsoft.Mcp.Core.Helpers;

public static partial class EndpointValidator
{
    /// <summary>
    /// A list of allow-list hostname suffixes for some service. See <paramref name="UseLegacyCheck"/>
    /// for instructions on how to match a use case to how to populate the allow-lists
    /// and opt-out of new comparison code.
    /// </summary>
    /// <param name="Public">The public Azure cloud allow-list of subdomains.</param>
    /// <param name="China">The China Azure cloud allow-list of subdomains.</param>
    /// <param name="UsGov">The US Gov Azure cloud allow-list of subdomains.</param>
    /// <param name="Germany">The Germany Azure cloud allow-list of subdomains.</param>
    /// <param name="UseLegacyCheck">
    /// <para>
    ///   A value indicating whether to use the legacy (<see langword="true"/>) or new
    ///   (<see langword="false"/>) comparison of a URL against the allow-lists.
    /// </para>
    /// <para>
    ///   When using legacy (<see langword="true"/>):
    ///   <list type="bullet">
    ///     <item>".contoso.com" with a leading '.' allows both "contoso.com" and "sub.contoso.com"</item>
    ///     <item>"contoso.com" without a leading '.' ONLY "contoso.com" -- DIFFERENT</item>
    ///   </list>
    /// </para>
    /// <para>
    ///   When using new (<see langword="false"/>):
    ///   <list type="bullet">
    ///     <item>".contoso.com" with a leading '.' allows both "contoso.com" and "sub.contoso.com"</item>
    ///     <item>"contoso.com" without a leading '.' allows both "contoso.com" and "sub.contoso.com" -- DIFFERENT</item>
    ///   </list>
    /// </para>
    /// <para>
    ///   An eventual goal is to remove this flag once all scenarios are inventoried.
    /// </para>
    /// </param>
    private record AllowedSuffixManager(
        string[] Public,
        string[] China,
        string[] UsGov,
        string[] Germany,
        bool UseLegacyCheck)
    {
        public string[] GetSuffixes(ArmEnvironment environment) =>
            ArmEnvironment.AzurePublicCloud.Equals(environment) ? Public :
            ArmEnvironment.AzureChina.Equals(environment) ? China :
            ArmEnvironment.AzureGovernment.Equals(environment) ? UsGov :
            ArmEnvironment.AzureGermany.Equals(environment) ? Germany :
            Public;
    }

    /// <summary>
    /// <para>
    ///   Allow-list of well-known URIs. A key identifies the service and must be used exactly in code that wants
    ///   to use the allow-list of that service. The <see cref="AllowedSuffixManager"/> value includes per-cloud
    ///   URIs based on ARM/Azure boundaries. For non-Azure services, use <see cref="AllowedSuffixManager.Public"/>
    ///   both here and in code using <see cref="EndpointValidator"/>.
    /// </para>
    /// <para>
    ///   READ THE DETAILS FOR <see cref="AllowedSuffixManager.UseLegacyCheck"/> WHEN CREATING OR MODIFYING
    ///   ALLOW-LISTS.
    /// </para>
    /// <br/>
    /// </summary>
    private static readonly FrozenDictionary<string, AllowedSuffixManager> s_allowedDomainSuffixes = new Dictionary<string, AllowedSuffixManager>
    {
        ["acr"] = new AllowedSuffixManager(
            Public: [".azurecr.io"],
            China: [".azurecr.cn"],
            UsGov: [".azurecr.us"],
            Germany: [".azurecr.de"],
            UseLegacyCheck: false),
        ["adme"] = new AllowedSuffixManager(
            Public: [
                ".energy.azure.com",
                ".oep.ppe.azure-int.net"
            ],
            China: [
                ".energy.azure.com",
                ".oep.ppe.azure-int.net"
            ],
            UsGov: [
                ".energy.azure.com",
                ".oep.ppe.azure-int.net"
            ],
            Germany: [
                ".energy.azure.com",
                ".oep.ppe.azure-int.net"
            ],
            UseLegacyCheck: false),
        ["appconfig"] = new AllowedSuffixManager(
            Public: [".azconfig.io"],
            China: [".azconfig.azure.cn"],
            UsGov: [".azconfig.azure.us"],
            Germany: [".azconfig.azure.de"],
            UseLegacyCheck: true), // INITIAL SEEDED UseLegacyCheck. NEEDS VERIFICATION.
        ["arm"] = new AllowedSuffixManager(
            // Raw ARM requests in Quota and Resource Health use ArmEnvironment.Endpoint to construct these hosts.
            Public: ["management.azure.com"],
            China: ["management.chinacloudapi.cn"],
            UsGov: ["management.usgovcloudapi.net"],
            Germany: [],
            UseLegacyCheck: true),
        ["azure-openai"] = new AllowedSuffixManager(
            Public: [
                ".openai.azure.com",
                ".cognitiveservices.azure.com"
            ],
            China: [
                ".openai.azure.cn",
                ".cognitiveservices.azure.cn"
            ],
            UsGov: [
                ".openai.azure.us",
                ".cognitiveservices.azure.us"
            ],
            Germany: [
                ".openai.azure.de",
                ".cognitiveservices.azure.de"
            ],
            UseLegacyCheck: true), // INITIAL SEEDED UseLegacyCheck. NEEDS VERIFICATION.
        ["communication"] = new AllowedSuffixManager(
            Public: [".communication.azure.com"],
            China: [".communication.azure.cn"],
            UsGov: [".communication.azure.us"],
            Germany: [".communication.azure.de"],
            UseLegacyCheck: true), // INITIAL SEEDED UseLegacyCheck. NEEDS VERIFICATION.
        ["confidential-ledger"] = new AllowedSuffixManager(
            Public: [".confidential-ledger.azure.com"],
            China: [".confidential-ledger.azure.cn"],
            UsGov: [".confidential-ledger.azure.us"],
            Germany: [], // Confidential Ledger is not offered in the Germany cloud; leave empty so validation fails rather than implicitly accepting a public-cloud host.
            UseLegacyCheck: false),
        ["eventgrid"] = new AllowedSuffixManager(
            Public: [".eventgrid.azure.net"],
            China: [".eventgrid.azure.cn"],
            UsGov: [".eventgrid.azure.us"],
            Germany: [],
            UseLegacyCheck: false),
        ["foundry"] = new AllowedSuffixManager(
            Public: [".services.ai.azure.com"],
            China: [],
            UsGov: [".services.ai.azure.us"],
            Germany: [],
            UseLegacyCheck: false),
        ["iothub"] = new AllowedSuffixManager(
            Public: [".azure-devices.net"],
            China: [".azure-devices.cn"],
            UsGov: [".azure-devices.us"],
            Germany: [],
            UseLegacyCheck: false),
        ["keyvault"] = new AllowedSuffixManager(
            Public: [".vault.azure.net"],
            China: [".vault.azure.cn"],
            UsGov: [".vault.usgovcloudapi.net"],
            Germany: [],
            UseLegacyCheck: false),
        ["loadtesting"] = new AllowedSuffixManager(
            Public: [".loadtesting.azure.com"],
            China: [],
            UsGov: [".loadtesting.azure.us"],
            Germany: [],
            UseLegacyCheck: false),
        ["managedhsm"] = new AllowedSuffixManager(
            Public: [".managedhsm.azure.net"],
            China: [".managedhsm.azure.cn"],
            UsGov: [".managedhsm.usgovcloudapi.net"],
            Germany: [],
            UseLegacyCheck: false),
        ["monitor-metrics"] = new AllowedSuffixManager(
            Public: [".metrics.monitor.azure.com"],
            China: [".metrics.monitor.azure.cn"],
            UsGov: [".metrics.monitor.azure.us"],
            Germany: [],
            UseLegacyCheck: false),
        ["mysql"] = new AllowedSuffixManager(
            Public: [".mysql.database.azure.com"],
            China: [".mysql.database.chinacloudapi.cn"],
            UsGov: [".mysql.database.usgovcloudapi.net"],
            Germany: [],
            UseLegacyCheck: false),
        ["postgres"] = new AllowedSuffixManager(
            // PostgresService.cs also has a copy of these. Keep them in sync.
            Public: [".postgres.database.azure.com"],
            China: [".postgres.database.chinacloudapi.cn"],
            UsGov: [".postgres.database.usgovcloudapi.net"],
            Germany: [],
            UseLegacyCheck: false),
        ["pricing"] = new AllowedSuffixManager(
            // PricingService.GetPricingEndpoint constructs these exact hosts, while the endpoint policy uses
            // this copy to authorize initial and paginated requests. Keep both copies and their tests in sync.
            Public: ["prices.azure.com"],
            China: ["prices.azure.cn"],
            UsGov: ["prices.azure.us"],
            Germany: [],
            UseLegacyCheck: true),
        ["servicebus"] = new AllowedSuffixManager(
            Public: [".servicebus.windows.net"],
            China: [".servicebus.chinacloudapi.cn"],
            UsGov: [".servicebus.usgovcloudapi.net"],
            Germany: [".servicebus.cloudapi.de"],
            UseLegacyCheck: true), // INITIAL SEEDED UseLegacyCheck. NEEDS VERIFICATION.
        ["storage-blob"] = new AllowedSuffixManager(
            Public: [".blob.core.windows.net"],
            China: [".blob.core.chinacloudapi.cn"],
            UsGov: [".blob.core.usgovcloudapi.net"],
            Germany: [".blob.core.cloudapi.de"],
            UseLegacyCheck: true), // INITIAL SEEDED UseLegacyCheck. NEEDS VERIFICATION.
    }.ToFrozenDictionary();
}
