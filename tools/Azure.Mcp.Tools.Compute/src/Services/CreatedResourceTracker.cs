// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.ResourceManager;

namespace Azure.Mcp.Tools.Compute.Services;

/// <summary>Tracks resources created during a multi-step operation for rollback on failure.</summary>
internal sealed class CreatedResourceTracker(ILogger logger)
{
    private readonly List<ResourceIdentifier> _createdResources = [];

    /// <summary>Record a newly created resource. Order matters — cleanup runs in reverse.</summary>
    public void Track(ResourceIdentifier resourceId) => _createdResources.Add(resourceId);

    /// <summary>True if any resources were created.</summary>
    public bool HasResources => _createdResources.Count > 0;

    /// <summary>
    /// Delete all tracked resources in reverse creation order.
    /// Best-effort: logs errors but does not throw.
    /// Uses a fresh cancellation token with a timeout so rollback can complete
    /// even when the original operation's token was cancelled.
    /// </summary>
    public async Task RollbackAsync(ArmClient armClient)
    {
        // Use a fresh token so rollback isn't short-circuited if the caller cancelled.
        // 5 minutes is enough to delete a handful of network resources but bounded
        // so a hung delete doesn't block the caller indefinitely.
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        var rollbackToken = cts.Token;

        for (var i = _createdResources.Count - 1; i >= 0; i--)
        {
            var id = _createdResources[i];
            try
            {
                logger.LogInformation("Rolling back: deleting {ResourceId}", id);
                var genericResource = armClient.GetGenericResource(id);
                await genericResource.DeleteAsync(WaitUntil.Completed, rollbackToken);
                logger.LogInformation("Rolled back: {ResourceId}", id);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                logger.LogDebug("Resource already deleted during rollback: {ResourceId}", id);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to roll back resource {ResourceId}. Manual cleanup required.", id);
            }
        }
    }
}
