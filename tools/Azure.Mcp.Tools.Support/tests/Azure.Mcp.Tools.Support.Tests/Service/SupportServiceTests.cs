// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Support.Services;
using Xunit;

namespace Azure.Mcp.Tools.Support.Tests.Service;

public class SupportServiceTests
{
    [Fact]
    public void GetServiceIdFromProblemClassificationId_ReturnsServicePrefix()
    {
        var serviceId = SupportService.GetServiceIdFromProblemClassificationId(
            "/providers/Microsoft.Support/services/svc1/problemClassifications/pc1");

        Assert.Equal("/providers/Microsoft.Support/services/svc1", serviceId);
    }

    [Fact]
    public void GetServiceIdFromProblemClassificationId_Throws_WhenFormatInvalid()
    {
        Assert.Throws<ArgumentException>(() => SupportService.GetServiceIdFromProblemClassificationId("pc1"));
    }
}
