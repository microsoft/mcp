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
    /// Configures file logging to write debug-level logs to a file for support and troubleshooting purposes.
    /// </summary>
    /// <param name="builder">The logging builder to configure.</param>
    /// <param name="logFilePath">The file path where logs should be written.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddSupportFileLogging(this ILoggingBuilder builder, string logFilePath)
    {
        builder.Services.AddSingleton<ILoggerProvider>(sp =>
            new FileLoggerProvider(logFilePath));

        return builder;
    }
}
