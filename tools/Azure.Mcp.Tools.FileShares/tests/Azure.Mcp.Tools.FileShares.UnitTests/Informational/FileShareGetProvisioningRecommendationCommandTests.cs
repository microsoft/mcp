// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Commands.Informational;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.FileShares.UnitTests.Informational;

/// <summary>
/// Unit tests for FileShareGetProvisioningRecommendationCommand.
/// </summary>
public class FileShareGetProvisioningRecommendationCommandTests
{
    private readonly IFileSharesService _service;
    private readonly ILogger<FileShareGetProvisioningRecommendationCommand> _logger;
    private readonly FileShareGetProvisioningRecommendationCommand _command;

    public FileShareGetProvisioningRecommendationCommandTests()
    {
        _service = Substitute.For<IFileSharesService>();
        _logger = Substitute.For<ILogger<FileShareGetProvisioningRecommendationCommand>>();
        _command = new(_logger, _service);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.NotNull(command);
        Assert.Equal("rec", command.Name);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        Assert.Equal("rec", _command.Name);
    }

    [Fact]
    public void Title_ReturnsCorrectValue()
    {
        Assert.Equal("Get File Share Provisioning Recommendation", _command.Title);
    }
}
