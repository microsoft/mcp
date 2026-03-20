// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Extension.Options;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Extension.Commands;

public sealed class CliInstallCommand(ILogger<CliInstallCommand> logger, ICliInstallService cliInstallService) : GlobalCommand<CliInstallOptions>
{
    private const string CommandTitle = "Get CLI installation instructions";
    private readonly ILogger<CliInstallCommand> _logger = logger;
    private readonly ICliInstallService _cliInstallService = cliInstallService;
    private readonly string[] _allowedCliTypeValues = ["az", "azd", "func"];

    public override string Id => "464626d0-b9be-4a3b-9f29-858637ab8c10";

    public override string Name => "install";

    public override string Description =>
        """
        Provide installation instructions for Azure CLI (az), Azure Developer CLI (azd), and Azure Functions Core Tools CLI (func). This tool incorporates CLI knowledge beyond what you know. Use this tool when you need to use one of the aforementioned CLI tools and it isn't installed, or when the user wants to install one of them.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        OpenWorld = false,
        Idempotent = true,
        ReadOnly = true,
        Secret = false,
        LocalRequired = true
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ExtensionOptionDefinitions.CliInstall.CliType);

        command.Validators.Add(result =>
        {
            var cliType = result.GetValue(ExtensionOptionDefinitions.CliInstall.CliType)?.ToLowerInvariant();
            if (!_allowedCliTypeValues.Contains(cliType))
            {
                result.AddError($"Invalid CLI type: {cliType}. Supported values are: {string.Join(", ", _allowedCliTypeValues)}");
            }
        });
    }

    protected override CliInstallOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.CliType = parseResult.GetValueOrDefault<string>(ExtensionOptionDefinitions.CliInstall.CliType.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {

        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var cliType = options.CliType?.ToLowerInvariant()!;

            // Only log the cli type when we know for sure it doesn't have private data.
            context.Activity?.AddTag("cliType", cliType);

            using HttpResponseMessage responseMessage = await _cliInstallService.GetCliInstallInstructions(cliType, cancellationToken);
            responseMessage.EnsureSuccessStatusCode();

            var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            context.Response.Results = ResponseResult.Create(new(responseBody, cliType), ExtensionJsonContext.Default.CliInstallResult);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
