// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Frozen;
using Azure.ResourceManager;

namespace Microsoft.Mcp.Core.Helpers;

public static partial class EndpointValidator
{
    private record AllowedSuffixManager(string[] Public, string[] China, string[] UsGov, string[] Germany)
    {
        public string[] GetSuffixes(ArmEnvironment environment) =>
            ArmEnvironment.AzurePublicCloud.Equals(environment) ? Public :
            ArmEnvironment.AzureChina.Equals(environment) ? China :
            ArmEnvironment.AzureGovernment.Equals(environment) ? UsGov :
            ArmEnvironment.AzureGermany.Equals(environment) ? Germany :
            Public;
    }

    private static readonly FrozenDictionary<string, AllowedSuffixManager> s_allowedDomainSuffixes = new Dictionary<string, AllowedSuffixManager>
    {
        ["acr"] = new AllowedSuffixManager(
            Public: [".azurecr.io"],
            China: [".azurecr.cn"],
            UsGov: [".azurecr.us"],
            Germany: [".azurecr.de"]),
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
            ]),
        ["appconfig"] = new AllowedSuffixManager(
            Public: [".azconfig.io"],
            China: [".azconfig.azure.cn"],
            UsGov: [".azconfig.azure.us"],
            Germany: [".azconfig.azure.de"]),
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
            ]),
        ["communication"] = new AllowedSuffixManager(
            Public: [".communication.azure.com"],
            China: [".communication.azure.cn"],
            UsGov: [".communication.azure.us"],
            Germany: [".communication.azure.de"]),
        ["foundry"] = new AllowedSuffixManager(
            Public: [".services.ai.azure.com"],
            China: [".services.ai.azure.cn"],
            UsGov: [".services.ai.azure.us"],
            Germany: [".services.ai.azure.de"]),
        ["servicebus"] = new AllowedSuffixManager(
            Public: [".servicebus.windows.net"],
            China: [".servicebus.chinacloudapi.cn"],
            UsGov: [".servicebus.usgovcloudapi.net"],
            Germany: [".servicebus.cloudapi.de"]),
        ["storage-blob"] = new AllowedSuffixManager(
            Public: [".blob.core.windows.net"],
            China: [".blob.core.chinacloudapi.cn"],
            UsGov: [".blob.core.usgovcloudapi.net"],
            Germany: [".blob.core.cloudapi.de"]),
    }.ToFrozenDictionary();
}
