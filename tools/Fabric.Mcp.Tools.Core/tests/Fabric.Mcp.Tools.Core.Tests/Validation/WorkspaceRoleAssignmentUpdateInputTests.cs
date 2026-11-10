// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.WorkspaceRoleAssignmentTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class WorkspaceRoleAssignmentUpdateInputTests()
{
    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    [InlineData("X")]
    public static void CreateRequest_NormalizesOnlyResourceIdsAndPreservesExplicitRole(string format)
    {
        foreach (var role in new[] { "Admin", "Member", "Contributor", "Viewer", "FutureRole", " Viewer ", "Future\u00e9" })
        {
            var request = WorkspaceRoleAssignmentUpdateInput.CreateRequest(
                Guid.Parse(WorkspaceId).ToString(format).ToUpperInvariant(),
                Guid.Parse(AssignmentId).ToString(format).ToUpperInvariant(), role, TestContext.Current.CancellationToken);
            Assert.Equal(Guid.Parse(WorkspaceId), request.WorkspaceId);
            Assert.Equal(Guid.Parse(AssignmentId), request.AssignmentId);
            using var body = JsonDocument.Parse(request.Body);
            Assert.Equal("role", Assert.Single(body.RootElement.EnumerateObject()).Name);
            Assert.Equal(role, body.RootElement.GetProperty("role").GetString());
            Assert.Equal(new UpdateWorkspaceRoleAssignmentRequest(role),
                JsonSerializer.Deserialize(request.Body, CoreJsonContext.Default.UpdateWorkspaceRoleAssignmentRequest));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("private")]
    [InlineData("../private?role=Admin")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public static void CreateRequest_RejectsInvalidWorkspaceOrAssignmentIds(string? value)
    {
        var workspace = Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentUpdateInput.CreateRequest(value!, AssignmentId, "Viewer", TestContext.Current.CancellationToken));
        var assignment = Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentUpdateInput.CreateRequest(WorkspaceId, value!, "Viewer", TestContext.Current.CancellationToken));
        Assert.DoesNotContain("private", workspace.Message);
        Assert.DoesNotContain("private", assignment.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public static void CreateRequest_RequiresRoleRatherThanTreatingItAsNullableResponseMetadata(string? role) =>
        Assert.Throws<FabricItemInputException>(() =>
            WorkspaceRoleAssignmentUpdateInput.CreateRequest(WorkspaceId, AssignmentId, role!, TestContext.Current.CancellationToken));

    [Theory]
    [InlineData(0xd800)]
    [InlineData(0xdc00)]
    public static void CreateRequest_RejectsInvalidUnicodeWithoutReplacingRequestedAccess(int codeUnit) =>
        Assert.Throws<FabricItemInputException>(() => WorkspaceRoleAssignmentUpdateInput.CreateRequest(
            WorkspaceId, AssignmentId, new string((char)codeUnit, 1), TestContext.Current.CancellationToken));

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public static void CreateRequest_EnforcesExactSerializedRequestBudgetIncludingEscaping(int excess)
    {
        const int limit = 1024 * 1024;
        var overhead = Encoding.UTF8.GetByteCount(WorkspaceRoleAssignmentUpdateInput.CreateRequest(
            WorkspaceId, AssignmentId, "\u00e9", TestContext.Current.CancellationToken).Body);
        var role = "\u00e9" + new string('x', limit + excess - overhead);
        Assert.True(Encoding.UTF8.GetByteCount(role) < limit);
        if (excess == 0)
        {
            Assert.Equal(limit, Encoding.UTF8.GetByteCount(WorkspaceRoleAssignmentUpdateInput.CreateRequest(
                WorkspaceId, AssignmentId, role, TestContext.Current.CancellationToken).Body));
        }
        else
        {
            Assert.Throws<FabricItemInputException>(() => WorkspaceRoleAssignmentUpdateInput.CreateRequest(
                WorkspaceId, AssignmentId, role, TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static void CreateRequest_RejectsOversizedRoleBeforeSerialization(bool multiByte)
    {
        var role = multiByte ? new string('\u00e9', 524289) : new string('x', 1048577);
        Assert.Equal(multiByte ? 1048578 : 1048577, Encoding.UTF8.GetByteCount(role));
        Assert.Throws<FabricItemInputException>(() => WorkspaceRoleAssignmentUpdateInput.CreateRequest(
            WorkspaceId, AssignmentId, role, TestContext.Current.CancellationToken));
    }
}
