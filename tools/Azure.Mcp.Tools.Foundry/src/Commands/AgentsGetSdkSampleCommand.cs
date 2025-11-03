// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Agents;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public class AgentsGetSdkSampleCommand : BaseCommand<AgentsGetSdkSampleOptions>
{
    private const string CommandTitle = "Get code samples to interact with a Foundry Agent using AI Foundry SDK and programming language of your choice";
    public override string Id => "38cc4070-6704-49be-af1b-be55aec8d6d6";
    public override string Name => "get-sdk-sample";

    public override string Description =>
        """
        Get code samples to interact with a Foundry Agent using AI Foundry SDK and programming language of your choice.
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
        var options = new AgentsGetSdkSampleOptions
        {
            ProgrammingLanguage = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ProgrammingLanguageOption)
        };
        return options;
    }

    public override string Title => CommandTitle;

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            var programmingLanguageLowercase = options.ProgrammingLanguage!.ToLowerInvariant();

            var service = context.GetService<IFoundryService>();
            var result = service.GetSdkCodeSample(
                programmingLanguageLowercase);

            // If we can get a code sample, the programming language must be within one of our expected values so it's safe to log it.
            context.Activity?.AddTag("programmingLanguage", programmingLanguageLowercase);

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryJsonContext.Default.AgentsGetSdkCodeSampleResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
