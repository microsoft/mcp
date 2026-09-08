// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.FileShares.Commands;

/// <summary>
/// Base command for Azure File Shares operations. The Microsoft.FileShares resource provider is a new
/// ARM resource provider that is not registered in all clouds. This base class handles scenario where tool is called in a non supported cloud to ensure there is a clear error message.
/// </summary>
public abstract class BaseFileSharesCommand<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TOptions, TResult>(subscriptionResolver) where TOptions : class, ISubscriptionOption
{
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when IsResourceProviderUnavailable(reqEx) =>
            $"Azure File Shares (the Microsoft.FileShares resource provider) is not available in this cloud. Details: {reqEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx when IsResourceProviderUnavailable(reqEx) => HttpStatusCode.NotImplemented,
        _ => base.GetStatusCode(ex)
    };

    private static bool IsResourceProviderUnavailable(RequestFailedException ex) =>
        string.Equals(ex.ErrorCode, "InvalidResourceNamespace", StringComparison.OrdinalIgnoreCase);
}
