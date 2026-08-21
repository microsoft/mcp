// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppConfig.Services;
using Xunit;

namespace Azure.Mcp.Tools.AppConfig.Tests.Services;

public class AppConfigServiceTests
{
    [Theory]
    [InlineData(200, true)]
    [InlineData(204, false)]
    public void KeyValueExistedFromDeleteStatus_MapsKnownStatuses(int statusCode, bool expected)
    {
        Assert.Equal(expected, AppConfigService.KeyValueExistedFromDeleteStatus(statusCode));
    }
}
