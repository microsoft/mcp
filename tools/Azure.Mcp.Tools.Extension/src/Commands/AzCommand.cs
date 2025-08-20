// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.ProcessExecution;
using Azure.Mcp.Tools.Extension.Options;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Extension.Commands;

public sealed class AzCommand(ILogger<AzCommand> logger, int processTimeoutSeconds = 300) : GlobalCommand<AzOptions>()
{
    private const string CommandTitle = "Azure CLI Command";
    private readonly ILogger<AzCommand> _logger = logger;
    private readonly int _processTimeoutSeconds = processTimeoutSeconds;
    private readonly Option<string> _commandOption = ExtensionOptionDefinitions.Az.Command;
    private static string? _cachedAzPath;
    private volatile bool _isAuthenticated = false;
    private static readonly SemaphoreSlim s_authSemaphore = new(1, 1);

    /// <summary>
    /// Clears the cached Azure CLI path. Used for testing purposes.
    /// </summary>
    internal static void ClearCachedAzPath()
    {
        _cachedAzPath = null;
    }

    public override string Name => "az";

    public override string Description =>
        """
Your job is to answer questions about an Azure environment by executing Azure CLI commands. You have the following rules:

- Use the Azure CLI to manage Azure resources and services. Do not use any other tool.
- Provide a valid Azure CLI command. For example: 'group list'.
- When deleting or modifying resources, ALWAYS request user confirmation.
- If a command fails, retry 3 times before giving up with an improved version of the code based on the returned feedback.
- When listing resources, ensure pagination is handled correctly so that all resources are returned.
- You can ONLY write code that interacts with Azure. It CANNOT generate charts, tables, graphs, etc.
- You can delete or modify resources in your Azure environment. Always be cautious and include appropriate warnings when providing commands to users.
- Be concise, professional and to the point. Do not give generic advice, always reply with detailed & contextual data sourced from the current Azure environment.
""";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = true, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_commandOption);
    }

    protected override AzOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Command = parseResult.GetValueForOption(_commandOption);
        return options;
    }

    internal static string? FindAzCliPath()
    {
        string executableName = "az";

        // Return cached path if available and still exists
        if (!string.IsNullOrEmpty(_cachedAzPath) && File.Exists(_cachedAzPath))
        {
            return _cachedAzPath;
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return null;

        string[] paths = pathEnv.Split(Path.PathSeparator);
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        foreach (string path in paths)
        {
            string fullPath = Path.Combine(path.Trim(), executableName);

            // On Windows, prioritize .cmd and .bat extensions over the base executable
            // This ensures we use az.cmd instead of the az bash script which isn't executable by .NET
            if (isWindows)
            {
                string cmdPath = Path.ChangeExtension(fullPath, ".cmd");
                if (File.Exists(cmdPath))
                {
                    _cachedAzPath = cmdPath;
                    return _cachedAzPath;
                }
                string batPath = Path.ChangeExtension(fullPath, ".bat");
                if (File.Exists(batPath))
                {
                    _cachedAzPath = batPath;
                    return _cachedAzPath;
                }
            }

            // Fall back to the base executable name
            if (File.Exists(fullPath))
            {
                _cachedAzPath = fullPath;
                return _cachedAzPath;
            }
        }
        return null;
    }

    private async Task<bool> AuthenticateWithAzureCredentialsAsync(IExternalProcessService processService, ILogger logger)
    {
        if (_isAuthenticated)
        {
            Console.WriteLine("Already authenticated with Azure CLI.1");
            return true;
        }

        try
        {
            // Check if the semaphore is already acquired to avoid re-authentication
            bool isAcquired = await s_authSemaphore.WaitAsync(1000);
            if (!isAcquired || _isAuthenticated)
            {
                return _isAuthenticated;
            }
            var credentials = AuthenticationUtils.GetAzureCredentials(logger);
            if (credentials == null)
            {
                logger.LogWarning("Invalid AZURE_CREDENTIALS format. Skipping authentication. Ensure it contains clientId, clientSecret, and tenantId.");
                return false;
            }

            var azPath = FindAzCliPath() ?? throw new FileNotFoundException("Azure CLI executable not found in PATH or common installation locations. Please ensure Azure CLI is installed.");

            var loginCommand = $"login --service-principal -u {credentials.ClientId} -p {credentials.ClientSecret} --tenant {credentials.TenantId}";
            var result = await processService.ExecuteAsync(azPath, loginCommand, 60);

            if (result.ExitCode != 0)
            {
                logger.LogWarning("Failed to authenticate with Azure CLI. Error: {Error}", result.Error);
                return false;
            }

            _isAuthenticated = true;
            logger.LogInformation("Successfully authenticated with Azure CLI using service principal.");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error during service principal authentication. Command will proceed without authentication.");
            return false;
        }
        finally
        {
            s_authSemaphore.Release();
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            ArgumentNullException.ThrowIfNull(options.Command);
            var command = options.Command;
            var processService = context.GetService<IExternalProcessService>();

            // Try to authenticate, but continue even if it fails
            await AuthenticateWithAzureCredentialsAsync(processService, _logger);

            var azPath = FindAzCliPath() ?? throw new FileNotFoundException("Azure CLI executable not found in PATH or common installation locations. Please ensure Azure CLI is installed.");
            var result = await processService.ExecuteAsync(azPath, command, _processTimeoutSeconds);

            if (result.ExitCode != 0)
            {
                context.Response.Status = 500;
                context.Response.Message = result.Error;
            }

            var jElem = processService.ParseJsonOutput(result);
            context.Response.Results = ResponseResult.Create(jElem, ExtensionJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred executing command. Command: {Command}.", options.Command);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
