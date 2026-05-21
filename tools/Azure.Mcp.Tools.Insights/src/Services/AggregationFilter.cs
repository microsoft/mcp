// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Frozen;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Azure.Mcp.Tools.Insights.Services.Models;

namespace Azure.Mcp.Tools.Insights.Services;

/// <summary>
/// Filters a <see cref="SubscriptionAggregation"/> produced by <see cref="PropertyAggregator"/>:
/// drops noise resource types, denied property keys, low-coverage leaves, and per-instance or
/// secret-shaped values.
/// </summary>
internal static class AggregationFilter
{
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
        RegexOptions.Compiled | RegexOptions.CultureInvariant;

    private static readonly Regex[] KeyDenyPatterns =
    [
        // Runtime / lifecycle state
        new Regex(@"^(provisioning|operational|usage|power|running|runtime|target|replica|disk|maintenance|availability|content)?state$", KeyRegexOptions),
        new Regex(@"^(scmsitealsostopped|hostnamesdisabled|sitedisabledreason|deploymenterrors|deploymentid|displaystatus|actionsrequired|runtimeavailabilitystate|contentavailabilitystate|storagerecoverydefaultstate|serviceproviderprovisioningstate)$", KeyRegexOptions),
        
        // Timestamps / dates
        new Regex(@".*(time|date|timestamp|timestamputc)$", KeyRegexOptions),
        new Regex(@"^(starttime|earliestrestoredate|provisioningtime|completedtimestamputc|starttimestamputc|timestamputc)$", KeyRegexOptions),
        
        // Per-instance hostnames / endpoints / IPs / URLs
        new Regex(@"^(default)?hostname$", KeyRegexOptions),
        new Regex(@"^(enabled)?hostnames$", KeyRegexOptions),
        new Regex(@"^(fqdn|portalfqdn|ftpshostname|ftpusername|selflink|webspace|stamp|homestamp|destinationstampname|sourcestampname|repositorysitename|mdmid|metricid|buildversion|contentversion)$", KeyRegexOptions),
        new Regex(@"^(servicebusendpoint|eventstreamendpoint|discoveryurl|azureasyncoperationuri|accounturl|datashareuri)$", KeyRegexOptions),
        new Regex(@"^(inbound|outbound|possibleinbound|possibleoutbound|public)?ip(v6)?address(es)?$", KeyRegexOptions),
        new Regex(@"^nameservers$", KeyRegexOptions),
        new Regex(@"^icm\..*$", KeyRegexOptions),
        new Regex(@"^(eligiblelogcategories|sdktelemetryappinsightskey|customdomainverificationid)$", KeyRegexOptions),
        
        // Opaque handles / GUIDs
        new Regex(@"^(etag|forceupdatetag|resourceguid|immutableid|internalid|uniqueid|customerid|accountid|scopeid)$", KeyRegexOptions),
        new Regex(@"^(principalid|objectid|tenantid|clientid|appid|applicationid)$", KeyRegexOptions),
        new Regex(@"^(correlationid|operationid|requestid|traceid|keyid|keyversion|publickey|thumbprint|fingerprint)$", KeyRegexOptions),
        new Regex(@"^(flowlogguid|perimeterguid|restorepointcollectionid)$", KeyRegexOptions),
        
        // Secrets / credentials
        new Regex(@".*(password|passwd|pwd|passphrase|credential|credentials)$", KeyRegexOptions),
        new Regex(@".*(secret|secretvalue|secrets|secretaccesskey|clientsecret|appsecret|applicationsecret)$", KeyRegexOptions),
        new Regex(@"^(access|account|primary|secondary|shared|master|api|subscription|function|host|sas|encryption|wrapped|unwrapped)key$", KeyRegexOptions),
        new Regex(@"^key(material|blob|uri|url)$", KeyRegexOptions),
        new Regex(@"^(refresh|access|bearer|id|sas)token$", KeyRegexOptions),
        new Regex(@"^(sharedaccesssignature|personalaccesstoken|authorizationcode|pat|instrumentationkey|ingestionkey)$", KeyRegexOptions),
        new Regex(@"^(primary|secondary)?connectionstring$", KeyRegexOptions),
        new Regex(@"^(connstring|connstr)$", KeyRegexOptions),
        new Regex(@"^(pfx|pfxblob|certificateblob|privatekey|pemprivatekey)$", KeyRegexOptions),
        new Regex(@"^(administratorlogin|adminusername|username|ftpusername)$", KeyRegexOptions),
    ];

    private static readonly Regex[] ValueDenyPatterns =
    [
        new Regex(@"^\d{1,3}(\.\d{1,3}){3}$", ValueRegexOptions), // IPv4
        new Regex(@"^(?=.*:)[0-9a-fA-F:]{6,45}$", ValueRegexOptions), // IPv6
        new Regex(@"^[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}$", ValueRegexOptions), // GUID
        new Regex(@"^eyJ[A-Za-z0-9_-]+\.", ValueRegexOptions), // JWT
        new Regex(@"^[A-Za-z0-9+/]{40,}={0,2}$", ValueRegexOptions), // base64 blob >=40
        new Regex(@"^[0-9a-fA-F]{32,}$", ValueRegexOptions), // long hex (hash/digest/thumbprint)
        new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ValueRegexOptions), // email
        new Regex(@"^https?://", ValueRegexOptions), // any URL (per-instance endpoint/portal/discovery)
        new Regex(@"^\d{4}-\d{2}-\d{2}(T\d{2}:\d{2}(:\d{2}(\.\d+)?)?(Z|[+-]\d{2}:?\d{2})?)?$", ValueRegexOptions), // ISO 8601 date / datetime
        new Regex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}(:\d{2}(\.\d+)?)?$", ValueRegexOptions), // "2023-08-21 10:35:55" style
        new Regex(@"InstrumentationKey=", ValueRegexOptions), // AppInsights conn string
        new Regex(@"(mongodb(\+srv)?|postgres|mysql)://", ValueRegexOptions), // DB conn strings
        new Regex(@"AccountKey=|SharedAccessKey=", ValueRegexOptions), // storage conn strings
        new Regex(@"Server=.*;Password=", ValueRegexOptions), // SQL conn string
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

            JsonObject? filtered = IsLeafFractionMap(child)
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
        double total = 0;
        foreach (var kvp in leaf)
        {
            if (kvp.Value is JsonValue jv && jv.TryGetValue<double>(out var d))
            {
                total += d;
            }
        }

        if (total < MinTopCoverage)
        {
            return null;
        }

        var isRelational = RelationalIdKeys.Contains(key);
        var result = new JsonObject();

        foreach (var kvp in leaf)
        {
            var sample = kvp.Key;
            if (isRelational)
            {
                if (!sample.StartsWith(ArmIdPrefix, StringComparison.Ordinal))
                {
                    continue;
                }
            }
            else if (IsValueDenied(sample))
            {
                continue;
            }

            result[sample] = kvp.Value?.DeepClone();
        }

        return result.Count > 0 ? result : null;
    }

    private static bool IsLeafFractionMap(JsonObject obj)
    {
        if (obj.Count == 0)
        {
            return false;
        }
        foreach (var kvp in obj)
        {
            if (kvp.Value is not JsonValue v || !v.TryGetValue<double>(out _))
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
