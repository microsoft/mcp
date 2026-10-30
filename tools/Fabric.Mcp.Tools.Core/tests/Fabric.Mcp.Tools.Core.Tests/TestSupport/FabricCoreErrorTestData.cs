// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Identity;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class FabricCoreErrorTestData
{
    internal const string PrivateDetails = @"private-backend-detail C:\private\FabricSecrets.cs:line 42 access-token=private-token";

    internal static Exception CreateException(string failure) => failure switch
    {
        "credentials" => new CredentialUnavailableException(PrivateDetails),
        "authentication" => new AuthenticationFailedException(PrivateDetails),
        "argument" => new ArgumentException(PrivateDetails),
        "configuration" => new InvalidOperationException(PrivateDetails),
        "json" => new JsonException(PrivateDetails),
        "timeout" => new TimeoutException(PrivateDetails),
        "task-canceled" => new TaskCanceledException(PrivateDetails),
        "operation-canceled" => new OperationCanceledException(PrivateDetails),
        "service" => new Azure.RequestFailedException(403, PrivateDetails),
        _ => new Exception(PrivateDetails)
    };

    internal static void AssertSanitized(CommandResponse response)
    {
        Assert.Null(response.Results);
        var json = JsonSerializer.Serialize(response, ModelsJsonContext.Default.CommandResponse);
        Assert.DoesNotContain("private", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FabricSecrets", json);
        Assert.DoesNotContain("Exception", json);
        Assert.DoesNotContain("stackTrace", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Fabric.Mcp.Tools.Core", json);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(["duration", "message", "status"], document.RootElement.EnumerateObject().Select(p => p.Name).Order());
    }
}
