// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace CopilotCliTester.Tests;

public sealed class PromptParserTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();

    public void Dispose() => File.Delete(_tempFile);

    [Fact]
    public void ParseFile_ValidMarkdown_ReturnsPrompts()
    {
        File.WriteAllText(_tempFile, """
            ## storage

            | Tool Name | Prompt |
            |-----------|--------|
            | storage_account_list | list my storage accounts |
            | storage_blob_get | get a blob from my container |

            ## keyvault

            | Tool Name | Prompt |
            |-----------|--------|
            | keyvault_secret_get | show secret from vault |
            """);

        var result = PromptParser.ParseFile(_tempFile);

        Assert.Equal(3, result.Count);

        Assert.Equal("storage", result[0].Section);
        Assert.Equal("storage_account_list", result[0].Tool);
        Assert.Equal("list my storage accounts", result[0].Prompt);
        Assert.Equal("storage", result[0].Namespace);

        Assert.Equal("storage", result[1].Section);
        Assert.Equal("storage_blob_get", result[1].Tool);

        Assert.Equal("keyvault", result[2].Section);
        Assert.Equal("keyvault_secret_get", result[2].Tool);
        Assert.Equal("keyvault", result[2].Namespace);
    }

    [Fact]
    public void ParseFile_SkipsHeaderAndDashRows()
    {
        File.WriteAllText(_tempFile, """
            ## section1

            | Tool Name | Prompt |
            |-----------|--------|
            | my_tool | do something |
            """);

        var result = PromptParser.ParseFile(_tempFile);

        Assert.Single(result);
        Assert.Equal("my_tool", result[0].Tool);
    }

    [Fact]
    public void ParseFile_EmptyFile_ReturnsEmpty()
    {
        File.WriteAllText(_tempFile, "");

        var result = PromptParser.ParseFile(_tempFile);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseFile_NoTableRows_ReturnsEmpty()
    {
        File.WriteAllText(_tempFile, """
            ## storage

            Some descriptive text but no table.
            """);

        var result = PromptParser.ParseFile(_tempFile);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseFile_PromptContainingPipe_IsParsedCorrectly()
    {
        File.WriteAllText(_tempFile, """
            ## storage

            | Tool Name | Prompt |
            |-----------|--------|
            | storage_account_list | show resources where tag | equals 'prod' |
            """);

        var result = PromptParser.ParseFile(_tempFile);

        Assert.Single(result);
        Assert.Equal(
            "show resources where tag | equals 'prod'",
            result[0].Prompt);
    }

    [Fact]
    public void ParseFile_RowBeforeAnySection_UsesEmptySection()
    {
        File.WriteAllText(_tempFile, "| my_tool | do something |\n");

        var result = PromptParser.ParseFile(_tempFile);

        Assert.Single(result);
        Assert.Equal(string.Empty, result[0].Section);
        Assert.Equal(string.Empty, result[0].Namespace);
    }

    [Fact]
    public void ParseNamespaces_ExtractsSectionHeaders()
    {
        File.WriteAllText(_tempFile, """
            ## storage

            Some text.

            ## keyvault

            More text.

            ## cosmos
            """);

        var result = PromptParser.ParseNamespaces(_tempFile);

        Assert.Equal(3, result.Count);
        Assert.Equal("storage", result[0]);
        Assert.Equal("keyvault", result[1]);
        Assert.Equal("cosmos", result[2]);
    }

    [Fact]
    public void ParseNamespaces_EmptyFile_ReturnsEmpty()
    {
        File.WriteAllText(_tempFile, "");

        var result = PromptParser.ParseNamespaces(_tempFile);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseNamespaces_NoSections_ReturnsEmpty()
    {
        File.WriteAllText(_tempFile, "Just some plain text\nwithout any headers.\n");

        var result = PromptParser.ParseNamespaces(_tempFile);

        Assert.Empty(result);
    }

    [Fact]
    public void ParseFile_ToolNameCaseInsensitiveHeaderSkip()
    {
        File.WriteAllText(_tempFile, """
            ## section

            | tool name | prompt |
            |-----------|--------|
            | real_tool | do it |
            """);

        var result = PromptParser.ParseFile(_tempFile);

        Assert.Single(result);
        Assert.Equal("real_tool", result[0].Tool);
    }
}
