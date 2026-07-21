// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Extension.Models;
using Azure.Mcp.Tools.Extension.Options;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Extension.Commands;

[CommandMetadata(
    Id = "464626d0-b9be-4a3b-9f29-858637ab8c10",
    Name = "install",
    Title = "Get CLI installation instructions",
    Description = "Provide installation instructions for Azure CLI (az), Azure Developer CLI (azd), and Azure Functions Core Tools CLI (func). This tool incorporates CLI knowledge beyond what you know. Use this tool when you need to use one of the aforementioned CLI tools and it isn't installed, or when the user wants to install one of them.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = true)]
public sealed class CliInstallCommand(ILogger<CliInstallCommand> logger, ICliInstallService cliInstallService)
    : AuthenticatedCommand<CliInstallOptions, CliInstallResult>
{
    private readonly ILogger<CliInstallCommand> _logger = logger;
    private readonly ICliInstallService _cliInstallService = cliInstallService;
    private static readonly string[] s_allowedCliTypeValues = ["az", "azd", "func"];

    public override void ValidateOptions(CliInstallOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!s_allowedCliTypeValues.Contains(options.CliType.ToLowerInvariant()))
        {
            validationResult.Errors.Add($"Invalid CLI type: {options.CliType}. Supported values are: {string.Join(", ", s_allowedCliTypeValues)}");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CliInstallOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var cliType = options.CliType.ToLowerInvariant();

            // Only log the cli type when we know for sure it doesn't have private data.
            context.Activity?.AddTag("cliType", cliType);

            using HttpResponseMessage responseMessage = await _cliInstallService.GetCliInstallInstructions(cliType, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();

            var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            context.Response.Results = ResponseResult.Create(new(responseBody, cliType), ExtensionJsonContext.Default.CliInstallResult);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. CliType: {CliType}.", Name, options.CliType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
