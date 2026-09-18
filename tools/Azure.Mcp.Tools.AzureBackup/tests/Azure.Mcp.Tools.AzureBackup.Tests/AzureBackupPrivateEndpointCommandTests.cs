// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests;

// Split into a separate test class (not a partial of AzureBackupCommandTests) so
// the recording session for the Private Endpoint tests is isolated and can be
// re-recorded independently of the rest of the vault/protected-item suite.
public class AzureBackupPrivateEndpointCommandTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override CustomDefaultMatcher? TestMatcher => new()
    {
        ExcludedHeaders = "Authorization,Content-Type,x-ms-client-request-id",
        CompareBodies = false
    };

    public override List<BodyRegexSanitizer> BodyRegexSanitizers =>
    [
        new BodyRegexSanitizer(new BodyRegexSanitizerBody()
        {
            Regex = "(?<=http://|https://)(?<host>[^/?\\.]+)",
            GroupForReplace = "host",
        })
    ];

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "AzureBackupRG_mcp-test",
            Value = "Sanitized",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "(?i)azurebackuprg_mcp-test",
            Value = "Sanitized",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = @"[A-Za-z0-9._%+-]+@microsoft\.com",
            Value = "sanitized@example.com",
        }),
        new GeneralRegexSanitizer(new GeneralRegexSanitizerBody()
        {
            Regex = "72f988bf-86f1-41af-91ab-2d7cd011db47",
            Value = "00000000-0000-0000-0000-000000000000",
        })
    ];

    [Fact]
    public async Task PrivateEndpointGet_EmptyVault_ReturnsEmptyList()
    {
        // A fresh PE vault (${baseName}-rsv-pe) provisioned by the Bicep template has
        // no Private Endpoint Connections attached. `get` (list mode) must return an
        // empty array with no error.
        var vaultName = $"{Settings.ResourceBaseName}-rsv-pe";

        var result = await CallToolAsync(
            "azurebackup_vault_privateendpoint_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" }
            });

        Assert.True(result.HasValue, "Expected a response from privateendpoint get.");
        var connections = result.Value.AssertProperty("connections");
        Assert.Equal(JsonValueKind.Array, connections.ValueKind);
    }

    [Fact]
    public async Task PrivateEndpointGet_OnDppVault_ReturnsNotSupported()
    {
        // Private Endpoints on Backup vaults (DPP) are not exposed by this tool. The
        // service layer raises NotSupportedException which the command surfaces as an
        // error response containing "not supported".
        var vaultName = $"{Settings.ResourceBaseName}-dpp";

        var result = await CallToolAsync(
            "azurebackup_vault_privateendpoint_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "dpp" }
            });

        Assert.True(result.HasValue, "Expected an error response for DPP vault-type.");
        var text = result.Value.GetProperty("message").GetString() ?? "";
        Output.WriteLine($"Result: {text}");
        Assert.Contains("not supported", text, StringComparison.OrdinalIgnoreCase);
    }

    [LiveTestOnly] // Full PE lifecycle spans Microsoft.Network + Microsoft.RecoveryServices long-running operations that are not deterministic under record/playback.
    [Fact]
    public async Task PrivateEndpointLifecycle_Create_Approve_Get_Delete()
    {
        var vaultName = $"{Settings.ResourceBaseName}-rsv-pe";
        var peName = RegisterOrRetrieveVariable("createdPrivateEndpointName", $"pe-{Random.Shared.NextInt64()}");
        var subnetId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.Network/virtualNetworks/{Settings.ResourceBaseName}-pe-vnet/subnets/pe-subnet";

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: PrivateEndpoint lifecycle (vault={vaultName}, pe={peName})");

        // 1. Create the PE on the vault. Do not auto-approve so we exercise Approve explicitly.
        var createResult = await CallToolAsync(
            "azurebackup_vault_privateendpoint_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" },
                { "private-endpoint-name", peName },
                { "vnet-subnet-id", subnetId },
                { "group-id", "AzureBackup" },
                { "auto-approve", "false" }
            });

        Assert.True(createResult.HasValue, "Expected a response from privateendpoint create.");
        var created = createResult.Value.AssertProperty("connection");
        var pecName = created.AssertProperty("name").GetString();
        Assert.False(string.IsNullOrEmpty(pecName), "Expected created PEC name to be non-empty.");
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Created PEC '{pecName}' on vault '{vaultName}'");

        // 2. Approve the connection.
        var approveResult = await CallToolAsync(
            "azurebackup_vault_privateendpoint_approve-reject",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" },
                { "private-endpoint-name", pecName! },
                { "description", "Approved by MCP lifecycle test" }
            });

        Assert.True(approveResult.HasValue, "Expected a response from privateendpoint approve.");
        var approved = approveResult.Value.AssertProperty("connection");
        Assert.Equal("Approved", approved.AssertProperty("connectionStatus").GetString(), ignoreCase: true);

        // 3. Get returns the approved connection with the expected group id.
        var getResult = await CallToolAsync(
            "azurebackup_vault_privateendpoint_get",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" },
                { "private-endpoint-name", pecName! }
            });

        Assert.True(getResult.HasValue, "Expected a response from privateendpoint get.");
        var got = getResult.Value.AssertProperty("connections");
        Assert.Equal(JsonValueKind.Array, got.ValueKind);
        Assert.Equal(1, got.GetArrayLength());
        var gotConn = got.EnumerateArray().First();
        Assert.Equal("Approved", gotConn.AssertProperty("connectionStatus").GetString(), ignoreCase: true);
        var groupIds = gotConn.AssertProperty("groupIds");
        Assert.Equal(JsonValueKind.Array, groupIds.ValueKind);
        Assert.Contains("AzureBackup", groupIds.EnumerateArray().Select(g => g.GetString()));

        // 4. Delete the vault-side PEC and confirm success.
        var deleteResult = await CallToolAsync(
            "azurebackup_vault_privateendpoint_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" },
                { "private-endpoint-name", pecName! }
            });

        Assert.True(deleteResult.HasValue, "Expected a response from privateendpoint delete.");
        var deleteStatus = deleteResult.Value.AssertProperty("result").AssertProperty("status").GetString();
        Assert.Equal("Succeeded", deleteStatus, ignoreCase: true);
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: PrivateEndpoint lifecycle");
    }

    [LiveTestOnly] // Creating the PE + Private DNS zone group spans Microsoft.Network long-running operations that are not deterministic under record/playback (same rationale as the lifecycle test above).
    [Fact]
    public async Task PrivateEndpointCreate_WithPrivateDnsZone_LinksDnsZoneGroup()
    {
        // Exercises --private-dns-zone-ids / --private-dns-zone-group-name: after the PE is
        // created and auto-approved, a Private DNS zone group is created on the endpoint that
        // links the Bicep-provisioned backup private DNS zone. The zone resource ID is derived
        // from ResourceBaseName-scoped values so it resolves in the live subscription.
        var vaultName = $"{Settings.ResourceBaseName}-rsv-pe";
        var peName = RegisterOrRetrieveVariable("createdPrivateEndpointDnsName", $"pe-dns-{Random.Shared.NextInt64()}");
        var subnetId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.Network/virtualNetworks/{Settings.ResourceBaseName}-pe-vnet/subnets/pe-subnet";
        var privateDnsZoneId = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}/providers/Microsoft.Network/privateDnsZones/privatelink.{Settings.ResourceBaseName}.backup.windowsazure.com";

        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] START: PrivateEndpoint + DNS zone group (vault={vaultName}, pe={peName})");

        var createResult = await CallToolAsync(
            "azurebackup_vault_privateendpoint_create",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" },
                { "private-endpoint-name", peName },
                { "vnet-subnet-id", subnetId },
                { "group-id", "AzureBackup" },
                { "auto-approve", "true" },
                { "private-dns-zone-ids", privateDnsZoneId },
                { "private-dns-zone-group-name", "backup-dns" }
            });

        Assert.True(createResult.HasValue, "Expected a response from privateendpoint create with DNS zone.");
        var created = createResult.Value.AssertProperty("connection");
        var pecName = created.AssertProperty("name").GetString();
        Assert.False(string.IsNullOrEmpty(pecName), "Expected created PEC name to be non-empty.");
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] Created PEC '{pecName}' with DNS zone group on vault '{vaultName}'");

        // Clean up the vault-side PEC so the vault stays eligible for re-runs.
        var deleteResult = await CallToolAsync(
            "azurebackup_vault_privateendpoint_delete",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "resource-group", Settings.ResourceGroupName },
                { "vault", vaultName },
                { "vault-type", "rsv" },
                { "private-endpoint-name", pecName! }
            });

        Assert.True(deleteResult.HasValue, "Expected a response from privateendpoint delete.");
        Assert.Equal("Succeeded", deleteResult.Value.AssertProperty("result").AssertProperty("status").GetString(), ignoreCase: true);
        Output.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DONE: PrivateEndpoint + DNS zone group");
    }
}
