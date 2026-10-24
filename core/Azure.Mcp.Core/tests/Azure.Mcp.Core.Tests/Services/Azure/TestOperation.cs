// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;

namespace Azure.Mcp.Core.Tests.Services.Azure;

internal sealed class TestOperation<T>(
    T value,
    Response response,
    int updatesBeforeCompletion,
    Func<CancellationToken, Task>? beforeUpdate = null) : Operation<T> where T : notnull
{
    private bool _hasCompleted;

    public int UpdateCount { get; private set; }

    public override string Id => "test-operation";

    public override T Value => value;

    public override bool HasValue => HasCompleted;

    public override bool HasCompleted => _hasCompleted;

    public override Response GetRawResponse() => response;

    public override Response UpdateStatus(CancellationToken cancellationToken = default) =>
        UpdateStatusAsync(cancellationToken).AsTask().GetAwaiter().GetResult();

    public override async ValueTask<Response> UpdateStatusAsync(CancellationToken cancellationToken = default)
    {
        UpdateCount++;

        if (beforeUpdate is not null)
        {
            await beforeUpdate(cancellationToken);
        }

        _hasCompleted = UpdateCount >= updatesBeforeCompletion;
        return response;
    }
}
