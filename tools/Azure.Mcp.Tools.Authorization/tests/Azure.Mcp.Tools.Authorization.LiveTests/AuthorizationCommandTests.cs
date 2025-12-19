// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.LiveTests;


public class AuthorizationCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    [Fact]
    public async Task Should_list_role_assignments()
    {
        var resourceGroupName = RegisterOrRetrieveVariable("resourceGroupName", Settings.ResourceGroupName);
        var scope = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{resourceGroupName}";
        var result = await CallToolAsync(
            "role_assignment_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "scope", scope }
            });

        var roleAssignmentsArray = result.AssertProperty("assignments");
        Assert.Equal(JsonValueKind.Array, roleAssignmentsArray.ValueKind);

        var enumerator = roleAssignmentsArray.EnumerateArray();
        Assert.NotEmpty(enumerator);

        var testRoleAssignmentFound = false;
        var expectedDescription = "Role assignment for azmcp test"; // Defined in ./infra/services/authorization.bicep
        while (enumerator.MoveNext() && !testRoleAssignmentFound)
        {
            var roleAssignment = enumerator.Current;
            var description = roleAssignment.AssertProperty("description").GetString();
            testRoleAssignmentFound = expectedDescription.Equals(description, StringComparison.Ordinal);
        }
        Assert.True(testRoleAssignmentFound, "Test role assignment not found in the list of role assignments.");
    }
}

