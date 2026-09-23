// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Core;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

internal sealed class GoalAssignmentTestResponse(int status, string header, string value) : Response
{
    public override int Status => status;
    public override string ReasonPhrase => string.Empty;
    public override Stream? ContentStream { get; set; }
    public override string ClientRequestId { get; set; } = string.Empty;
    public override void Dispose() { }
    protected override bool ContainsHeader(string name) => name.Equals(header, StringComparison.OrdinalIgnoreCase);
    protected override IEnumerable<HttpHeader> EnumerateHeaders() => [new(header, value)];
    protected override bool TryGetHeader(string name, out string result)
    {
        result = value;
        return ContainsHeader(name);
    }
    protected override bool TryGetHeaderValues(string name, out IEnumerable<string> values)
    {
        values = [value];
        return ContainsHeader(name);
    }
}
