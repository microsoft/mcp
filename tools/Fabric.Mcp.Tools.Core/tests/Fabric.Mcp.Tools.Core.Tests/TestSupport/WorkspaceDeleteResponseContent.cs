// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal sealed class WorkspaceDeleteResponseContent() : HttpContent
{
    public bool ReadAttempted { get; private set; }

    public bool IsDisposed { get; private set; }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        ReadAttempted = true;
        return Task.FromException(new InvalidOperationException("The deletion response body must not be read."));
    }

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
