// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Traces;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceCompareAttributesCommandTests()
    : SubscriptionCommandUnitTestsBase<TraceCompareAttributesCommand, IMonitorTracesService>
{
    private const string Target = "--subscription sub --resource-group rg --workspace amw --start-time 2026-01-02T00:00:00Z --end-time 2026-01-02T01:00:00Z";

    [Theory]
    [InlineData("--query Traces --compare-query Traces", true)]
    [InlineData("--compare-query Traces --compare-start-time 2026-01-01T00:00:00Z --compare-end-time 2026-01-01T01:00:00Z", false)]
    [InlineData("--query Traces --compare-query Traces --trace-ids 0123456789abcdef0123456789abcdef", false)]
    public async Task UsesIndependentAttributeEndpointAndOnlyItsOptions(string args, bool valid)
    {
        Service.ComparePopulationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), "attributes",
            Arg.Any<TraceComparisonRequest>(), null, Arg.Any<string?>(),
            Arg.Any<CancellationToken>()).Returns(new TraceQueryResult { Status = "Succeeded", OperationId = "operation" });
        var response = await ExecuteCommandAsync($"{Target} {args}");
        Assert.Equal(valid ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        Assert.Equal(valid ? 1 : 0, Service.ReceivedCalls().Count());
    }
}
