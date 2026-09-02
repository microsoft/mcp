// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests;

public sealed class AdmeServiceHelperTests
{
    [Theory]
    [InlineData(TestConstants.Endpoint)]
    [InlineData("https://sample.oep.ppe.azure-int.net")]
    public void ValidateEndpoint_AcceptsTrustedEndpoint(string endpoint)
    {
        var result = AdmeServiceHelper.ValidateEndpoint(new Uri(endpoint));

        Assert.Equal(endpoint, result.AbsoluteUri.TrimEnd('/'));
    }

}
