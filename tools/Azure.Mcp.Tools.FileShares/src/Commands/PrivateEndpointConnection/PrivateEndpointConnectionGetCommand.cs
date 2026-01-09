using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;

public sealed class PrivateEndpointConnectionGetCommand() : SubscriptionCommand<SubscriptionOptions>()
{
    public override string Id => "b3-e0e0e1-e2e3-e4e5-e6e7-e8e9eaebecea";
    public override string Name => "get";
    public override string Description => "";
    public override string Title => "";
    public override ToolMetadata Metadata => new();
    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken) => throw new NotImplementedException();
}

