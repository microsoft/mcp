// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderCreateTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class FolderUpdateInputTests()
{
    [Fact]
    public static void Validate_UsesTheSameNameRulesAsCreationWithoutEchoingInput()
    {
        foreach (var name in InvalidNames.Append(null))
        {
            var validation = new ValidationResult();
            FolderUpdateInput.Validate(WorkspaceId, FolderId, name, validation);
            Assert.StartsWith("--display-name", Assert.Single(validation.Errors));
            Assert.DoesNotContain("Sales", validation.Errors[0]);
            var creation = new ValidationResult();
            FolderCreateInput.Validate(WorkspaceId, name, null, creation);
            Assert.Equal(creation.Errors, validation.Errors);
        }
    }

    [Theory]
    [InlineData("Sales")]
    [InlineData("Sales Team")]
    [InlineData("CON")]
    [InlineData("AUX")]
    [InlineData(@"Sales\Team")]
    [InlineData("$Planning")]
    [InlineData("Sales (2026) + [Q1]")]
    [InlineData("\u00e9\U0001F680")]
    [InlineData("e\u0301")]
    public static void Validate_AcceptsLogicalNamesWithoutFilesystemRules(string name)
    {
        var validation = new ValidationResult();
        FolderUpdateInput.Validate(WorkspaceId, FolderId, name, validation);
        Assert.True(validation.IsValid);
    }

    [Fact]
    public static void Validate_EnforcesUnicodeCharacterBoundaryWithoutSplittingSurrogates()
    {
        foreach (var name in new[] { new string('a', 255), new string('\u00e9', 255), string.Concat(Enumerable.Repeat("\U0001F680", 255)) })
        {
            var validation = new ValidationResult();
            FolderUpdateInput.Validate(WorkspaceId, FolderId, name, validation);
            Assert.True(validation.IsValid);
            FolderUpdateInput.Validate(WorkspaceId, FolderId, name + "a", validation);
            Assert.False(validation.IsValid);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("private-invalid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public static void Validate_RequiresBothNonemptyUuids(string? value)
    {
        var validation = new ValidationResult();
        FolderUpdateInput.Validate(value, FolderId, "Sales", validation);
        Assert.Equal("--workspace-id must be a nonempty UUID.", Assert.Single(validation.Errors));
        validation = new();
        FolderUpdateInput.Validate(WorkspaceId, value, "Sales", validation);
        Assert.Equal("--folder-id must be a nonempty UUID.", Assert.Single(validation.Errors));
    }
}
