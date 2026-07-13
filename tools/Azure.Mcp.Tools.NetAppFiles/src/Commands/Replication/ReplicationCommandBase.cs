// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Option;
using System.Net;
using System.Diagnostics.CodeAnalysis;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Replication;

public abstract class ReplicationCommandBase<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions> : SubscriptionCommand<TOptions>
    where TOptions : BaseReplicationOptions, new()
{
    protected void RegisterReplicationOptions(Command command, bool includeNoWait)
    {
        command.Options.Add(NetAppFilesOptionDefinitions.Account.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Pool.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Volume.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.Ids.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.AcquirePolicyToken.AsOptional());
        command.Options.Add(NetAppFilesOptionDefinitions.ChangeReference.AsOptional());

        if (includeNoWait)
        {
            command.Options.Add(NetAppFilesOptionDefinitions.NoWait.AsOptional());
        }
    }

    protected TOptions BindReplicationOptions(ParseResult parseResult, TOptions options)
    {
        options.Account = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Account.Name);
        options.Pool = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Pool.Name);
        options.Volume = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.Volume.Name);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Ids = parseResult.GetValueOrDefault<string[]>(NetAppFilesOptionDefinitions.Ids.Name);
        options.AcquirePolicyToken = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.AcquirePolicyToken.Name);
        options.ChangeReference = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.ChangeReference.Name);

        if (options is BaseReplicationActionOptions actionOptions)
        {
            actionOptions.NoWait = parseResult.GetValueOrDefault<bool>(NetAppFilesOptionDefinitions.NoWait.Name);
        }

        return options;
    }

    protected static void ValidateVolumeTarget(BaseReplicationOptions options)
    {
        if (options.Ids is { Length: > 0 })
        {
            return;
        }

        var missingOptions = new List<string>();
        if (string.IsNullOrWhiteSpace(options.Account)) missingOptions.Add("--account");
        if (string.IsNullOrWhiteSpace(options.Pool)) missingOptions.Add("--pool");
        if (string.IsNullOrWhiteSpace(options.Volume)) missingOptions.Add("--volume");
        if (string.IsNullOrWhiteSpace(options.ResourceGroup)) missingOptions.Add("--resource-group");

        if (missingOptions.Count > 0)
        {
            throw new ArgumentException($"Provide either --ids or all of the following arguments: {string.Join(", ", missingOptions)}.");
        }
    }

    protected static void ValidateUnsupportedCommonOptions(BaseReplicationOptions options)
    {
        if (options.AcquirePolicyToken)
        {
            throw new ArgumentException("The --acquirePolicyToken argument is not supported by this command yet.");
        }

        if (!string.IsNullOrWhiteSpace(options.ChangeReference))
        {
            throw new ArgumentException("The --changeReference argument is not supported by this command yet.");
        }
    }

    protected static void ValidateUnsupportedActionOptions(BaseReplicationActionOptions options)
    {
        if (options.NoWait)
        {
            throw new ArgumentException("The --no-wait argument is not supported by this command yet.");
        }

        ValidateUnsupportedCommonOptions(options);
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argumentException => argumentException.Message,
        RequestFailedException requestFailedException => requestFailedException.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        RequestFailedException requestFailedException => (HttpStatusCode)requestFailedException.Status,
        _ => base.GetStatusCode(ex)
    };
}