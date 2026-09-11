// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Option;
using System.Net;
using System.Diagnostics.CodeAnalysis;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Replication;

public class ReplicationCommandHelpers
{
    public static void ValidateVolumeTarget(BaseReplicationOptions options)
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

    public static void ValidateUnsupportedCommonOptions(BaseReplicationOptions options)
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

    public static void ValidateUnsupportedActionOptions(BaseReplicationActionOptions options)
    {
        if (options.NoWait)
        {
            throw new ArgumentException("The --no-wait argument is not supported by this command yet.");
        }

        ReplicationCommandHelpers.ValidateUnsupportedCommonOptions(options);
    }
}