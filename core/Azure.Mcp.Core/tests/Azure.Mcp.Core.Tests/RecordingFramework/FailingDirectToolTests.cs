// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Core.Tests.RecordingFramework;

internal sealed class FailingDirectToolTests(ITestOutputHelper output, string variableName)
    : DirectToolTestsBase(output)
{
    protected override IAreaSetup Area { get; } = new StorageSetup();

    protected override ValueTask LoadSettingsAsync()
    {
        TestMode = TestMode.Live;
        Settings = new LiveTestSettings
        {
            TestMode = TestMode.Live,
            EnvironmentVariables = new() { [variableName] = "temporary" }
        };
        return ValueTask.CompletedTask;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        Assert.Equal("temporary", Environment.GetEnvironmentVariable(variableName));
        throw new InvalidOperationException("test configuration failure");
    }
}
