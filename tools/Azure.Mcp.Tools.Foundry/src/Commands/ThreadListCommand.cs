// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Thread;
using Azure.Mcp.Tools.Foundry.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Foundry.Commands;

public class ThreadListCommand : GlobalCommand<ThreadListOptions>
{
    private const string CommandTitle = "List Microsoft Foundry Agent Threads";
    public override string Id => "ec6ce496-cfae-45b6-8ab3-97fb55f861c8";
    public override string Name => "list";


    public override string Description =>
        """
            List Microsoft Foundry Agent Threads.
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
        command.Options.Add(FoundryOptionDefinitions.EndpointOption);
    }

    protected override ThreadListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EndpointOption);
        return options;
    }

    public override string Title => CommandTitle;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            IHttpClientFactory? httpClientFactory = null;
            try
            {
                httpClientFactory = context.GetService<IHttpClientFactory>();
            }
            catch (InvalidOperationException)
            {
                // IHttpClientFactory not registered - this is fine for production scenarios
            }

            var service = context.GetService<IFoundryService>();
            ThreadListResult result = await service.ListThreads(
                options.Endpoint!,
                options.Tenant,
                httpClientFactory: httpClientFactory,
                cancellationToken: cancellationToken
            );

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryJsonContext.Default.ThreadListResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
