// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Agents;
using Azure.Mcp.Tools.Foundry.Services;
using static Azure.Mcp.Tools.Foundry.Commands.AgentsConnectCommand;

namespace Azure.Mcp.Tools.Foundry.Commands;

public class AgentsGetSdkSampleCommand : GlobalCommand<AgentsGetSdkSampleOptions>
{
    private const string CommandTitle = "Get Sdk code samples to interact with a Foundry Agent";

    public override string Name => "get-sdk-sample";

    public override string Description =>
        """
        Get Sdk code samples to interact with a Foundry Agent using the programming language of choice.
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
        command.Options.Add(FoundryOptionDefinitions.ProgrammingLanguageOption);
    }

    protected override AgentsGetSdkSampleOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ProgrammingLanguage = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ProgrammingLanguageOption);
        return options;
    }

    public override string Title => CommandTitle;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var programmingLanguageLowercase = options.ProgrammingLanguage!.ToLowerInvariant();

            var service = context.GetService<IFoundryService>();
            var result = await service.GetSdkCodeSample(
                programmingLanguageLowercase);

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryJsonContext.Default.AgentsGetSdkCodeSampleResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
