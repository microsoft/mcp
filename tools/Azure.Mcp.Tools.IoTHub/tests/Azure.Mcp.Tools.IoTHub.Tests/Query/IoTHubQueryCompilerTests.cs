// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Query;
using Xunit;

namespace Azure.Mcp.Tools.IoTHub.Tests.Query;

public class IoTHubQueryCompilerTests
{
    private static JsonElement Val(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static QueryPredicate Predicate(PredicateScope scope, string field, PredicateOperator op, string valueJson) => new()
    {
        Scope = scope,
        Field = field,
        Operator = op,
        Value = Val(valueJson)
    };

    [Theory]
    [InlineData(PredicateScope.Device, "status", PredicateOperator.Equals, "\"enabled\"", "SELECT * FROM devices WHERE status = 'enabled'")]
    [InlineData(PredicateScope.Tags, "floor", PredicateOperator.Equals, "3", "SELECT * FROM devices WHERE tags.floor = 3")]
    [InlineData(PredicateScope.Desired, "interval", PredicateOperator.GreaterThanOrEqual, "30", "SELECT * FROM devices WHERE properties.desired.interval >= 30")]
    [InlineData(PredicateScope.Reported, "temperature", PredicateOperator.GreaterThan, "80", "SELECT * FROM devices WHERE properties.reported.temperature > 80")]
    [InlineData(PredicateScope.Reported, "connected", PredicateOperator.NotEquals, "true", "SELECT * FROM devices WHERE properties.reported.connected != true")]
    [InlineData(PredicateScope.Reported, "batteryLevel", PredicateOperator.LessThanOrEqual, "15", "SELECT * FROM devices WHERE properties.reported.batteryLevel <= 15")]
    [InlineData(PredicateScope.Reported, "batteryLevel", PredicateOperator.LessThan, "20", "SELECT * FROM devices WHERE properties.reported.batteryLevel < 20")]
    public void Compile_MapsScopeAndOperator(PredicateScope scope, string field, PredicateOperator op, string valueJson, string expected)
    {
        var request = new QueryCompileRequest { Filters = [Predicate(scope, field, op, valueJson)] };

        Assert.Equal(expected, IoTHubQueryCompiler.Compile(request));
    }

    [Fact]
    public void Compile_JoinsPredicatesWithAndByDefault()
    {
        var request = new QueryCompileRequest
        {
            Filters =
            [
                Predicate(PredicateScope.Tags, "floor", PredicateOperator.Equals, "3"),
                Predicate(PredicateScope.Reported, "temperature", PredicateOperator.GreaterThan, "80")
            ]
        };

        Assert.Equal("SELECT * FROM devices WHERE tags.floor = 3 AND properties.reported.temperature > 80", IoTHubQueryCompiler.Compile(request));
    }

    [Fact]
    public void Compile_JoinsPredicatesWithOrWhenRequested()
    {
        var request = new QueryCompileRequest
        {
            LogicalOperator = "OR",
            Filters =
            [
                Predicate(PredicateScope.Tags, "floor", PredicateOperator.Equals, "3"),
                Predicate(PredicateScope.Reported, "temperature", PredicateOperator.GreaterThan, "80")
            ]
        };

        Assert.Equal("SELECT * FROM devices WHERE tags.floor = 3 OR properties.reported.temperature > 80", IoTHubQueryCompiler.Compile(request));
    }

    [Fact]
    public void Compile_EscapesSingleQuotesInStringValues()
    {
        var request = new QueryCompileRequest { Filters = [Predicate(PredicateScope.Tags, "owner", PredicateOperator.Equals, "\"O'Brien\"")] };

        Assert.Equal("SELECT * FROM devices WHERE tags.owner = 'O''Brien'", IoTHubQueryCompiler.Compile(request));
    }

    [Fact]
    public void Compile_UsesRequestedSource()
    {
        var request = new QueryCompileRequest
        {
            From = "devices.modules",
            Filters = [Predicate(PredicateScope.Reported, "status", PredicateOperator.Equals, "\"running\"")]
        };

        Assert.Equal("SELECT * FROM devices.modules WHERE properties.reported.status = 'running'", IoTHubQueryCompiler.Compile(request));
    }

    [Fact]
    public void Compile_RejectsUnsupportedSource()
    {
        var request = new QueryCompileRequest
        {
            From = "servers",
            Filters = [Predicate(PredicateScope.Device, "status", PredicateOperator.Equals, "\"enabled\"")]
        };

        var ex = Assert.Throws<ArgumentException>(() => IoTHubQueryCompiler.Compile(request));
        Assert.Contains("Unsupported query source", ex.Message);
    }

    [Fact]
    public void Compile_RejectsUnsupportedLogicalOperator()
    {
        var request = new QueryCompileRequest
        {
            LogicalOperator = "XOR",
            Filters = [Predicate(PredicateScope.Device, "status", PredicateOperator.Equals, "\"enabled\"")]
        };

        var ex = Assert.Throws<ArgumentException>(() => IoTHubQueryCompiler.Compile(request));
        Assert.Contains("Unsupported logical operator", ex.Message);
    }

    [Fact]
    public void Compile_RejectsInvalidFieldPath()
    {
        var request = new QueryCompileRequest { Filters = [Predicate(PredicateScope.Reported, "temp-sensor", PredicateOperator.Equals, "1")] };

        var ex = Assert.Throws<ArgumentException>(() => IoTHubQueryCompiler.Compile(request));
        Assert.Contains("invalid field path", ex.Message);
    }

    [Fact]
    public void Compile_RejectsMissingValue()
    {
        var request = new QueryCompileRequest { Filters = [Predicate(PredicateScope.Reported, "temperature", PredicateOperator.Equals, "null")] };

        var ex = Assert.Throws<ArgumentException>(() => IoTHubQueryCompiler.Compile(request));
        Assert.Contains("missing a 'value'", ex.Message);
    }

    [Fact]
    public void Compile_RejectsNonPositiveTop()
    {
        var request = new QueryCompileRequest
        {
            Top = 0,
            Filters = [Predicate(PredicateScope.Device, "status", PredicateOperator.Equals, "\"enabled\"")]
        };

        var ex = Assert.Throws<ArgumentException>(() => IoTHubQueryCompiler.Compile(request));
        Assert.Contains("must be a positive integer", ex.Message);
    }

    [Fact]
    public void Compile_AcceptsFieldPresentInDiscoveredFields()
    {
        var request = new QueryCompileRequest
        {
            Filters = [Predicate(PredicateScope.Reported, "temperature", PredicateOperator.GreaterThan, "80")],
            DiscoveredFields = new QueryDiscoveredFields
            {
                Reported = [new QueryDiscoveredField("temperature", "number", [])]
            }
        };

        Assert.Equal("SELECT * FROM devices WHERE properties.reported.temperature > 80", IoTHubQueryCompiler.Compile(request));
    }

    [Fact]
    public void Compile_RejectsFieldMissingFromDiscoveredFields()
    {
        var request = new QueryCompileRequest
        {
            Filters = [Predicate(PredicateScope.Reported, "humidity", PredicateOperator.Equals, "50")],
            DiscoveredFields = new QueryDiscoveredFields
            {
                Reported = [new QueryDiscoveredField("temperature", "number", [])]
            }
        };

        var ex = Assert.Throws<ArgumentException>(() => IoTHubQueryCompiler.Compile(request));
        Assert.Contains("unknown field", ex.Message);
        Assert.Contains("temperature", ex.Message);
    }
}
