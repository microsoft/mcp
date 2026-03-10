// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Thread;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

public sealed class ThreadListCommand : GlobalCommand<ThreadListOptions>
{
    private const string CommandTitle = "List Microsoft Foundry Threads";

    public override string Id => "e1f2a3b4-1234-f012-4567-890123456789";

    public override string Name => "list";

    public override string Description =>
        """
        Lists all threads in a Microsoft Foundry project using the Azure AI Agents service.
        Provide the project endpoint to retrieve all conversation threads associated with the project.
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
        command.Options.Add(FoundryExtensionsOptionDefinitions.EndpointOption.AsRequired());
    }

    protected override ThreadListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.EndpointOption.Name);
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
            var service = context.GetService<IFoundryExtensionsService>();

            var result = await service.ListThreads(
                options.Endpoint!,
                options.Tenant,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryExtensionsJsonContext.Default.ThreadListResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
