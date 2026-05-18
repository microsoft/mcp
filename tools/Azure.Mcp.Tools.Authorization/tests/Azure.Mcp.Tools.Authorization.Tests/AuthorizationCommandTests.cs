// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.Tests;


public class AuthorizationCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    public async Task Should_list_role_assignments()
    {
        //
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
        
        // Defined in test-resources.bicep
        var expectedRoleAssignment = "/providers/Microsoft.Authorization/RoleDefinitions/acdd72a7-3385-48ef-bd42-f606fba81ae7";

        while (enumerator.MoveNext() && !testRoleAssignmentFound)
        {
            var roleAssignment = enumerator.Current;
            var roleDefinitionId = roleAssignment.AssertProperty("roleDefinitionId").GetString();
            testRoleAssignmentFound = expectedRoleAssignment.Equals(roleDefinitionId, StringComparison.Ordinal);

            var actualScope = roleAssignment.AssertProperty("scope").GetString();
            Assert.Equal(scope, actualScope);
        }

        Assert.True(testRoleAssignmentFound, "Test role assignment not found in the list of role assignments.");
    }
}

