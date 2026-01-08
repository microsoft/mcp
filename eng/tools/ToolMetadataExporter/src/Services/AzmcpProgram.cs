// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToolMetadataExporter.Models;
using ToolSelection.Models;

namespace ToolMetadataExporter.Services;

/// <summary>
/// Represents the MCP server and exposes methods to interact with it.
/// </summary>
public class AzmcpProgram
{
    private readonly string _toolDirectory;
    private readonly string _azureMcp;
    private readonly ILogger<AzmcpProgram> _logger;
    private readonly Lazy<Task<>>

    public AzmcpProgram(IOptions<AppConfiguration> options, ILogger<AzmcpProgram> logger)
    {
        _toolDirectory = options.Value.WorkDirectory ?? throw new ArgumentNullException(nameof(AppConfiguration.WorkDirectory));

        _azureMcp = options.Value.AzmcpExe
            ?? throw new ArgumentNullException(nameof(CommandLineOptions.AzmcpExe));
        _logger = logger;
    }

    /// <summary>
    /// Gets the name of the MCP server.
    /// </summary>
    /// <returns></returns>
    public virtual Task<string> GetServerNameAsync()
    {
        return Utility.GetServerName(_azureMcp);
    }

    /// <summary>
    /// Gets the server version.
    /// </summary>
    /// <returns></returns>
    public virtual Task<string> GetServerVersionAsync()
    {
        return Utility.GetVersionAsync(_azureMcp);
    }

    /// <summary>
    /// Gets the list of tools from the MCP server.
    /// </summary>
    /// <returns></returns>
    public virtual Task<ListToolsResult?> LoadToolsDynamicallyAsync()
    {
        return Utility.LoadToolsDynamicallyAsync(_azureMcp, _toolDirectory, false);
    }
}
