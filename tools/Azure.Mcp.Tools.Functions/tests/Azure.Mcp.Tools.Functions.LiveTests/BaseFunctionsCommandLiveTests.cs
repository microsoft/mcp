// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Functions.LiveTests;

/// <summary>
/// Base class for Azure Functions MCP tool live tests.
/// These tests validate HTTP calls to GitHub and Azure CDN for template fetching.
/// </summary>
public abstract class BaseFunctionsCommandLiveTests(
    ITestOutputHelper output,
    TestProxyFixture fixture,
    LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    // No additional sanitizers needed for Functions tests since they don't
    // contain Azure resource-specific sensitive data like subscription IDs
    // or IP addresses. The base class sanitizers handle standard patterns.
}
