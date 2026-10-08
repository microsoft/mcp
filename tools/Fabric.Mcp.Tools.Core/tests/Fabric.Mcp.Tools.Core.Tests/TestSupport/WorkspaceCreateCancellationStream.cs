// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class WorkspaceCreateCancellationStream(CancellationTokenSource cancellation)
    : MemoryStream(Encoding.UTF8.GetBytes(WorkspaceCreateTestData.MinimalMetadata))
{
    public bool IsDisposed { get; private set; }
    public CancellationToken ReadCancellationToken { get; private set; }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ReadCancellationToken = cancellationToken;
        cancellation.Cancel();
        return ValueTask.FromCanceled<int>(cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }
}
