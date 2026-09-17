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

        var assignments = roleAssignmentsArray.EnumerateArray().ToList();
        Assert.NotEmpty(assignments);

        // The test infrastructure (./test-resources.bicep) creates a Reader role assignment for the
        // test application at the resource group scope with a known description.
        const string readerRoleDefinitionId = "acdd72a7-3385-48ef-bd42-f606fba81ae7"; // Built-in Reader role
        const string expectedDescription = "Role assignment for azmcp test";

        var testRoleAssignmentFound = false;
        foreach (var roleAssignment in assignments)
        {
            // Role assignments only include a 'description' when one was set, so check for its presence
            // rather than asserting it. Public Cloud surfaces the description via Azure Resource Graph;
            // match on it when available for precision.
            if (roleAssignment.TryGetProperty("description", out var descriptionElement)
                && descriptionElement.ValueKind == JsonValueKind.String
                && expectedDescription.Equals(descriptionElement.GetString(), StringComparison.Ordinal))
            {
                testRoleAssignmentFound = true;
                break;
            }

            // Azure Resource Graph does not surface the role assignment 'description' in some sovereign
            // clouds (e.g. Azure US Government). Fall back to identifying the bicep-created assignment by
            // its Reader role definition there.
            if (Settings.IsAzureUSGovernment
                && roleAssignment.TryGetProperty("roleDefinitionId", out var roleDefinitionElement)
                && roleDefinitionElement.GetString() is { } roleDefinitionId
                && roleDefinitionId.EndsWith(readerRoleDefinitionId, StringComparison.OrdinalIgnoreCase))
            {
                testRoleAssignmentFound = true;
                break;
            }
        }
        Assert.True(testRoleAssignmentFound, "Test role assignment not found in the list of role assignments.");
    }
}

