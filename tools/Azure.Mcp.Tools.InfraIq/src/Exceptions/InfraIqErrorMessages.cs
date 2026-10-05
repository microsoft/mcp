// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Exceptions;

/// <summary>
/// Deterministic, backend-text-free messages for InfraIQ ARM failures.
/// </summary>
internal static class InfraIqErrorMessages
{
    public static string ForStatus(int status) => status switch
    {
        400 => "The InfraIQ recommendation request was invalid. Review the input values and try again.",
        401 => "Authentication to Azure Resource Manager failed. Sign in again and retry the request.",
        403 => "Authorization failed for the InfraIQ recommendation request. Verify access and permissions for the subscription.",
        404 => "The InfraIQ endpoint or requested scope was not found for this subscription, location, or configuration.",
        408 => "The InfraIQ recommendation request timed out. Retry later or reduce optional enrichment.",
        409 => "The InfraIQ recommendation request could not be completed because of a conflicting service state.",
        413 => "The InfraIQ recommendation request is too large. Reduce the request size and try again.",
        415 => "The InfraIQ recommendation request content type is unsupported.",
        422 => "The InfraIQ recommendation request could not be processed. Review model, workload, deployment, and optimization inputs.",
        429 => "The InfraIQ recommendation request was throttled. Retry after the indicated delay if provided.",
        500 => "The InfraIQ service returned an internal error. Retry later.",
        502 => "The InfraIQ service dependency returned an upstream gateway error. Retry later.",
        503 => "The InfraIQ service is unavailable or delegated ARM access is unavailable. Retry later.",
        504 => "The InfraIQ service dependency timed out. Retry later.",
        _ => "The InfraIQ recommendation request failed."
    };
}
