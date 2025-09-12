// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Authorization.LiveTests;


public class AuthorizationCommandTests(ITestOutputHelper output)
    : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_list_role_assignments()
    {
        var scope = $"/subscriptions/{Settings.SubscriptionId}/resourceGroups/{Settings.ResourceGroupName}";
        var result = await CallToolAsync(
            "azmcp_role_assignment_list",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "scope", scope }
            });

        var roleAssignmentsArray = result.AssertProperty("Assignments");
        Assert.Equal(JsonValueKind.Array, roleAssignmentsArray.ValueKind);

        var enumerator = roleAssignmentsArray.EnumerateArray();
        Assert.NotEmpty(enumerator);

        var testRoleAssignmentFound = false;
        var expectedDescription = "Role assignment for azmcp test"; // Defined in ./infra/services/authorization.bicep
        while (enumerator.MoveNext() && !testRoleAssignmentFound)
        {
            var roleAssignment = enumerator.Current;
            var description = roleAssignment.AssertProperty("Description").GetString();
            testRoleAssignmentFound = expectedDescription.Equals(description, StringComparison.Ordinal);
        }
        Assert.True(testRoleAssignmentFound, "Test role assignment not found in the list of role assignments.");
    }
}

