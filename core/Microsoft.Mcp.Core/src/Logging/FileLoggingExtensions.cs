// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Logging;

/// <summary>
/// Extension methods for configuring file logging for support scenarios.
/// </summary>
public static class FileLoggingExtensions
{
    /// <summary>
    /// Configures file logging to write debug-level logs to a folder for support and troubleshooting purposes.
    /// Log files are automatically created with timestamp-based filenames (e.g., azmcp_20251202_14.log)
    /// and rotated hourly to prevent excessively large files during long-running sessions.
    /// </summary>
    /// <param name="builder">The logging builder to configure.</param>
    /// <param name="logFolderPath">The folder path where log files should be written.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddSupportFileLogging(this ILoggingBuilder builder, string logFolderPath)
    {
        builder.Services.AddSingleton<ILoggerProvider>(sp =>
            new FileLoggerProvider(logFolderPath));

        return builder;
    }
}
