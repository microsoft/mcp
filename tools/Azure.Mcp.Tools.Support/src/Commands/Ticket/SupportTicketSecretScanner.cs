// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Azure.Mcp.Tools.Support.Commands.Ticket;

/// <summary>
/// Deterministic scan of free-text support ticket fields for common secret formats, so credentials
/// are not sent to a support case. Returns only category names, never the matched values.
/// </summary>
internal static partial class SupportTicketSecretScanner
{
    public const string ConnectionStringCredential = "connection string credential (AccountKey, SharedAccessKey, Password or Pwd)";
    public const string SasToken = "shared access signature (SAS) token";
    public const string BearerToken = "bearer or JWT token";
    public const string PrivateKey = "PEM private key";
    public const string StorageAccountKey = "storage account key";
    public const string AccessToken = "access token or API key";

    public static IReadOnlyList<string> Scan(params string?[] values)
    {
        var categories = new List<string>();
        foreach (var value in values ?? [])
        {
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (ConnectionStringCredentialRegex().IsMatch(value))
            {
                Add(categories, ConnectionStringCredential);
            }

            if (SasSignatureRegex().IsMatch(value) && SasExpiryRegex().IsMatch(value))
            {
                Add(categories, SasToken);
            }

            if (JwtRegex().IsMatch(value) || BearerRegex().IsMatch(value))
            {
                Add(categories, BearerToken);
            }

            if (PrivateKeyRegex().IsMatch(value))
            {
                Add(categories, PrivateKey);
            }

            if (StorageAccountKeyRegex().IsMatch(value))
            {
                Add(categories, StorageAccountKey);
            }

            if (KnownTokenPrefixRegex().IsMatch(value))
            {
                Add(categories, AccessToken);
            }
        }

        return categories;
    }

    private static void Add(List<string> categories, string category)
    {
        if (!categories.Contains(category))
        {
            categories.Add(category);
        }
    }

    [GeneratedRegex(@"\b(?:AccountKey|SharedAccessKey|Password|Pwd)\s*=\s*[^;\s""']{4,}", RegexOptions.IgnoreCase)]
    private static partial Regex ConnectionStringCredentialRegex();

    [GeneratedRegex(@"(?:^|[?&\s])sig=[A-Za-z0-9%+/=]{16,}", RegexOptions.IgnoreCase)]
    private static partial Regex SasSignatureRegex();

    [GeneratedRegex(@"(?:^|[?&\s])se=\d{4}-\d{2}-\d{2}", RegexOptions.IgnoreCase)]
    private static partial Regex SasExpiryRegex();

    [GeneratedRegex(@"\beyJ[A-Za-z0-9_-]{8,}\.eyJ[A-Za-z0-9_-]{8,}\.[A-Za-z0-9_-]{8,}")]
    private static partial Regex JwtRegex();

    [GeneratedRegex(@"\bBearer\s+[A-Za-z0-9\-._~+/]{20,}=*", RegexOptions.IgnoreCase)]
    private static partial Regex BearerRegex();

    [GeneratedRegex(@"-----BEGIN (?:[A-Z0-9]+ )*PRIVATE KEY-----")]
    private static partial Regex PrivateKeyRegex();

    [GeneratedRegex(@"(?<![A-Za-z0-9+/])[A-Za-z0-9+/]{86}==(?![A-Za-z0-9+/=])")]
    private static partial Regex StorageAccountKeyRegex();

    // GitHub, AWS, Slack, Google, npm and Microsoft Entra client secret formats.
    [GeneratedRegex(@"\b(?:gh[pousr]_[A-Za-z0-9]{36,}|github_pat_[A-Za-z0-9_]{22,}|AKIA[0-9A-Z]{16}|xox[abprs]-[A-Za-z0-9-]{10,}|AIza[0-9A-Za-z_\-]{35}|npm_[A-Za-z0-9]{36}|[A-Za-z0-9_\-.]{3}\dQ~[A-Za-z0-9_~.\-]{31,34})")]
    private static partial Regex KnownTokenPrefixRegex();
}
