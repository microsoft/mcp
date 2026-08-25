// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IRemediationService
{
    /// <summary>
    /// Retrieves the remediation package for a recommendation type from the
    /// Microsoft.Advisor/remediationTypes ARM API.
    /// </summary>
    /// <param name="recommendationId">The recommendation type id (GUID) used in the ARM resource path.</param>
    /// <param name="tenant">Optional tenant id or name to authenticate against.</param>
    /// <param name="retryPolicy">Optional retry policy for the underlying request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The remediation package for the recommendation type.</returns>
    Task<RemediationPackage> GetRemediationAsync(
        string recommendationId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
