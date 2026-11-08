// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderCreateTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.FolderListTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class FolderCreateInputTests()
{
    [Fact]
    public static void Validate_RejectsDocumentedNameViolationsAndInvalidUnicodeWithoutEchoingNames()
    {
        foreach (var name in InvalidNames.Append(null))
        {
            var validation = new ValidationResult();
            FolderCreateInput.Validate(WorkspaceId, name, null, validation);
            Assert.False(validation.IsValid);
            Assert.StartsWith("--display-name", Assert.Single(validation.Errors));
            Assert.DoesNotContain("Sales", validation.Errors[0]);
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
    public static void Validate_AcceptsLogicalNamesWithoutHostFilesystemRules(string name)
    {
        var validation = new ValidationResult();
        FolderCreateInput.Validate(WorkspaceId, name, ParentFolderId, validation);
        Assert.True(validation.IsValid);
    }

    [Fact]
    public static void Validate_CountsUnicodeCharactersWithoutSplittingSurrogatePairs()
    {
        foreach (var name in new[] { new string('a', 255), new string('\u00e9', 255), string.Concat(Enumerable.Repeat("\U0001F680", 255)) })
        {
            var validation = new ValidationResult();
            FolderCreateInput.Validate(WorkspaceId, name, null, validation);
            Assert.True(validation.IsValid);
            FolderCreateInput.Validate(WorkspaceId, name + "a", null, validation);
            Assert.False(validation.IsValid);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("private-invalid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public static void Validate_RequiresNonemptyUuidContext(string? value)
    {
        var validation = new ValidationResult();
        FolderCreateInput.Validate(value, "Sales", ParentFolderId, validation);
        Assert.Equal("--workspace-id must be a nonempty UUID.", Assert.Single(validation.Errors));
        validation = new();
        FolderCreateInput.Validate(WorkspaceId, "Sales", value, validation);
        Assert.Equal(value is null, validation.IsValid);
    }
}
