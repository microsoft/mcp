// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

/// <summary>
/// Unit tests for FileShareUpdateCommand.
/// </summary>
public class FileShareUpdateCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<FileShareUpdateCommand> _logger;
    private readonly FileShareUpdateCommand _command;

    public FileShareUpdateCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareUpdateCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("update", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("update", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Update File Share", _command.Title);
    }

    [Fact]
    public void RegisterOptions_IncludesMediaTierOption()
    {
        var command = _command.GetCommand();
        var option = command.Options.FirstOrDefault(o => o.Name == FileSharesOptionDefinitions.MediaTier.Name);
        Assert.NotNull(option);
    }

    [Fact]
    public void Description_MentionsMediaTier()
    {
        Assert.Contains("media tier", _command.Description, StringComparison.OrdinalIgnoreCase);
    }
}
