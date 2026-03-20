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

public sealed class CliGenerateCommand(ILogger<CliGenerateCommand> logger, ICliGenerateService cliGenerateService) : GlobalCommand<CliGenerateOptions>
{
    private const string CommandTitle = "Generate CLI Command";
    private readonly ILogger<CliGenerateCommand> _logger = logger;
    private readonly ICliGenerateService _cliGenerateService = cliGenerateService;
    private readonly string[] _allowedCliTypeValues = ["az"];

    public override string Id => "3de4ef37-90bf-41f1-8385-5e870c3ae911";

    public override string Name => "generate";

    public override string Description =>
        """
        Generate Azure CLI (az) commands used to accomplish a goal described by the user. This tool incorporates CLI knowledge beyond what you know. Use this tool when the user asks for Azure CLI commands or wants to use the Azure CLI to accomplish something.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        OpenWorld = false,
        Idempotent = true,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ExtensionOptionDefinitions.CliGenerate.Intent);
        command.Options.Add(ExtensionOptionDefinitions.CliGenerate.CliType);

        command.Validators.Add(result =>
        {
            var cliType = result.GetValue(ExtensionOptionDefinitions.CliGenerate.CliType)?.ToLowerInvariant();
            if (!_allowedCliTypeValues.Contains(cliType))
            {
                result.AddError($"Invalid CLI type: {cliType}. Supported values are: {string.Join(", ", _allowedCliTypeValues)}");
            }
        });
    }

    protected override CliGenerateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Intent = parseResult.GetValueOrDefault<string>(ExtensionOptionDefinitions.CliGenerate.Intent.Name);
        options.CliType = parseResult.GetValueOrDefault<string>(ExtensionOptionDefinitions.CliGenerate.CliType.Name);
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
            var cliType = options.CliType?.ToLowerInvariant();

            // Only log the cli type when we know for sure it doesn't have private data.
            context.Activity?.AddTag("cliType", cliType);

            if (cliType == Constants.AzureCliType)
            {
                using HttpResponseMessage responseMessage = await _cliGenerateService.GenerateAzureCLICommandAsync(
                    options.Intent!,
                    cancellationToken);
                responseMessage.EnsureSuccessStatusCode();

                var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                context.Response.Results = ResponseResult.Create(new(responseBody, cliType), ExtensionJsonContext.Default.CliGenerateResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
