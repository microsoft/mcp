// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class WorkspaceListCancellationStream(CancellationTokenSource cancellation) : MemoryStream
{
    public bool ReadStarted { get; private set; }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ReadStarted = true;
        cancellation.Cancel();
        return ValueTask.FromCanceled<int>(cancellationToken);
    }
}
