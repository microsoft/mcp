// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests;

public abstract class ResilienceManagementRecordedTestsBase(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override ValueTask InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("MCP_GOALS_TEST_ECY") == "true")
        {
            // Opt-in Goals canary recording rewrites ARM requests to eastus2euap, which endpoint validation rejects.
            SetArguments("server", "start", "--mode", "all", "--dangerously-disable-elicitation", "--disable-caching",
                "--disable-proxy-tools", "--dangerously-disable-ssrf-protections-by-namespace", "resiliency");
        }

        return base.InitializeAsync();
    }

    // Canary Goals recordings carry a routing header that playback without MCP_GOALS_TEST_ECY does not send.
    public override CustomDefaultMatcher? TestMatcher { get; set; } = new()
    {
        ExcludedHeaders = "x-ms-rhub-routing-location"
    };

    // Preserve LRO Location paths during recording; replacing the entire header breaks polling playback.
    public override List<string> DisabledDefaultSanitizers =>
    [
        .. base.DisabledDefaultSanitizers,
        "AZSDK2003"
    ];

    public override List<HeaderRegexSanitizer> HeaderRegexSanitizers =>
    [
        .. base.HeaderRegexSanitizers,
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("x-ms-operation-identifier")
        {
            Value = "sanitized"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("operation-id")
        {
            Value = "sanitized"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("Location")
        {
            Regex = "([?&](?:t|c|s|h)=)(?<value>[^&]+)",
            GroupForReplace = "value",
            Value = "sanitized"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("Azure-AsyncOperation")
        {
            Regex = "([?&](?:t|c|s|h)=)(?<value>[^&]+)",
            GroupForReplace = "value",
            Value = "sanitized"
        }),
        // Canary Goals LRO polling must replay through the default ARM host that endpoint validation allows.
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("Location")
        {
            Regex = @"(?i)eastus2euap\.management\.azure\.com",
            Value = "management.azure.com"
        }),
        new HeaderRegexSanitizer(new HeaderRegexSanitizerBody("Azure-AsyncOperation")
        {
            Regex = @"(?i)eastus2euap\.management\.azure\.com",
            Value = "management.azure.com"
        })
    ];

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = @"(?i)[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}",
            Value = "sanitized@example.com"
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = @"(?i)(?:object id|client) '(?<id>[a-f0-9-]{36})'",
            GroupForReplace = "id",
            Value = "00000000-0000-0000-0000-000000000000"
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = @"(?i)/resourcegroups/(?<name>[^/""\\?]+)",
            GroupForReplace = "name",
            Value = "Sanitized"
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody
        {
            Regex = @"(?i)resource group '(?<name>[^']+)'",
            GroupForReplace = "name",
            Value = "Sanitized"
        })
    ];

    public override string RegisterOrRetrieveDeploymentOutputVariable(string name, string deploymentOutputName)
    {
        var value = base.RegisterOrRetrieveDeploymentOutputVariable(name, deploymentOutputName);
        if (TestMode != Microsoft.Mcp.Tests.Helpers.TestMode.Record)
        {
            return value;
        }

        if (IsResourceGroupVariable(name) ||
            value.Equals(Settings.ResourceGroupName, StringComparison.OrdinalIgnoreCase))
        {
            TestVariables[name] = "Sanitized";
        }
        else if (value.StartsWith('/'))
        {
            // The proxy does not apply HTTP sanitizers to saved playback variables.
            var segments = value.Split('/');
            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (segments[i].Equals("subscriptions", StringComparison.OrdinalIgnoreCase))
                {
                    segments[++i] = Guid.Empty.ToString();
                }
                else if (segments[i].Equals("resourceGroups", StringComparison.OrdinalIgnoreCase))
                {
                    segments[++i] = "Sanitized";
                }
            }
            TestVariables[name] = string.Join('/', segments);
        }
        return value;
    }

    public override string RegisterOrRetrieveVariable(string name, string value)
    {
        var registeredValue = base.RegisterOrRetrieveVariable(name, value);
        if (TestMode == Microsoft.Mcp.Tests.Helpers.TestMode.Record &&
            (IsResourceGroupVariable(name) ||
             value.Equals(Settings.ResourceGroupName, StringComparison.OrdinalIgnoreCase)))
        {
            TestVariables[name] = "Sanitized";
        }
        return registeredValue;
    }

    private static bool IsResourceGroupVariable(string name)
        => name.Contains("resourceGroup", StringComparison.OrdinalIgnoreCase) ||
           name.Contains("assetGroup", StringComparison.OrdinalIgnoreCase);

    public override List<UriRegexSanitizer> UriRegexSanitizers =>
    [
        .. base.UriRegexSanitizers,
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "([?&](?:t|c|s|h)=)(?<value>[^&]+)",
            GroupForReplace = "value",
            Value = "sanitized"
        }),
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = @"resource[Gg]roups/([^?\\/]+)",
            GroupForReplace = "1",
            Value = "Sanitized"
        }),
        // Keep older recordings compatible with the backend-required operationId query parameter.
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = "&operationId=[^&]+",
            Value = string.Empty
        }),
        // Goals canary recordings (MCP_GOALS_TEST_ECY) must replay against the default ARM host.
        new UriRegexSanitizer(new UriRegexSanitizerBody
        {
            Regex = @"(?i)(?:eastus2euap|sanitized)\.management\.azure\.com",
            Value = "Sanitized.azure.com"
        })
    ];

    public override List<BodyKeySanitizer> BodyKeySanitizers =>
    [
        .. base.BodyKeySanitizers,
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..subscription")
        {
            Value = "Sanitized"
        }),
        // Preserve ARM ID shapes; GA SDK models and command inputs validate them.
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..resourceGroup")
        {
            Value = "Sanitized"
        }),
        // The report download URL carries a signed token that grants read access to the report.
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..downloadUri")
        {
            Value = "https://sanitized.blob.core.windows.net/reports/report.html"
        }),
        new BodyKeySanitizer(new BodyKeySanitizerBody("$..downloadUrl")
        {
            Value = "https://sanitized.blob.core.windows.net/reports/report.html"
        }),
        // Recovery plan creation generates a new default-group GUID on each run.
        new BodyKeySanitizer(new BodyKeySanitizerBody("$.properties.recoveryGroupsSetting.defaultGroup.properties.groupUniqueId")
        {
            Value = Guid.Empty.ToString()
        })
    ];
}
