// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Options;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Wraps a service continuation token with a fingerprint of the tool and its fixed arguments.
/// This detects request mismatches, not tampering or authorization.
/// </summary>
public sealed class PaginationCursor(string toolId, params string?[] fixedArguments)
{
    private const string Version = "v1";
    private static readonly UTF8Encoding s_utf8 = new(false, true);
    private readonly string _fingerprint = GetFingerprint(toolId, fixedArguments);

    public string? Encode(string? serviceCursor) =>
        string.IsNullOrWhiteSpace(serviceCursor)
            ? null
            : $"{Version}.{_fingerprint}.{Base64Url.EncodeToString(s_utf8.GetBytes(serviceCursor))}";

    public string? Decode(string? cursor)
    {
        if (cursor is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(cursor))
        {
            throw new CommandValidationException(ICursorOption.BlankCursorError);
        }

        var parts = cursor.Split('.', 3);
        if (parts.Length != 3 || parts[0] != Version || parts[1].Length != _fingerprint.Length || cursor.Any(char.IsWhiteSpace))
        {
            throw InvalidCursor();
        }

        if (!string.Equals(parts[1], _fingerprint, StringComparison.Ordinal))
        {
            throw new CommandValidationException(
                "--cursor does not match the original request. Reuse the original parameters or omit --cursor to start over.");
        }

        string serviceCursor;
        try
        {
            serviceCursor = s_utf8.GetString(Base64Url.DecodeFromChars(parts[2]));
        }
        catch (Exception ex) when (ex is FormatException or DecoderFallbackException)
        {
            throw InvalidCursor();
        }

        return string.IsNullOrWhiteSpace(serviceCursor) ? throw InvalidCursor() : serviceCursor;
    }

    private static string GetFingerprint(string toolId, string?[] fixedArguments)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        writer.WriteStartArray();
        writer.WriteStringValue(toolId);
        foreach (var argument in fixedArguments)
        {
            writer.WriteStringValue(argument);
        }
        writer.WriteEndArray();
        writer.Flush();
        return Convert.ToHexString(SHA256.HashData(buffer.WrittenSpan));
    }

    private static CommandValidationException InvalidCursor() => new(
        "--cursor is invalid. Pass nextCursor unchanged from a previous call to this tool or omit --cursor to start over.");
}
