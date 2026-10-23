// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Frozen;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Azure.Mcp.Tools.Insights.Services.Models;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Insights.Services;

/// <summary>
/// Filters a <see cref="SubscriptionAggregation"/> produced by <see cref="PropertyAggregator"/>:
/// drops noise resource types, denied property keys, low-coverage leaves, and per-instance or
/// secret-shaped values. Also trims each leaf to the top-N most common values.
/// </summary>
internal static class AggregationFilter
{
    internal const int TopValuesPerLeaf = 3;

    internal const double MinTopCoverage = 0.1;

    private const string ArmIdPrefix = "/subscriptions/";

    private static readonly FrozenSet<string> DiscardTypes = new[]
    {
        // No usable Bicep schema
        "microsoft.authorization/provideroperations",
        "microsoft.policyinsights/policyevents",
        "microsoft.policyinsights/policystates",
        "microsoft.resourcehealth/availabilitystatuses",
        "microsoft.security/securescorecontrols",
        "microsoft.security/subassessments",

        // Placeholder schema with only "name"
        "microsoft.advisor/recommendations",
        "microsoft.alertsmanagement/alerts",
        "microsoft.resourcehealth/events",
        "microsoft.security/regulatorycompliancestandards",
        "microsoft.security/securescores",
        "microsoft.support/services",
    }.ToFrozenSet(StringComparer.Ordinal);

    // Leaves whose values are kept only when they look like ARM resource IDs.
    private static readonly FrozenSet<string> RelationalIdKeys = new[]
    {
        "id", "resourceid", "subscriptionid", "resourcegroup",
        "workspaceid", "workspaceresourceid", "serverfarmid",
        "environmentid", "actiongroupid", "connectionid",
        "keyvaultid", "certificateresourceid", "usercertificateresourceid",
        "keyvaultsecretid", "metricresourceid", "targetresourceid",
        "groupids", "scopes", "scope", "source", "storageaccount",
        "subnetid", "vnetid",
    }.ToFrozenSet(StringComparer.Ordinal);

    private const RegexOptions KeyRegexOptions =
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant;

    private const RegexOptions ValueRegexOptions =
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant;

    private static readonly Regex[] KeyDenyPatterns =
    [
        // Runtime / lifecycle state
        RegexHelper.CreateRegex(@"^(provisioning|operational|usage|power|running|runtime|target|replica|disk|maintenance|availability|content)?state$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(scmsitealsostopped|hostnamesdisabled|sitedisabledreason|deploymenterrors|deploymentid|displaystatus|actionsrequired|runtimeavailabilitystate|contentavailabilitystate|storagerecoverydefaultstate|serviceproviderprovisioningstate)$", KeyRegexOptions),

        // Timestamps / dates
        RegexHelper.CreateRegex(@".*(time|date|timestamp|timestamputc)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(starttime|earliestrestoredate|provisioningtime|completedtimestamputc|starttimestamputc|timestamputc)$", KeyRegexOptions),

        // Per-instance hostnames / endpoints / IPs / URLs
        RegexHelper.CreateRegex(@"^(default)?hostname$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(enabled)?hostnames$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(fqdn|portalfqdn|ftpshostname|ftpusername|selflink|webspace|stamp|homestamp|destinationstampname|sourcestampname|repositorysitename|mdmid|metricid|buildversion|contentversion)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(servicebusendpoint|eventstreamendpoint|discoveryurl|azureasyncoperationuri|accounturl|datashareuri)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(inbound|outbound|possibleinbound|possibleoutbound|public)?ip(v6)?address(es)?$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^nameservers$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^icm\..*$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(eligiblelogcategories|sdktelemetryappinsightskey|customdomainverificationid)$", KeyRegexOptions),

        // Opaque handles / GUIDs
        RegexHelper.CreateRegex(@"^(etag|forceupdatetag|resourceguid|immutableid|internalid|uniqueid|customerid|accountid|scopeid)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(principalid|objectid|tenantid|clientid|appid|applicationid)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(correlationid|operationid|requestid|traceid|keyid|keyversion|publickey|thumbprint|fingerprint)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(flowlogguid|perimeterguid|restorepointcollectionid)$", KeyRegexOptions),

        // Secrets / credentials
        RegexHelper.CreateRegex(@".*(password|passwd|pwd|passphrase|credential|credentials)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@".*(secret|secretvalue|secrets|secretaccesskey|clientsecret|appsecret|applicationsecret)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(access|account|primary|secondary|shared|master|api|subscription|function|host|sas|encryption|wrapped|unwrapped)key$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^key(material|blob|uri|url)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(refresh|access|bearer|id|sas)token$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(sharedaccesssignature|personalaccesstoken|authorizationcode|pat|instrumentationkey|ingestionkey)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(primary|secondary)?connectionstring$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(connstring|connstr)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(pfx|pfxblob|certificateblob|privatekey|pemprivatekey)$", KeyRegexOptions),
        RegexHelper.CreateRegex(@"^(administratorlogin|adminusername|username|ftpusername)$", KeyRegexOptions),
    ];

    private static readonly Regex[] ValueDenyPatterns =
    [
        RegexHelper.CreateRegex(@"^\d{1,3}(\.\d{1,3}){3}$", ValueRegexOptions), // IPv4
        RegexHelper.CreateRegex(@"^(?=.*:)[0-9a-fA-F:]{6,45}$", ValueRegexOptions), // IPv6
        RegexHelper.CreateRegex(@"^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$", ValueRegexOptions), // GUID
        RegexHelper.CreateRegex(@"^eyJ[A-Za-z0-9_-]+\.", ValueRegexOptions), // JWT
        RegexHelper.CreateRegex(@"^[A-Za-z0-9+/]{40,}={0,2}$", ValueRegexOptions), // base64 blob >=40
        RegexHelper.CreateRegex(@"^[0-9a-fA-F]{32,}$", ValueRegexOptions), // long hex (hash/digest/thumbprint)
        RegexHelper.CreateRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ValueRegexOptions), // email
        RegexHelper.CreateRegex(@"^https?://", ValueRegexOptions), // any URL (per-instance endpoint/portal/discovery)
        RegexHelper.CreateRegex(@"^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}(:\d{2}(\.\d+)?)?(Z|[+-]\d{2}:?\d{2})?)?$", ValueRegexOptions), // ISO 8601 date / datetime
        RegexHelper.CreateRegex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}(:\d{2}(\.\d+)?)?$", ValueRegexOptions), // "2023-08-21 10:35:55" style
        RegexHelper.CreateRegex(@"InstrumentationKey=", ValueRegexOptions), // AppInsights conn string
        RegexHelper.CreateRegex(@"(mongodb(\+srv)?|postgres|mysql)://", ValueRegexOptions), // DB conn strings
        RegexHelper.CreateRegex(@"AccountKey=|SharedAccessKey=", ValueRegexOptions), // storage conn strings
        RegexHelper.CreateRegex(@"Server=.*;Password=", ValueRegexOptions), // SQL conn string
    ];

    public static SubscriptionAggregation Filter(SubscriptionAggregation input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var kept = new Dictionary<string, ResourceTypeAggregation>(StringComparer.Ordinal);

        foreach (var (typeKey, agg) in input.ResourceTypes)
        {
            if (DiscardTypes.Contains(typeKey) || IsArmChildType(typeKey))
            {
                continue;
            }

            var filteredProps = FilterObject(agg.PropertyAggregations) ?? new JsonObject();
            kept[typeKey] = agg with { PropertyAggregations = filteredProps };
        }

        return new SubscriptionAggregation(
            kept,
            input.SubscriptionCount,
            input.ResourceGroupCount);
    }

    private static bool IsArmChildType(string armType)
    {
        int slashes = 0;
        for (int i = 0; i < armType.Length; i++)
        {
            if (armType[i] == '/' && ++slashes >= 2)
            {
                return true;
            }
        }
        return false;
    }

    private static JsonObject? FilterObject(JsonObject node)
    {
        var result = new JsonObject();

        foreach (var kvp in node)
        {
            if (IsKeyDenied(kvp.Key))
            {
                continue;
            }

            if (kvp.Value is not JsonObject child)
            {
                continue;
            }

            JsonObject? filtered = IsLeafCountMap(child)
                ? FilterLeaf(kvp.Key, child)
                : FilterObject(child);

            if (filtered is not null)
            {
                result[kvp.Key] = filtered;
            }
        }

        return result.Count > 0 ? result : null;
    }

    private static JsonObject? FilterLeaf(string key, JsonObject leaf)
    {
        long total = 0;
        foreach (var kvp in leaf)
        {
            if (kvp.Value is JsonValue jv && jv.TryGetValue<int>(out var c))
            {
                total += c;
            }
        }

        if (total <= 0)
        {
            return null;
        }

        var topN = leaf
            .Where(kvp => kvp.Value is JsonValue jv && jv.TryGetValue<int>(out _))
            .Select(kvp => (Key: kvp.Key, Count: kvp.Value!.GetValue<int>()))
            .OrderByDescending(t => t.Count)
            .ThenBy(t => t.Key, StringComparer.Ordinal)
            .Take(TopValuesPerLeaf)
            .ToList();

        long topSum = 0;
        foreach (var (_, count) in topN)
        {
            topSum += count;
        }

        if ((double)topSum / total < MinTopCoverage)
        {
            return null;
        }

        var isRelational = RelationalIdKeys.Contains(key);
        var result = new JsonObject();

        foreach (var (sample, count) in topN)
        {
            if (isRelational)
            {
                if (!sample.StartsWith(ArmIdPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }
            else if (IsValueDenied(sample))
            {
                continue;
            }

            result[sample] = JsonValue.Create(count);
        }

        return result.Count > 0 ? result : null;
    }

    private static bool IsLeafCountMap(JsonObject obj)
    {
        if (obj.Count == 0)
        {
            return false;
        }
        foreach (var kvp in obj)
        {
            if (kvp.Value is not JsonValue v || !v.TryGetValue<int>(out _))
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsKeyDenied(string key)
    {
        for (int i = 0; i < KeyDenyPatterns.Length; i++)
        {
            if (KeyDenyPatterns[i].IsMatch(key))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsValueDenied(string value)
    {
        for (int i = 0; i < ValueDenyPatterns.Length; i++)
        {
            if (ValueDenyPatterns[i].IsMatch(value))
            {
                return true;
            }
        }
        return false;
    }
}
