// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Authorization.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.Tests;

public sealed class AuthorizationServiceTests
{
    [Fact]
    public async Task ListRoleAssignmentsAsync_RejectsSubscription_ForManagementGroupScope()
    {
        var service = new AuthorizationService(Substitute.For<IAzureService>());

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ListRoleAssignmentsAsync(
                "00000000-0000-0000-0000-000000000001",
                "/providers/Microsoft.Management/managementGroups/mg-contoso",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal("subscription", exception.ParamName);
    }
}
