// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.Shortcut;

[CommandMetadata(
    Id = "a1b2c3d4-2001-4000-8000-000000000015",
    Name = "create_shortcut_s3_compatible",
    Title = "Create OneLake Shortcut (S3 Compatible Target)",
    Description = """
        Create a shortcut pointing to an S3-compatible storage location. Requires
        a connection ID, target URL, and bucket name. Requires OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutCreateS3CompatibleCommand(
    ILogger<ShortcutCreateS3CompatibleCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ShortcutCreateOptions>()
{
    private readonly ILogger<ShortcutCreateS3CompatibleCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutPath.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutName.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutConflictPolicy.AsOptional());
        command.Options.Add(FabricOptionDefinitions.TargetLocation.AsRequired());
        command.Options.Add(FabricOptionDefinitions.TargetSubpath.AsOptional());
        command.Options.Add(FabricOptionDefinitions.TargetConnectionId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.TargetBucket.AsRequired());
    }

    protected override ShortcutCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.Path = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutPath.Name);
        options.Name = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutName.Name);
        options.ConflictPolicy = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutConflictPolicy.Name);
        options.TargetLocation = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.TargetLocation.Name);
        options.TargetSubpath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.TargetSubpath.Name);
        options.TargetConnectionId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.TargetConnectionId.Name);
        options.TargetBucket = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.TargetBucket.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);
        try
        {
            var shortcut = new OneLakeShortcut
            {
                Path = options.Path,
                Name = options.Name,
                Target = new ShortcutTarget
                {
                    S3Compatible = new S3CompatibleShortcutTarget
                    {
                        Location = options.TargetLocation,
                        Subpath = options.TargetSubpath,
                        ConnectionId = options.TargetConnectionId,
                        Bucket = options.TargetBucket
                    }
                }
            };

            var result = await _oneLakeService.CreateShortcutAsync(options.WorkspaceId!, options.ItemId!, shortcut, options.ConflictPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeShortcut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating S3-compatible shortcut. Workspace: {Workspace}, Item: {Item}.", options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
