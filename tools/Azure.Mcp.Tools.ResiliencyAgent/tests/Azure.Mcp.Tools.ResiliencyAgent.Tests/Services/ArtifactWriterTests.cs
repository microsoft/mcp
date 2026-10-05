// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Models;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests.Services;

public sealed class ArtifactWriterTests : IDisposable
{
    private readonly string _workingDirectory;
    private readonly string _originalDirectory;
    private readonly ArtifactWriter _writer = new(Substitute.For<ILogger<ArtifactWriter>>());

    public ArtifactWriterTests()
    {
        _originalDirectory = Environment.CurrentDirectory;
        _workingDirectory = Path.Combine(Path.GetTempPath(), "resiliency-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workingDirectory);
        Environment.CurrentDirectory = _workingDirectory;
    }

    [Fact]
    public void Write_PutsContentInAFileUnderTheWorkingDirectory()
    {
        var artifact = new AgentArtifact("main.bicep", "template", "text/plain", "param a string", "bicep");

        var written = _writer.Write([artifact], "conv-1");

        var only = Assert.Single(written);
        Assert.Equal("main.bicep", only.Name);
        Assert.Equal("bicep", only.Format);
        Assert.True(File.Exists(only.Path));
        Assert.Equal("param a string", File.ReadAllText(only.Path));
        Assert.StartsWith(_workingDirectory, only.Path, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            ArtifactWriter.CreateMarkdownLink(only.Name, only.Path),
            only.MarkdownLink);
    }

    [Theory]
    [InlineData("../../escape.txt")]
    [InlineData("..\\..\\escape.txt")]
    [InlineData("/etc/passwd")]
    [InlineData("C:\\Windows\\System32\\evil.txt")]
    public void Write_KeepsAgentSuppliedNamesInsideTheOutputFolder(string hostileName)
    {
        // Artifact names arrive from a remote service and are treated as untrusted input.
        var artifact = new AgentArtifact(hostileName, null, "text/plain", "content", null);

        var written = _writer.Write([artifact], "conv-1");

        var only = Assert.Single(written);
        Assert.StartsWith(_workingDirectory, only.Path, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("..", only.Path, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("..")]
    [InlineData("\t \u2003--..  ")]
    public void Write_SkipsArtifactsWithAnUnusableName(string name)
    {
        var artifact = new AgentArtifact(name, null, "text/plain", "content", null);

        Assert.Empty(_writer.Write([artifact], "conv-1"));
    }

    [Fact]
    public void Write_SkipsArtifactsWithNoContent()
    {
        var artifact = new AgentArtifact("empty.bicep", null, "text/plain", null, "bicep");

        Assert.Empty(_writer.Write([artifact], "conv-1"));
    }

    [Theory]
    [InlineData("posture-report", "text/markdown", null, "posture-report.md")]
    [InlineData("posture-report.json", "text/markdown", null, "posture-report.md")]
    [InlineData("corrected", "text/plain", "bicep", "corrected.bicep")]
    [InlineData("corrected.txt", "text/plain", "bicep", "corrected.bicep")]
    [InlineData("corrected", "text/markdown", "bicep", "corrected.bicep")]
    [InlineData("already.bicep", "text/plain", "bicep", "already.bicep")]
    public void Write_AddsAUsefulExtension(
        string name,
        string mimeType,
        string? format,
        string expectedName)
    {
        var artifact = new AgentArtifact(name, null, mimeType, "content", format);

        var written = Assert.Single(_writer.Write([artifact], "conv-1"));

        Assert.Equal(expectedName, written.Name);
        Assert.True(File.Exists(written.Path));
    }

    [Theory]
    [InlineData(
        "Zone-Resilient Posture Report",
        "text/markdown",
        null,
        "Zone-Resilient-Posture-Report.md")]
    [InlineData(
        "  report\t \u2003name.md  ",
        "text/markdown",
        null,
        "report-name.md")]
    [InlineData(
        "already-safe_name.v1.md",
        "text/markdown",
        null,
        "already-safe_name.v1.md")]
    public void Write_NormalizesWhitespaceWithoutChangingContent(
        string name,
        string mimeType,
        string? format,
        string expectedName)
    {
        const string content = "content remains exactly the same";
        var artifact = new AgentArtifact(name, null, mimeType, content, format);

        var written = Assert.Single(_writer.Write([artifact], "conv-1"));

        Assert.Equal(expectedName, written.Name);
        Assert.Equal(content, File.ReadAllText(written.Path));
        Assert.Equal(
            ArtifactWriter.CreateMarkdownLink(expectedName, written.Path),
            written.MarkdownLink);
        Assert.DoesNotContain(written.Name, character => char.IsWhiteSpace(character));
    }

    [Fact]
    public void Write_DeduplicatesNamesAfterWhitespaceNormalization()
    {
        var first = new AgentArtifact("Zone Report.md", null, "text/markdown", "first", null);
        var second = new AgentArtifact("Zone\t \u2003Report.md", null, "text/markdown", "second", null);

        var written = _writer.Write([first, second], "conv-1");

        Assert.Collection(
            written,
            artifact =>
            {
                Assert.Equal("Zone-Report.md", artifact.Name);
                Assert.Equal("first", File.ReadAllText(artifact.Path));
            },
            artifact =>
            {
                Assert.Equal("Zone-Report-2.md", artifact.Name);
                Assert.Equal("second", File.ReadAllText(artifact.Path));
            });
    }

    [Fact]
    public void Write_DeduplicatesArtifactNamesWithoutOverwritingContent()
    {
        var first = new AgentArtifact("main.bicep", null, "text/plain", "param first string", "bicep");
        var second = new AgentArtifact("main.bicep", null, "text/plain", "param second string", "bicep");

        var written = _writer.Write([first, second], "conv-1");

        Assert.Collection(
            written,
            artifact =>
            {
                Assert.Equal("main.bicep", artifact.Name);
                Assert.Equal("param first string", File.ReadAllText(artifact.Path));
            },
            artifact =>
            {
                Assert.Equal("main-2.bicep", artifact.Name);
                Assert.Equal("param second string", File.ReadAllText(artifact.Path));
            });
    }

    [Theory]
    [InlineData(
        "main.bicep",
        "main.bicep",
        "main.bicep")]
    [InlineData(
        "Zone-Resilient Posture Report.md",
        "Zone-Resilient Posture Report.md",
        "Zone-Resilient Posture Report.md")]
    [InlineData(
        "Report (final) [1] #demo.md",
        "Report (final) [1] #demo.md",
        @"Report \(final\) \[1\] \#demo.md")]
    [InlineData(
        "![open](unexpected)#.md",
        "![open](unexpected)#.md",
        @"\!\[open\]\(unexpected\)\#.md")]
    public void CreateMarkdownLink_EscapesLabelAndFileUri(
        string displayName,
        string fileName,
        string expectedLabel)
    {
        string path = Path.Combine(_workingDirectory, "azure-resiliency", fileName);
        string expectedUri = new Uri(Path.GetFullPath(path)).AbsoluteUri
            .Replace("(", "%28", StringComparison.Ordinal)
            .Replace(")", "%29", StringComparison.Ordinal)
            .Replace("[", "%5B", StringComparison.Ordinal)
            .Replace("]", "%5D", StringComparison.Ordinal);

        Assert.Equal(
            $"[{expectedLabel}]({expectedUri})",
            ArtifactWriter.CreateMarkdownLink(displayName, path));
    }

    [Fact]
    public void CreateMarkdownLink_UsesWindowsFileUriForWindowsAbsolutePath()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        const string path = @"C:\workspace\azure-resiliency\main.bicep";

        Assert.Equal(
            "[main.bicep](file:///C:/workspace/azure-resiliency/main.bicep)",
            ArtifactWriter.CreateMarkdownLink("main.bicep", path));
    }

    public void Dispose()
    {
        Environment.CurrentDirectory = _originalDirectory;
        try
        {
            Directory.Delete(_workingDirectory, recursive: true);
        }
        catch (IOException)
        {
            // A leftover temp directory is not worth failing a test run over.
        }
    }
}
