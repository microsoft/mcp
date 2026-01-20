// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.AzureMigrate.Options;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;

/// <summary>
/// Command to get platform landing zone modification guidance and recommendations.
/// </summary>
public sealed class GetModificationGuidanceCommand(
    ILogger<GetModificationGuidanceCommand> logger)
    : BaseAzureMigrateCommand<GetModificationGuidanceOptions>()
{
    private const string CommandTitle = "Get Platform Landing Zone Modification Guidance";
    private readonly ILogger<GetModificationGuidanceCommand> _logger = logger;

    /// <inheritdoc/>
    public override string Id => "d4e8c9b2-5f3a-4d1c-8b7e-2a9f1c6d5e4b";

    /// <inheritdoc/>
    public override string Name => "getmodificationguidance";

    /// <inheritdoc/>
    public override string Description =>
        """
        This tool should not be used to generate new landing zones from scratch.
        It is specifically designed to help you MODIFY existing Azure Landing Zone (ALZ) configurations
        Modifies the configuration files in your workspace, including:
        - Service configuration: DDoS protection, Bastion, DNS zones, Virtual Network Gateways, Azure Monitor Agent (AMA), Microsoft Defender, AMBA alerts
        - Policy management: changing enforcement modes, disabling/removing policy assignments, policy customization
        - Resource naming: updating prefixes, suffixes, and naming patterns
        - Network topology: IP address ranges, regions, CIDR blocks
        - Identity and governance: management groups, Zero Trust, Sovereign Landing Zone (SLZ) controls
        - Starter module options and tfvars customization
        
        Works with both enabling AND disabling services. Fetches official Azure Landing Zone documentation from GitHub and cross-references 
        policy definitions across 11 archetype files. Provides exact file paths and configuration changes needed.
        
        Examples: "enable DDoS protection", "turn off Bastion", "disable Enable-DDoS-VNET policy", "change IP ranges", 
        "customize resource names", "implement zero trust"
        
        **IMPORTANT**: Always pass the user's COMPLETE question/request as the 'topic' parameter. Do not leave it empty.
        
        Parameter:
        - topic (required): Pass the user's complete question or modification request here
        """;

    /// <inheritdoc/>
    public override string Title => CommandTitle;

    /// <inheritdoc/>
    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = true,
        Secret = false
    };

    /// <inheritdoc/>
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureMigrateOptionDefinitions.Topic);
    }

    /// <inheritdoc/>
    protected override GetModificationGuidanceOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Topic = parseResult.GetValueOrDefault(AzureMigrateOptionDefinitions.Topic) ?? string.Empty;
        return options;
    }

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);
        var question = options.Topic?.Trim() ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(question) && parseResult.UnmatchedTokens != null && parseResult.UnmatchedTokens.Any())
        {
            question = string.Join(" ", parseResult.UnmatchedTokens).Trim();
            _logger.LogInformation("Extracted question from unmatched tokens: {Question}", question);
        }

        try
        {
            var service = context.GetService<IPlatformLandingZoneGuidanceService>();
            var guidance = await service.GetModificationGuidanceAsync(question, cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new GetModificationGuidanceCommandResult(guidance),
                AzureMigrateJsonContext.Default.GetModificationGuidanceCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating guidance for: {Question}", question);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record GetModificationGuidanceCommandResult(
        [property: JsonPropertyName("guidance")] string Guidance);
}
