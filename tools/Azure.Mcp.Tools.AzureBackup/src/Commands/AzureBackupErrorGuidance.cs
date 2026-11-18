// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Azure.Mcp.Tools.AzureBackup.Commands;

/// <summary>
/// Builds actionable messages for Azure Backup's <c>UserErrorInvalidRequestParameter</c> errors. The service
/// often reports these as "Parameter NO_PARAM in request is invalid", which does not identify the
/// offending value, so the message lists the options the caller supplied and the documented limits.
/// </summary>
internal static partial class AzureBackupErrorGuidance
{
    internal const string InvalidRequestParameterErrorCode = "UserErrorInvalidRequestParameter";
    private const string UnnamedParameter = "NO_PARAM";

    internal const string VmPolicyLimitsHint =
        "Azure VM policy limits: instant restore 1-5 days for Standard policies (exactly 5 for Standard Weekly) " +
        "or 1-30 days for Enhanced policies, and no longer than daily retention; daily retention 7-9999 days; " +
        "weekly retention up to 5163 weeks; monthly up to 1188 months; yearly up to 99 years. " +
        "Use 'azurebackup policy get' to inspect the policy subtype and current values.";

    internal const string VaultLimitsHint =
        "Storage redundancy accepts GeoRedundant, LocallyRedundant, or ZoneRedundant and can change only before any item is protected; " +
        "soft delete retention is 14-180 days.";

    [GeneratedRegex(@"Parameter\s+(?<name>[A-Za-z0-9_.\-]{1,100})\s+in request is invalid", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex InvalidParameterRegex();

    /// <summary>
    /// Returns the names of the options that were supplied, in the given order.
    /// </summary>
    internal static IReadOnlyList<string> Supplied(params (string Name, bool IsSupplied)[] options) =>
        [.. options.Where(o => o.IsSupplied).Select(o => o.Name)];

    /// <summary>
    /// Builds guidance for an Azure <c>UserErrorInvalidRequestParameter</c> failure. Only the service-defined
    /// parameter name is taken from the Azure message; the rest of the backend text is not echoed. Exceptions
    /// that MCP already wrapped with context (an inner <see cref="RequestFailedException"/>, such as "vault was
    /// created, but setting storage redundancy failed") keep their message.
    /// </summary>
    internal static bool TryDescribeInvalidParameter(
        Exception ex,
        string operation,
        IReadOnlyCollection<string> suppliedOptions,
        string? limitsHint,
        [NotNullWhen(true)] out string? message)
    {
        message = null;
        if (ex is not RequestFailedException requestFailed
            || requestFailed.InnerException is RequestFailedException
            || !string.Equals(requestFailed.ErrorCode, InvalidRequestParameterErrorCode, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var match = InvalidParameterRegex().Match(requestFailed.Message ?? string.Empty);
        var parameter = match.Success ? match.Groups["name"].Value : null;

        var builder = new StringBuilder();
        builder.Append(parameter is null || parameter.Equals(UnnamedParameter, StringComparison.OrdinalIgnoreCase)
            ? $"Azure rejected the {operation} ({InvalidRequestParameterErrorCode}): a supplied value is outside the range the service accepts, but Azure did not identify which one."
            : $"Azure rejected the {operation} ({InvalidRequestParameterErrorCode}): request parameter '{parameter}' is invalid.");
        builder.Append(" Options supplied: ")
            .Append(suppliedOptions.Count == 0 ? "none" : string.Join(", ", suppliedOptions))
            .Append('.');
        if (!string.IsNullOrWhiteSpace(limitsHint))
        {
            builder.Append(' ').Append(limitsHint);
        }

        message = builder.ToString();
        return true;
    }
}
