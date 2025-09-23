// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Services;

public class MySqlServiceTests
{
    private readonly IResourceGroupService _resourceGroupService;
    private readonly ITenantService _tenantService;
    private readonly ILogger<MySqlService> _logger;
    private readonly MySqlService _mysqlService;

    public MySqlServiceTests()
    {
        _resourceGroupService = Substitute.For<IResourceGroupService>();
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<MySqlService>>();

        _mysqlService = new MySqlService(_resourceGroupService, _tenantService, _logger);
    }

    [Fact]
    public void Constructor_WithNullResourceGroupService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MySqlService(null!, _tenantService, _logger));
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var service = new MySqlService(_resourceGroupService, _tenantService, _logger);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListServersAsync_WhenResourceGroupServiceThrows_RethrowsException()
    {
        var exception = new InvalidOperationException("Resource group not found");
        _resourceGroupService.GetResourceGroupResource("sub123", "rg1").ThrowsAsync(exception);

        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mysqlService.ListServersAsync("sub123", "rg1", "user1"));

        Assert.Equal(exception, thrownException);
    }
}
