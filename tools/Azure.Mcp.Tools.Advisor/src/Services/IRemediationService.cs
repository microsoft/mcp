// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IRemediationService
{
    /// <summary>
    /// Retrieves the remediation package for a recommendation type from the
    /// Microsoft.Advisor/remediationTypes ARM API.
    /// </summary>
    /// <param name="recommendationTypeId">The recommendation type id (GUID) used in the ARM resource path.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The remediation package for the recommendation type.</returns>
    Task<RemediationPackage> GetRemediationAsync(
        string recommendationTypeId,
        CancellationToken cancellationToken = default);
}
