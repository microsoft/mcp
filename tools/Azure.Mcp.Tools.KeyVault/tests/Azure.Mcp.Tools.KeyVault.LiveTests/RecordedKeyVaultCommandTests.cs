// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Azure.Security.KeyVault.Keys;
using Xunit;

namespace Azure.Mcp.Tools.KeyVault.LiveTests;

public class RecordedKeyVaultCommandTests(ITestOutputHelper output, TestProxyFixture fixture) : RecordedCommandTestsBase(output, fixture)
{
    [Fact]
    public void Example()
    {
        Console.WriteLine("Example test to ensure test framework is working.");
    }
}
