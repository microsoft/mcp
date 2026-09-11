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
