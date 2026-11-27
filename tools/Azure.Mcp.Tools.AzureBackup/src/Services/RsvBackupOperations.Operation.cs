// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.RecoveryServices;
using Azure.ResourceManager.RecoveryServicesBackup;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureBackup.Services;

// RSV asynchronous operation status (backupOperations / operationsStatus).
public sealed partial class RsvBackupOperations
{
    private const string OperationApiVersion = "2025-02-01";

    public async Task<BackupOperationInfo> GetOperationAsync(
        string operation, string vault, string resourceGroup, string? subscription,
        string? container, string? protectedItem, string? fabric, string? tenant,
        CancellationToken cancellationToken)
    {
        var validationResult = new ValidationResult();
        RsvOperationInputValidator.Validate(operation, vault, resourceGroup, container, protectedItem, fabric, validationResult);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join(" ", validationResult.Errors));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        subscription = (await AzureService.GetSubscription(subscription, tenant, cancellationToken)).Id.SubscriptionId;

        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vault);
        var itemScope = container is not null;
        var scopeId = itemScope
            ? BackupProtectedItemResource.CreateResourceIdentifier(subscription, resourceGroup, vault, fabric ?? "Azure", container!, protectedItem!)
            : vaultId;

        // The SDK has no public status GET for these endpoints. Build an authenticated pipeline
        // over AzureService.GetClient(), which delegates to IHttpClientFactory.CreateClient for
        // test-proxy recording, and target the cloud-aware ARM endpoint. SDK retry defaults apply.
        var token = await GetArmAccessTokenAsync(tenant, cancellationToken);
        using var client = AzureService.GetClient();
        var pipeline = HttpPipelineBuilder.Build(AddDefaultPolicies(new ArmClientOptions
        {
            Transport = new HttpClientTransport(client)
        }));

        using var request = pipeline.CreateRequest();
        request.Method = RequestMethod.Get;
        request.Uri.Reset(AzureService.CloudConfiguration.ArmEnvironment.Endpoint);
        foreach (var segment in scopeId.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            request.Uri.AppendPath("/", escape: false);
            request.Uri.AppendPath(segment, escape: true);
        }
        request.Uri.AppendPath(itemScope ? "/operationsStatus/" : "/backupOperations/", escape: false);
        request.Uri.AppendPath(operation, escape: true);
        request.Uri.AppendQuery("api-version", OperationApiVersion, true);
        request.Headers.Add("Authorization", $"Bearer {token.Token}");
        request.Headers.Add("Accept", "application/json");
        var resourceId = request.Uri.ToUri().AbsolutePath;

        using var response = await pipeline.SendRequestAsync(request, cancellationToken);
        if (response.Status != 200)
        {
            // Never read, log or embed HTTP error bodies (or headers) in exceptions.
            throw new RequestFailedException(response.Status, "Unable to retrieve RSV operation status.");
        }
        await using var contentStream = response.Content.ToStream();
        using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var status = ReadString(root, "status");
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new InvalidDataException("The RSV operation response has no status.");
        }

        BackupOperationError? error = null;
        if (root.TryGetProperty("error", out var errorElement) && errorElement.ValueKind == JsonValueKind.Object)
        {
            error = new(ReadString(errorElement, "code"),
                "Azure reported an operation error. Consult Azure Backup diagnostics for details; backend messages are omitted.");
        }
        string? jobId = null;
        List<string> jobIds = [];
        Dictionary<string, string> failedJobsError = [];
        if (root.TryGetProperty("properties", out var properties) && properties.ValueKind == JsonValueKind.Object)
        {
            jobId = ReadString(properties, "jobId");
            if (string.IsNullOrWhiteSpace(jobId))
            {
                jobId = null;
            }
            if (properties.TryGetProperty("jobIds", out var jobs) && jobs.ValueKind == JsonValueKind.Array)
            {
                foreach (var job in jobs.EnumerateArray())
                {
                    if (job.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(job.GetString()))
                    {
                        jobIds.Add(job.GetString()!);
                    }
                }
            }
            if (properties.TryGetProperty("failedJobsError", out var failedJobs) && failedJobs.ValueKind == JsonValueKind.Object)
            {
                foreach (var job in failedJobs.EnumerateObject())
                {
                    failedJobsError[job.Name] = (job.Value.ValueKind == JsonValueKind.String ? job.Value.GetString() : null) ?? "Unknown";
                }
            }
        }
        // Deliberate allowlist: never return other polymorphic properties (e.g. ILR scripts/secrets).
        // No fallback to operation ID, latest job, or GUID-shape guessing.
        return new(operation, resourceId, itemScope ? "protectedItem" : "vault", "rsv", status,
            ReadTime(root, "startTime"), ReadTime(root, "endTime"), error, jobId, jobIds, failedJobsError);
    }

    private static string? ReadString(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() : null;

    private static DateTimeOffset? ReadTime(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && value.TryGetDateTimeOffset(out var time)
            ? time : null;
}
