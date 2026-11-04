// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class ItemJobInstanceListInput
{
    internal const int MaxContinuationTokenBytes = 8192;
    internal const string InvalidContinuationTokenMessage =
        "Continuation token must be valid Unicode, not blank, without control characters and at most 8,192 UTF-8 bytes.";

    private static readonly UTF8Encoding s_utf8 = new(false, true);

    internal static bool IsValidContinuationToken(string? token)
    {
        if (token is null)
        {
            return true;
        }
        if (string.IsNullOrWhiteSpace(token) || token.Length > MaxContinuationTokenBytes || token.Any(char.IsControl))
        {
            return false;
        }
        try
        {
            return s_utf8.GetByteCount(token) <= MaxContinuationTokenBytes;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }
}
