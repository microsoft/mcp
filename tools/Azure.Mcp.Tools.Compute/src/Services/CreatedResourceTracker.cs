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
            await DeleteWithRetryAsync(armClient, id, rollbackToken);
        }

        // Clear the list so HasResources reflects post-rollback state and a stray
        // second call doesn't re-issue deletes for the same IDs.
        _createdResources.Clear();
    }

    /// <summary>
    /// Delete a single resource with bounded retries to handle transient/expected
    /// conflicts that resolve on their own.
    /// Specifically handles Azure RP's NIC reservation: when a VM PUT fails, the
    /// NIC remains reserved for that VM for ~180 seconds; deleting it during that
    /// window returns HTTP 400 NicReservedForAnotherVm. Retrying after a backoff
    /// lets the reservation expire so the cascade (NIC -> VNet -> NSG) can proceed.
    /// Other dependency-conflict errors (e.g. InUseSubnetCannotBeDeleted) are also
    /// retried since they typically clear once a prior resource in the rollback
    /// chain finishes deleting on the server side.
    /// </summary>
    private async Task DeleteWithRetryAsync(ArmClient armClient, ResourceIdentifier id, CancellationToken cancellationToken)
    {
        // Backoff schedule. Total wait budget covers Azure's 180s NIC reservation
        // with margin for the actual delete LROs themselves.
        var delays = new[]
        {
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(45),
            TimeSpan.FromSeconds(60),
            TimeSpan.FromSeconds(60),
        };

        var attempt = 0;
        while (true)
        {
            try
            {
                logger.LogInformation("Rolling back: deleting {ResourceId} (attempt {Attempt})", id, attempt + 1);
                var genericResource = armClient.GetGenericResource(id);
                await genericResource.DeleteAsync(WaitUntil.Completed, cancellationToken);
                logger.LogInformation("Rolled back: {ResourceId}", id);
                return;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                logger.LogDebug("Resource already deleted during rollback: {ResourceId}", id);
                return;
            }
            catch (RequestFailedException ex) when (IsRetriableRollbackError(ex) && attempt < delays.Length)
            {
                logger.LogInformation(
                    "Rollback delete of {ResourceId} returned {ErrorCode}; retrying in {Delay}s.",
                    id, ex.ErrorCode, delays[attempt].TotalSeconds);
                try
                {
                    await Task.Delay(delays[attempt], cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Rollback delete of {ResourceId} aborted: rollback budget exhausted. Manual cleanup required.", id);
                    return;
                }
                attempt++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to roll back resource {ResourceId}. Manual cleanup required.", id);
                return;
            }
        }
    }

    private static bool IsRetriableRollbackError(RequestFailedException ex) =>
        ex.Status == 400 && ex.ErrorCode is
            "NicReservedForAnotherVm" or
            "InUseSubnetCannotBeDeleted" or
            "InUseNetworkSecurityGroupCannotBeDeleted" or
            "InUseRouteTableCannotBeDeleted";
}
