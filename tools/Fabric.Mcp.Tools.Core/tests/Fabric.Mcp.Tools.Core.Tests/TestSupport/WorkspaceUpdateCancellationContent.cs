// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class WorkspaceUpdateCancellationContent(CancellationTokenSource cancellation) : HttpContent
{
    internal bool IsDisposed { get; private set; }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        throw new InvalidOperationException("The cancellation-aware overload must be used.");

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        cancellation.Cancel();
        return Task.FromCanceled(cancellationToken);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        IsDisposed |= disposing;
        base.Dispose(disposing);
    }
}
