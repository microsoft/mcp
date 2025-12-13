// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.FileShares.LiveTests;

public class FileSharesCommandTests
{
    private readonly IServiceProvider _serviceProvider;

    public FileSharesCommandTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IFileSharesService, FileSharesService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ListFileShares_WithValidSubscription_ReturnsFileShares()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IFileSharesService>();
        var subscription = "00000000-0000-0000-0000-000000000000";

        // Act
        var result = await service.ListFileShares(subscription, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetFileShare_WithValidName_ReturnsFileShare()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IFileSharesService>();
        var subscription = "00000000-0000-0000-0000-000000000000";
        var resourceGroup = "test-rg";
        var name = "testshare";

        // Act
        var result = await service.GetFileShare(
            subscription,
            resourceGroup,
            name,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(name, result.Name);
    }

    [Fact]
    public async Task CheckNameAvailability_WithValidName_ReturnsAvailability()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IFileSharesService>();
        var subscription = "00000000-0000-0000-0000-000000000000";
        var location = "eastus";
        var name = "testshare";

        // Act
        var result = await service.CheckNameAvailability(
            subscription,
            location,
            name,
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
    }
}
