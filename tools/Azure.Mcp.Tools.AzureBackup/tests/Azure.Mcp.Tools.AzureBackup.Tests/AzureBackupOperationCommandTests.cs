// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Mcp.Tools.AzureBackup.Tests.Operation;
using Azure.ResourceManager;
using Azure.ResourceManager.RecoveryServices;
using Azure.ResourceManager.RecoveryServicesBackup;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Generated.Models;
using Microsoft.Mcp.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests;

// Requires the EXISTING protected ${ResourceBaseName}-sqlvm in ${ResourceBaseName}-sqlrsv.
// Does not create policies/resources, enable/disable protection, or change disk coverage.
public class AzureBackupOperationCommandTests(
    ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    public override List<string> DisabledDefaultSanitizers { get; } = ["AZSDK3430", "AZSDK3493", "AZSDK3436"];

    public override CustomDefaultMatcher? TestMatcher => new()
    {
        ExcludedHeaders = "Authorization,Content-Type,x-ms-client-request-id",
        CompareBodies = true
    };

    public override List<GeneralRegexSanitizer> GeneralRegexSanitizers { get; } =
    [
        new(new() { Regex = "(?i)(?<=tenantId=)[0-9a-f-]{36}", Value = "00000000-0000-0000-0000-000000000000" }),
        new(new() { Regex = "(?i)(?<=objectId=)[0-9a-f-]{36}", Value = "00000000-0000-0000-0000-000000000000" }),
        new(new() { Regex = @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", Value = "sanitized@example.com" })
    ];

    public override async ValueTask InitializeAsync()
    {
        await LoadSettingsAsync();
        // Unlike the default URI-only group sanitizer, this also covers container/item
        // names in bodies and response headers. Escape input, don't interpret names as regex.
        GeneralRegexSanitizers.Add(new(new()
        {
            Regex = "(?i)" + Regex.Escape(Settings.ResourceGroupName),
            Value = "Sanitized"
        }));
        GeneralRegexSanitizers.Add(new(new()
        {
            Regex = "(?i)" + Regex.Escape(Settings.TenantId),
            Value = "00000000-0000-0000-0000-000000000000"
        }));
        await base.InitializeAsync();
    }

    [Fact]
    public async Task OperationGet_RsvCurrentPolicyUpdate_QueriesBothScopesAndValidatesReturnedJobs()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var vault = $"{Settings.ResourceBaseName}-sqlrsv";
        var container = $"IaasVMContainer;iaasvmcontainerv2;{Settings.ResourceGroupName};{Settings.ResourceBaseName}-sqlvm";
        var item = $"VM;iaasvmcontainerv2;{Settings.ResourceGroupName};{Settings.ResourceBaseName}-sqlvm";
        var services = new ServiceCollection();
        var builder = services.AddHttpClient("operation-recording");
        if (TestMode != TestMode.Live)
        {
            Assert.NotNull(Proxy);
            builder.AddHttpMessageHandler(() => new OperationRecordingHandler(new Uri(Proxy.BaseUri), RecordingId,
                TestMode == TestMode.Playback ? "playback" : "record"));
        }
        using var provider = services.BuildServiceProvider();
        using var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient("operation-recording");
        TokenCredential credential;
        if (TestMode == TestMode.Playback)
        {
            credential = Substitute.For<TokenCredential>();
            credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
                .Returns(new AccessToken("recorded-test-token", DateTimeOffset.UtcNow.AddHours(1)));
        }
        else
        {
            credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { TenantId = Settings.TenantId });
        }

        var client = new ArmClient(credential, Settings.SubscriptionId, new ArmClientOptions
        {
            Transport = new HttpClientTransport(httpClient)
        });
        var itemId = BackupProtectedItemResource.CreateResourceIdentifier(Settings.SubscriptionId, Settings.ResourceGroupName, vault, "Azure", container, item);
        var resource = client.GetBackupProtectedItemResource(itemId);
        var existing = (await resource.GetAsync(cancellationToken: cancellationToken)).Value.Data;
        var vm = Assert.IsType<IaasComputeVmProtectedItem>(existing.Properties);
        Assert.NotNull(vm.PolicyId);
        Assert.Equal(BackupProtectionState.Protected, vm.ProtectionState);
        var policyBefore = vm.PolicyId;
        var sourceBefore = vm.SourceResourceId;
        var inclusionBefore = vm.ExtendedProperties?.DiskExclusionProperties?.IsInclusionList;
        var disksBefore = vm.ExtendedProperties?.DiskExclusionProperties?.DiskLunList.ToArray() ?? [];

        // Re-submit the fetched model unchanged: same policy, protection state and disk
        // exclusions. Never use the existing update-protection tool's ambiguous JobId.
        var update = await resource.UpdateAsync(WaitUntil.Started, existing, cancellationToken: cancellationToken);
        var response = update.GetRawResponse();
        Assert.True(response.Headers.TryGetValue("Azure-AsyncOperation", out var operationUrl),
            "Expected a fresh asynchronous operation status header from the SDK update.");
        Assert.True(Uri.TryCreate(operationUrl, UriKind.Absolute, out var operationUri));
        Assert.Equal(ArmEnvironment.AzurePublicCloud.Endpoint.Host, operationUri.Host);
        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(Settings.SubscriptionId, Settings.ResourceGroupName, vault);
        var decodedPath = Uri.UnescapeDataString(operationUri.AbsolutePath);
        var vaultPrefix = vaultId + "/backupOperations/";
        var itemPrefix = itemId + "/operationsStatus/";
        Assert.True(decodedPath.StartsWith(vaultPrefix, StringComparison.OrdinalIgnoreCase) ||
            decodedPath.StartsWith(itemPrefix, StringComparison.OrdinalIgnoreCase), "Unexpected operation status scope.");
        var operation = Uri.UnescapeDataString(operationUri.Segments[^1]);
        Assert.DoesNotContain('/', operation);
        Assert.NotEmpty(operation);

        // Both GET scopes must succeed; a 404 or missing recording is not an acceptable skip.
        foreach (var itemScope in new[] { false, true })
        {
            Dictionary<string, object?> args = new()
            {
                ["subscription"] = Settings.SubscriptionId,
                ["tenant"] = Settings.TenantId,
                ["resource-group"] = Settings.ResourceGroupName,
                ["vault"] = vault,
                ["operation"] = operation
            };
            if (itemScope)
            {
                args["container"] = container;
                args["protected-item"] = item;
                args["fabric"] = "Azure";
            }
            var result = (await CallToolAsync("azurebackup_operation_get", args)).AssertProperty("operation");
            Assert.Equal(itemScope ? "protectedItem" : "vault", result.AssertProperty("scope").GetString());
            Assert.Equal(operation, result.AssertProperty("operationId").GetString());
            Assert.Equal("rsv", result.AssertProperty("vaultType").GetString());
            Assert.Contains(result.AssertProperty("status").GetString(), new[] { "InProgress", "Succeeded" });
            HashSet<string> jobs = [];
            if (result.TryGetProperty("jobId", out var job) && job.ValueKind == JsonValueKind.String)
            {
                jobs.Add(job.GetString()!);
            }
            foreach (var jobEntry in result.AssertProperty("jobIds").EnumerateArray())
            {
                jobs.Add(jobEntry.GetString()!);
            }
            // No jobs is legitimate. Validate every real returned job, never the operation ID.
            foreach (var jobId in jobs)
            {
                var jobResult = await CallToolAsync("azurebackup_job_get", new()
                {
                    ["subscription"] = Settings.SubscriptionId,
                    ["tenant"] = Settings.TenantId,
                    ["resource-group"] = Settings.ResourceGroupName,
                    ["vault"] = vault,
                    ["vault-type"] = "rsv",
                    ["job"] = jobId
                });
                var returnedJob = Assert.Single(jobResult.AssertProperty("jobs").EnumerateArray());
                Assert.Equal(jobId, returnedJob.AssertProperty("name").GetString());
                returnedJob.AssertProperty("status");
            }
        }

        // Wait for the re-submitted update to reach a terminal state so the final-state assertions
        // observe the settled write and the operation does not linger into later tests or cleanup.
        await update.WaitForCompletionResponseAsync(cancellationToken);

        var after = Assert.IsType<IaasComputeVmProtectedItem>((await resource.GetAsync(cancellationToken: cancellationToken)).Value.Data.Properties);
        Assert.Equal(policyBefore, after.PolicyId);
        Assert.Equal(sourceBefore, after.SourceResourceId);
        Assert.Equal(BackupProtectionState.Protected, after.ProtectionState);
        Assert.Equal(inclusionBefore, after.ExtendedProperties?.DiskExclusionProperties?.IsInclusionList);
        Assert.Equal(disksBefore, after.ExtendedProperties?.DiskExclusionProperties?.DiskLunList.ToArray() ?? []);
    }
}
