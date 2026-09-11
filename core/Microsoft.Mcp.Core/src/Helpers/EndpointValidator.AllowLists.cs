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
        ["foundry"] = new AllowedSuffixManager(
            Public: [".services.ai.azure.com"],
            China: [".services.ai.azure.cn"],
            UsGov: [".services.ai.azure.us"],
            Germany: [".services.ai.azure.de"],
            UseLegacyCheck: true), // INITIAL SEEDED UseLegacyCheck. NEEDS VERIFICATION.
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
