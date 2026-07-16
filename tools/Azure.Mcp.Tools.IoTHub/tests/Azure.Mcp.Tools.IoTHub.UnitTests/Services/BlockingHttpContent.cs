// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Azure.Mcp.Tools.IoTHub.UnitTests.Services;

internal sealed class BlockingHttpContent() : HttpContent
{
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        => Task.Delay(Timeout.InfiniteTimeSpan);

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        => Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }
}