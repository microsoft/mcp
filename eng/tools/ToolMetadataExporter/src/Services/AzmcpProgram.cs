// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToolMetadataExporter.Models;
using ToolSelection.Models;

namespace ToolMetadataExporter.Services;

/// <summary>
/// Represents the MCP server and exposes methods to interact with it.
/// </summary>
public partial class AzmcpProgram
{
    private readonly string _toolDirectory;

    private readonly Utility _utility;
    private readonly ILogger<AzmcpProgram> _logger;
    private readonly Lazy<Task<ListToolsResult?>> _listToolsTask;

    private readonly Task<ServerInfo?> _serverInfoTask;
    private readonly Task<string> _serverNameTask;
    private readonly Task<string> _serverVersionTask;

    public AzmcpProgram(Utility utility, IOptions<AppConfiguration> options, ILogger<AzmcpProgram> logger)
    {
        _toolDirectory = options.Value.WorkDirectory ?? throw new ArgumentNullException(nameof(AppConfiguration.WorkDirectory));

        AzMcpExe = options.Value.AzmcpExe
            ?? throw new ArgumentNullException(nameof(CommandLineOptions.AzmcpExe));
        _utility = utility;
        _logger = logger;

        _serverInfoTask = GetServerInfoInternalAsync();
        _serverNameTask = GetServerNameInternalAsync();
        _serverVersionTask = GetServerVersionInternalAsync();
        _listToolsTask = new Lazy<Task<ListToolsResult?>>(GetServerToolsInternalAsync);

        var fileInfo = new FileInfo(AzMcpExe);
        AzMcpBuildDateTime = new DateTimeOffset(fileInfo.LastWriteTimeUtc, TimeSpan.Zero);
    }

    /// <summary>
    /// Used to create an empty AzmcpProgram instance for initialization purposes.
    /// </summary>
    internal AzmcpProgram()
    {
        _toolDirectory = string.Empty;
        _utility = null!;
        _logger = null!;

        _serverNameTask = Task.FromResult(string.Empty);
        _serverInfoTask = Task.FromResult<ServerInfo?>(null);
        _serverVersionTask = Task.FromResult(string.Empty);
        _listToolsTask = new Lazy<Task<ListToolsResult?>>(() => Task.FromResult<ListToolsResult?>(null));

        AzMcpExe = string.Empty;
        AzMcpBuildDateTime = DateTimeOffset.MaxValue;
    }

    /// <summary>
    /// Path to the MCP server file name.
    /// </summary>
    public string AzMcpExe { get; }

    /// <summary>
    /// Gets the date time of when <see cref="AzMcpExe" /> was built.
    /// </summary>
    public virtual DateTimeOffset AzMcpBuildDateTime { get; }

    /// <summary>
    /// Gets the name of the MCP server in lower case.
    /// </summary>
    /// <returns></returns>
    public virtual Task<string> GetServerNameAsync() => _serverNameTask;

    /// <summary>
    /// Gets the server version.
    /// </summary>
    /// <returns></returns>
    public virtual Task<string> GetServerVersionAsync() => _serverVersionTask;

    /// <summary>
    /// Gets the list of tools from the MCP server.
    /// </summary>
    /// <returns></returns>
    public virtual Task<ListToolsResult?> LoadToolsDynamicallyAsync() => _listToolsTask.Value;

    /// <summary>
    /// Gets the server name of the MCP server in lower-case
    /// </summary>
    /// <returns>The server name of the MCP server in lowercase.</returns>
    /// <exception cref="InvalidOperationException">If name could not be determined.</exception>
    private async Task<string> GetServerNameInternalAsync()
    {
        ServerInfo? output = await _serverInfoTask;

        if (output != null)
        {
            return output.Name.ToLowerInvariant();
        }

        var serverName = await GetServerNameWithHelpCommandAsync();

        if (serverName == null)
        {
            throw new InvalidOperationException("Failed to determine server name via `server info` and `--help`.");
        }
        else
        {
            return serverName.ToLowerInvariant();
        }
    }

    /// <summary>
    /// Gets the server version of the MCP server.
    /// </summary>
    /// <returns>Semver version of the MCP server.</returns>
    /// <exception cref="InvalidOperationException">If a version could not be determined.</exception>
    private async Task<string> GetServerVersionInternalAsync()
    {
        ServerInfo? output = await _serverInfoTask;

        if (output != null)
        {
            return output.Version;
        }
        else
        {
            return await InvokeServerVersionCommandAsync();
        }
    }

    private Task<ListToolsResult?> GetServerToolsInternalAsync()
    {
        return _utility.LoadToolsDynamicallyAsync(AzMcpExe, _toolDirectory, isCiMode: false);
    }

    /// <summary>
    /// Gets server information by invoking the "server info" command in the MCP server.
    /// </summary>
    /// <returns>The server information. Null, if output from `server info` could not be parsed from JSON.
    /// This may be the case when "server info" has not been implemented by the server.</returns>
    private async Task<ServerInfo?> GetServerInfoInternalAsync()
    {
        var output = await _utility.ExecuteAzmcpAsync(AzMcpExe, "server info", checkErrorCode: false);

        try
        {
            ServerInfoResult? result = JsonSerializer.Deserialize(output, ModelsSerializationContext.Default.ServerInfoResult);
            if (result == null || result.Results == null)
            {
                _logger.LogDebug("The MCP server returned an invalid JSON response. Output: {Output}", output);
            }

            return result?.Results;
        }
        catch (JsonException ex)
        {
            _logger.LogDebug(ex, "The MCP server did not return valid JSON output for the 'server info' command. Output: {Output}", output);
            return null;
        }
    }

    /// <summary>
    /// Invokes the MCP server with the --version argument to get the server version.
    /// </summary>
    /// <returns>A semver compatible version.</returns>
    private async Task<string> InvokeServerVersionCommandAsync()
    {
        // Invoking --version returns an error code of 1.
        var versionOutput = (await _utility.ExecuteAzmcpAsync(AzMcpExe, "--version", checkErrorCode: false)).Trim();

        // The version output may contain a git hash after a '+' character.
        // Example: "1.0.0+4c6c98bca777f54350e426c01177a2b91ad12fd4"
        int hashSeparator = versionOutput.IndexOf('+');
        if (hashSeparator != -1)
        {
            versionOutput = versionOutput.Substring(0, hashSeparator);
        }

        return versionOutput;
    }

    /// <summary>
    /// Invokes the MCP server with the --help argument to get the server name.
    /// </summary>
    /// <returns>The server name for the MCP server.  Null if the server name could not be found.</returns>
    /// <remarks>When --help is invoked the name is found after the "Description:" line. Spaces in the server
    /// name are replaced with periods.  Example: "Azure Mcp Server" becomes "Azure.Mcp.Server".</remarks>
    private async Task<string?> GetServerNameWithHelpCommandAsync()
    {
        // Invoking --help returns an error code of 1.
        var helpOutput = await _utility.ExecuteAzmcpAsync(AzMcpExe, "--help", checkErrorCode: false);

        // Parse the help output by looking for "Description".  The following line
        // is the server name.
        // Example:
        //
        //          Description:
        //              Azure MCP Server
        var lines = MyRegex().Split(helpOutput);
        var isFound = false;
        var serverName = string.Empty;
        for (int i = 0; i < lines.Length; i++)
        {
            string? line = lines[i];
            if (line.StartsWith("Description:"))
            {
                isFound = true;
                serverName = lines[i + 1].Trim();
            }
        }

        if (!isFound)
        {
            _logger.LogDebug("Could not find server name in --help output.");
            return null;
        }
        else
        {
            var modified = serverName.Replace(' ', '.');
            _logger.LogInformation("Found server name: {ServerName} from help. Using: {ModifiedServerName}", serverName, modified);

            return modified;
        }
    }

    [GeneratedRegex(Utility.NewLineRegexPattern)]
    private static partial Regex MyRegex();
}
