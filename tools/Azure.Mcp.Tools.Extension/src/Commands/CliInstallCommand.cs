// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Extension.Models;
using Azure.Mcp.Tools.Extension.Options;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Extension.Commands;

public sealed class CliInstallCommand(ILogger<CliInstallCommand> logger) : GlobalCommand<CliInstallOptions>
{
    private const string CommandTitle = "Get CLI installation instructions";
    private readonly ILogger<CliInstallCommand> _logger = logger;
    private readonly string[] _allowedCliTypeValues = ["az", "azd", "func"];

    public override string Name => "install";

    public override string Description =>
        """
This tool can provide installation instructions for the specified CLI tool among Azure CLI (az), Azure Developer CLI (azd) and Azure Functions Core Tools CLI (func). It incorporates knowledge of the CLI tool beyond what the LLM knows. Use this tool to get installation instructions if you attempt to use the CLI tool but it isn't installed.
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
            var cliType = result.GetValue(ExtensionOptionDefinitions.CliInstall.CliType);
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {

        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var cliType = options.CliType?.ToLowerInvariant();

            if (cliType is null || !_allowedCliTypeValues.Contains(cliType))
            {
                throw new ArgumentException($"Invalid CLI type: {options.CliType}. Supported values are: {string.Join(", ", _allowedCliTypeValues)}");
            }
            ICliInstallService cliInstallService = context.GetService<ICliInstallService>();

            // Only log the cli type when we know for sure it doesn't have private data.
            context.Activity?.AddTag("cliType", cliType);

            using HttpResponseMessage responseMessage = await cliInstallService.GetCliInstallInstructions(cliType);
            responseMessage.EnsureSuccessStatusCode();

            var responseBody = await responseMessage.Content.ReadAsStringAsync();
            CliInstallResult result = new(responseBody, cliType);
            context.Response.Results = ResponseResult.Create(result, ExtensionJsonContext.Default.CliInstallResult);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
