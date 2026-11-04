// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tests.Client;

public abstract class RecordedAzureToolTestsBase<TAreaSetup>(ITestOutputHelper output, TestProxyFixture fixture)
    : RecordedToolTestsBase(output, fixture) where TAreaSetup : IAreaSetup, new()
{
    protected override IAreaSetup Area { get; } = new TAreaSetup();

    protected override void ConfigureServices(IServiceCollection services)
        => services.AddAzureToolTestServices(TestMode);
}
