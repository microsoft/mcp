// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using NSubstitute;

namespace Azure.Mcp.Tools.Adme.Tests.TestSupport;

internal static class AzureServiceSubstitute
{
    /// <summary>
    /// Creates an <see cref="IAzureService"/> substitute whose tenant resolution returns the input unchanged,
    /// matching how <see cref="IAzureService.ResolveTenantIdAsync"/> treats tenant IDs.
    /// </summary>
    public static IAzureService Create()
    {
        var azureService = Substitute.For<IAzureService>();
        azureService.ResolveTenantIdAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(call.ArgAt<string?>(0)));
        return azureService;
    }
}
