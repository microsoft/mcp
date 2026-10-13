// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class OperationTestStream(byte[] bytes, Func<int, CancellationToken, ValueTask>? afterRead = null)
    : MemoryStream(bytes, writable: false)
{
    internal int BytesRead { get; private set; }
    internal bool Disposed { get; private set; }
    public override bool CanSeek => false;

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var count = await base.ReadAsync(buffer, cancellationToken);
        BytesRead += count;
        if (afterRead is not null)
        {
            await afterRead(count, cancellationToken);
        }
        return count;
    }

    protected override void Dispose(bool disposing)
    {
        Disposed = true;
        base.Dispose(disposing);
    }
}
