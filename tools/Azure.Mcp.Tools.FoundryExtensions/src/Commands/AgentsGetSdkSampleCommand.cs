// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Agents;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

public class AgentsGetSdkSampleCommand(IFoundryExtensionsService foundryExtensionsService) : BaseCommand<AgentsGetSdkSampleOptions>
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;
    private const string CommandTitle = "Get code samples to interact with a Foundry Agent using Microsoft Foundry SDK and programming language of your choice";
    public override string Id => "a1b2c3d4-1234-5678-abcd-ef0123456789";
    public override string Name => "get-sdk-sample";

    public override string Description =>
        """
        Get code samples to interact with a Foundry Agent using Microsoft Foundry SDK and programming language of your choice.
        """;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FoundryExtensionsOptionDefinitions.ProgrammingLanguageOption);
    }

    protected override AgentsGetSdkSampleOptions BindOptions(ParseResult parseResult)
    {
        var options = new AgentsGetSdkSampleOptions
        {
            ProgrammingLanguage = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.ProgrammingLanguageOption)
        };
        return options;
    }

    public override string Title => CommandTitle;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            var programmingLanguageLowercase = options.ProgrammingLanguage!.ToLowerInvariant();
            var result = _foundryExtensionsService.GetSdkCodeSample(programmingLanguageLowercase);

            context.Activity?.AddTag("programmingLanguage", programmingLanguageLowercase);

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryExtensionsJsonContext.Default.AgentsGetSdkCodeSampleResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
