// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Services;

internal static class FabricResponseBodyReader
{
    internal const int DefaultMaxJsonBytes = 1024 * 1024;

    internal static async Task<ReadOnlyMemory<byte>> ReadAsync(
        HttpResponseMessage response, int maxBytes, Func<Exception> tooLarge, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxBytes);
        if (response.Content.Headers.ContentLength > maxBytes)
        {
            throw tooLarge();
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        while (true)
        {
            // Read at most one byte beyond the limit, including streams without Content-Length.
            var count = (int)Math.Min(chunk.Length, (long)maxBytes + 1 - buffer.Length);
            var read = await stream.ReadAsync(chunk.AsMemory(0, count), cancellationToken);
            if (read == 0)
            {
                break;
            }
            if (buffer.Length + read > maxBytes)
            {
                throw tooLarge();
            }
            buffer.Write(chunk, 0, read);
        }

        // Disposing MemoryStream does not release its backing array; avoid a second full-size copy.
        return buffer.GetBuffer().AsMemory(0, (int)buffer.Length);
    }
}
