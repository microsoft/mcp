// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class UnreadableHttpContent : HttpContent
{
    public bool IsDisposed { get; private set; }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        throw new InvalidOperationException("The assignment response body must not be read.");

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }
}
