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
    Id = "3de4ef37-90bf-41f1-8385-5e870c3ae911",
    Name = "generate",
    Title = "Generate CLI Command",
    Description = "Generate Azure CLI (az) commands used to accomplish a goal described by the user. This tool incorporates CLI knowledge beyond what you know. Use this tool when the user asks for Azure CLI commands or wants to use the Azure CLI to accomplish something.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CliGenerateCommand(ILogger<CliGenerateCommand> logger, ICliGenerateService cliGenerateService)
    : AuthenticatedCommand<CliGenerateOptions, CliGenerateResult>
{
    private readonly ILogger<CliGenerateCommand> _logger = logger;
    private readonly ICliGenerateService _cliGenerateService = cliGenerateService;
    private static readonly string[] s_allowedCliTypeValues = ["az"];

    public override void ValidateOptions(CliGenerateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!s_allowedCliTypeValues.Contains(options.CliType.ToLowerInvariant()))
        {
            validationResult.Errors.Add($"Invalid CLI type: {options.CliType}. Supported values are: {string.Join(", ", s_allowedCliTypeValues)}");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CliGenerateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var cliType = options.CliType?.ToLowerInvariant();

            // Only log the cli type when we know for sure it doesn't have private data.
            context.Activity?.AddTag("cliType", cliType);

            if (cliType == Constants.AzureCliType)
            {
                using HttpResponseMessage responseMessage = await _cliGenerateService.GenerateAzureCLICommandAsync(
                    options.Intent,
                    cancellationToken);
                responseMessage.EnsureSuccessStatusCode();

                var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                context.Response.Results = ResponseResult.Create(new(responseBody, cliType), ExtensionJsonContext.Default.CliGenerateResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. CliType: {CliType}.", Name, options.CliType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
