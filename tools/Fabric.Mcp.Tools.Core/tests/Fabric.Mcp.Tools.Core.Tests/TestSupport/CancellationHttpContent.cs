// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class CancellationHttpContent(CancellationTokenSource cancellation) : HttpContent
{
    public bool ObservedCancellation { get; private set; }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        throw new InvalidOperationException("Expected cancellation-aware content reading.");

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        cancellation.Cancel();
        ObservedCancellation = cancellationToken.IsCancellationRequested;
        return Task.FromCanceled(cancellationToken);
    }
}
