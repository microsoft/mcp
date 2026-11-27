// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Core;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

internal sealed class GoalAssignmentTestResponse(int status, IReadOnlyDictionary<string, string> headers) : Response
{
    public override int Status => status;
    public override string ReasonPhrase => "Test response";
    public override Stream? ContentStream { get; set; }
    public override string ClientRequestId { get; set; } = "test-request";
    public override void Dispose() { }
    protected override bool ContainsHeader(string name) => headers.ContainsKey(name);
    protected override IEnumerable<HttpHeader> EnumerateHeaders() => headers.Select(header => new HttpHeader(header.Key, header.Value));
    protected override bool TryGetHeader(string name, [NotNullWhen(true)] out string? value) => headers.TryGetValue(name, out value);
    protected override bool TryGetHeaderValues(string name, [NotNullWhen(true)] out IEnumerable<string>? values)
    {
        bool found = headers.TryGetValue(name, out string? value);
        values = found ? [value!] : null;
        return found;
    }
}
