// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Core;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

internal sealed class GoalAssignmentTestResponse : Response
{
    private readonly int _status;
    private readonly IReadOnlyDictionary<string, string> _headers;

    internal GoalAssignmentTestResponse(int status, string header, string value)
        : this(status, new Dictionary<string, string> { [header] = value })
    {
    }

    internal GoalAssignmentTestResponse(int status, IReadOnlyDictionary<string, string> headers)
    {
        _status = status;
        _headers = headers;
    }

    public override int Status => _status;
    public override string ReasonPhrase => "Test response";
    public override Stream? ContentStream { get; set; }
    public override string ClientRequestId { get; set; } = "test-request";
    public override void Dispose() { }
    protected override bool ContainsHeader(string name) => _headers.ContainsKey(name);
    protected override IEnumerable<HttpHeader> EnumerateHeaders() =>
        _headers.Select(header => new HttpHeader(header.Key, header.Value));
    protected override bool TryGetHeader(string name, [NotNullWhen(true)] out string? value) =>
        _headers.TryGetValue(name, out value);
    protected override bool TryGetHeaderValues(
        string name,
        [NotNullWhen(true)] out IEnumerable<string>? values)
    {
        bool found = _headers.TryGetValue(name, out string? value);
        values = found ? [value!] : null;
        return found;
    }
}
