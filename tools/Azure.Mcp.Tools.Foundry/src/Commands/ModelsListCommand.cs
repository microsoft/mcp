// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class ModelsListCommand : GlobalCommand<ModelsListOptions>
{
    private const string CommandTitle = "List Models from Model Catalog";
    private readonly Option<bool> _searchForFreePlaygroundOption = FoundryOptionDefinitions.SearchForFreePlaygroundOption;
    private readonly Option<string> _publisherNameOption = FoundryOptionDefinitions.PublisherNameOption;
    private readonly Option<string> _licenseNameOption = FoundryOptionDefinitions.LicenseNameOption;
    private readonly Option<string> _optionalModelNameOption = FoundryOptionDefinitions.OptionalModelNameOption;

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

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_searchForFreePlaygroundOption);
        command.AddOption(_publisherNameOption);
        command.AddOption(_licenseNameOption);
        command.AddOption(_optionalModelNameOption);
    }

    protected override ModelsListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SearchForFreePlayground = parseResult.GetValueForOption(_searchForFreePlaygroundOption);
        options.PublisherName = parseResult.GetValueForOption(_publisherNameOption);
        options.LicenseName = parseResult.GetValueForOption(_licenseNameOption);
        options.ModelName = parseResult.GetValueForOption(_optionalModelNameOption);

        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var service = context.GetService<IFoundryService>();
            var models = await service.ListModels(
                options.SearchForFreePlayground ?? false,
                options.PublisherName ?? "",
                options.LicenseName ?? "",
                options.ModelName ?? "",
                3,
                options.RetryPolicy);

            context.Response.Results = models?.Count > 0 ?
                ResponseResult.Create(
                    new ModelsListCommandResult(models),
                    FoundryJsonContext.Default.ModelsListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ModelsListCommandResult(IEnumerable<ModelInformation> Models);
}
