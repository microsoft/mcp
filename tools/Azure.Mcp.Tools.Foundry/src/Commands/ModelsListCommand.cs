// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class ModelsListCommand : GlobalCommand<ModelsListOptions>
{
    private const string CommandTitle = "List Models from Model Catalog";

    public override string Name => "list";

    public override string Description =>
        """
        Retrieves a list of supported models from the Azure AI Foundry catalog.
        This function is useful when a user requests a list of available Foundry models or Foundry Labs projects.
        It fetches models based on optional filters like whether the model supports free playground usage,
        the publisher name, and the license type. The function will return the list of models with useful fields.
        Usage:
            Use this function when users inquire about available models from the Azure AI Foundry catalog.
            It can also be used when filtering models by free playground usage, publisher name, or license type.
            If user didn't specify free playground or ask for models that support GitHub token, always explain that by default it will show the all the models but some of them would support free playground.
            Explain to the user that if they want to find models suitable for prototyping and free to use with support for free playground, they can look for models that supports free playground, or look for models that they can use with GitHub token.
        """;

    public override string Title => CommandTitle;

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
        command.Options.Add(FoundryOptionDefinitions.SearchForFreePlaygroundOption);
        command.Options.Add(FoundryOptionDefinitions.PublisherNameOption);
        command.Options.Add(FoundryOptionDefinitions.LicenseNameOption);
        command.Options.Add(FoundryOptionDefinitions.ModelNameOption.AsOptional());
    }

    protected override ModelsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SearchForFreePlayground = parseResult.GetValueOrDefault<bool>(FoundryOptionDefinitions.SearchForFreePlaygroundOption.Name);
        options.PublisherName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.PublisherNameOption.Name);
        options.LicenseName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.LicenseNameOption.Name);
        options.ModelName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ModelNameOption.Name);

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
            var service = context.GetService<IFoundryService>();
            var models = await service.ListModels(
                options.SearchForFreePlayground ?? false,
                options.PublisherName ?? "",
                options.LicenseName ?? "",
                options.ModelName ?? "",
                3,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(models ?? []), FoundryJsonContext.Default.ModelsListCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ModelsListCommandResult(IEnumerable<ModelInformation> Models);
}
