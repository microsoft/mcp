// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Query;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Query;

public class IoTHubQueryFieldDiscovererTests
{
    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Fact]
    public void Discover_ExcludesFieldPathsTheCompilerWouldReject()
    {
        var twin = Parse(
            """
            {
                "deviceId": "device-1",
                "tags": {
                    "env": "prod",
                    "bad-tag": "x",
                    "location group": "west"
                },
                "properties": {
                    "desired": {
                        "firmware_version": "1.0.0",
                        "target-version": "2.0.0"
                    },
                    "reported": {
                        "temperature": 42,
                        "temp-sensor": 42,
                        "nested": {
                            "ok_child": true,
                            "bad-child": false
                        },
                        "bad-parent": {
                            "child": 1
                        }
                    }
                }
            }
            """);

        var result = IoTHubQueryFieldDiscoverer.Discover([twin]);

        // Valid names (letters, digits, underscore) are surfaced.
        Assert.Contains("deviceId", result.Device.Select(f => f.Field));
        Assert.Contains("env", result.Tags.Select(f => f.Field));
        Assert.Contains("firmware_version", result.Desired.Select(f => f.Field));
        Assert.Contains("temperature", result.Reported.Select(f => f.Field));
        Assert.Contains("nested.ok_child", result.Reported.Select(f => f.Field));

        // Names with characters outside the compiler's allowed set are excluded.
        Assert.DoesNotContain("bad-tag", result.Tags.Select(f => f.Field));
        Assert.DoesNotContain("location group", result.Tags.Select(f => f.Field));
        Assert.DoesNotContain("target-version", result.Desired.Select(f => f.Field));
        Assert.DoesNotContain("temp-sensor", result.Reported.Select(f => f.Field));
        Assert.DoesNotContain("nested.bad-child", result.Reported.Select(f => f.Field));
        Assert.DoesNotContain("bad-parent.child", result.Reported.Select(f => f.Field));
    }

    [Fact]
    public void Discover_EmitsOnlyCompilableFieldPaths()
    {
        var twin = Parse(
            """
            {
                "deviceId": "device-1",
                "status": "enabled",
                "tags": { "env": "prod", "bad-tag": 1, "region-code": "us" },
                "properties": {
                    "desired": { "interval": 30, "bad key": true },
                    "reported": { "temperature": 42, "nested": { "child": 1 }, "bad-parent": { "x": 2 } }
                }
            }
            """);

        var result = IoTHubQueryFieldDiscoverer.Discover([twin]);

        AssertAllCompilable(result.Device, PredicateScope.Device);
        AssertAllCompilable(result.Tags, PredicateScope.Tags);
        AssertAllCompilable(result.Desired, PredicateScope.Desired);
        AssertAllCompilable(result.Reported, PredicateScope.Reported);
    }

    private static void AssertAllCompilable(IEnumerable<QueryDiscoveredField> fields, PredicateScope scope)
    {
        foreach (var field in fields)
        {
            Assert.True(
                IoTHubQueryCompiler.IsValidFieldPath(field.Field),
                $"Discovered field '{field.Field}' is not a valid field path.");

            var request = new QueryCompileRequest
            {
                From = "devices",
                Filters =
                [
                    new QueryPredicate
                    {
                        Scope = scope,
                        Field = field.Field,
                        Operator = PredicateOperator.Equals,
                        Value = Parse("\"x\"")
                    }
                ]
            };

            // Compiling any discovered field must not throw.
            var query = IoTHubQueryCompiler.Compile(request);
            Assert.Contains(field.Field, query);
        }
    }
}
