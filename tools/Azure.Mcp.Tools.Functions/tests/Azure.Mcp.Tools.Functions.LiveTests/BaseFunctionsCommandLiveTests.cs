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
    /// <summary>
    /// Disable default sanitizer additions since Functions tests don't have
    /// Azure resources (no ResourceBaseName or SubscriptionId environment variables).
    /// Using empty strings in regex sanitizers causes test proxy 400 errors.
    /// </summary>
    public override bool EnableDefaultSanitizerAdditions => false;
}
