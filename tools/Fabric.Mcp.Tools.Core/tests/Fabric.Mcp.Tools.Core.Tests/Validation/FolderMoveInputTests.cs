// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class FolderMoveInputTests()
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("private-invalid")]
    [InlineData("../private")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public static void Validate_RequiresNonemptyUuidsWithoutEchoingInput(string? value)
    {
        var validation = new ValidationResult();
        FolderMoveInput.Validate(value, FolderId, null, validation);
        Assert.Equal("--workspace-id must be a nonempty UUID.", Assert.Single(validation.Errors));
        validation = new();
        FolderMoveInput.Validate(WorkspaceId, value, null, validation);
        Assert.Equal("--folder-id must be a nonempty UUID.", Assert.Single(validation.Errors));
        if (value is not null)
        {
            validation = new();
            FolderMoveInput.Validate(WorkspaceId, FolderId, value, validation);
            Assert.StartsWith("--target-folder-id must be a nonempty UUID", Assert.Single(validation.Errors));
            Assert.DoesNotContain("private", validation.Errors[0]);
        }
    }

    [Theory]
    [InlineData("D")]
    [InlineData("B")]
    [InlineData("N")]
    public static void Validate_AcceptsUuidFormatsAndRejectsSelfTargetsByIdentity(string format)
    {
        var validation = new ValidationResult();
        var workspace = Guid.Parse(WorkspaceId).ToString(format).ToUpperInvariant();
        var folder = Guid.Parse(FolderId).ToString(format).ToUpperInvariant();
        FolderMoveInput.Validate(workspace, folder, null, validation);
        FolderMoveInput.Validate(workspace, folder, Guid.Parse(ParentFolderId).ToString(format), validation);
        Assert.True(validation.IsValid);

        FolderMoveInput.Validate(workspace, folder, FolderId, validation);
        Assert.Equal("--target-folder-id cannot identify the folder being moved.", Assert.Single(validation.Errors));
    }
}
