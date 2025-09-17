// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Extension.Models;
using Azure.Mcp.Tools.Extension.Options;
using Azure.Mcp.Tools.Extension.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Extension.Commands;

public sealed class CliGenerateCommand(ILogger<CliGenerateCommand> logger) : GlobalCommand<CliGenerateOptions>
{
    private const string CommandTitle = "Generate CLI Command";
    private readonly ILogger<CliGenerateCommand> _logger = logger;
    private readonly Option<string> _intentOption = ExtensionOptionDefinitions.CliGenerate.Intent;
    private readonly Option<string> _cliTypeOption = ExtensionOptionDefinitions.CliGenerate.CliType;
    private readonly string[] _allowedCliTypeValues = ["az"];

    public override string Name => "generate";

    public override string Description =>
        """
This tool can generate Azure CLI commands to be used with the corresponding CLI tool to accomplish a goal described by the user. This tool incorporates knowledge of the CLI tool beyond what the LLM knows. Always use this tool to generate the CLI command when the user asks for such CLI commands or wants to use the CLI tool to accomplish something.
""";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_intentOption);
        command.Options.Add(_cliTypeOption);
    }

    protected override CliGenerateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Intent = parseResult.GetValue(_intentOption);
        options.CliType = parseResult.GetValue(_cliTypeOption);
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
            ArgumentNullException.ThrowIfNull(options.Intent);
            var intent = options.Intent;
            var cliType = options.CliType?.ToLowerInvariant();

            if (_allowedCliTypeValues.Contains(cliType)) {
                // Only log the cli type when we know for sure it doesn't have private data.
                context.Activity?.AddTag("cliType", cliType);

                if (cliType == Constants.AzureCliType)
                {
                    ICliGenerateService cliGenerateService = context.GetService<ICliGenerateService>();

                    using HttpResponseMessage responseMessage = await cliGenerateService.GenerateAzureCLICommandAsync(intent);
                    responseMessage.EnsureSuccessStatusCode();

                    var responseBody = await responseMessage.Content.ReadAsStringAsync();
                    CliGenerateResult result = new(responseBody, cliType);
                    context.Response.Results = ResponseResult.Create(result, ExtensionJsonContext.Default.CliGenerateResult);
                }
            }
            else
            {
                throw new ArgumentException($"Invalid CLI type: {options.CliType}. Supported values are: {string.Join(", ", _allowedCliTypeValues)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred generating cli command. Cli type: {CliType}.", options.CliType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
