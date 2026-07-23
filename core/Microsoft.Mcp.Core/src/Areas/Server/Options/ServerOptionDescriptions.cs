// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Options;

internal static class ServerOptionDescriptions
{
    internal const string Debug = "Enable debug mode with verbose logging to stderr.";
    internal const string DangerouslyWriteSupportLogsToDir = "Dangerously enables detailed debug-level logging for support and troubleshooting purposes. " +
        "Specify a folder path where log files will be automatically created with timestamp-based filenames (e.g., azmcp_20251202_143052.log). " +
        "This may include sensitive information in logs. Use with extreme caution and only when requested by support.";
}
