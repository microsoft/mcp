// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:ignore FFFFFFFK

using System.Globalization;
using System.Xml;
using Azure.Mcp.Tools.Monitor.Models.Log;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Monitor.Validation;

internal static class LogSearchTimeRangeParser
{
    public static TimeSpan MaximumTimespan { get; } = TimeSpan.FromDays(30);

    private static readonly string[] s_rfc3339Formats =
    [
        "yyyy-MM-dd'T'HH:mm:ssK",
        "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK"
    ];

    public static LogSearchTimeRange Parse(string? timespan, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(timespan))
        {
            throw new CommandValidationException("--timespan is required.");
        }

        if (timespan.Contains('/'))
        {
            return ParseClosedInterval(timespan, now);
        }

        int timeSeparator = timespan.IndexOf('T');
        var dateComponent = timespan.AsSpan(0, timeSeparator >= 0 ? timeSeparator : timespan.Length);
        if (dateComponent.Contains('Y') || dateComponent.Contains('M'))
        {
            throw new CommandValidationException(
                "--timespan durations cannot use calendar year or month components.");
        }

        TimeSpan duration;
        try
        {
            duration = XmlConvert.ToTimeSpan(timespan);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            throw new CommandValidationException(
                "--timespan must be a positive ISO 8601 duration or a closed RFC 3339 start/end interval.");
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new CommandValidationException("--timespan duration must be positive.");
        }

        if (duration > MaximumTimespan)
        {
            throw new CommandValidationException("--timespan cannot exceed 30 days.");
        }

        return new(now - duration, now);
    }

    private static LogSearchTimeRange ParseClosedInterval(string timespan, DateTimeOffset now)
    {
        var parts = timespan.Split('/');
        if (parts.Length != 2 ||
            !TryParseRfc3339(parts[0], out var start) ||
            !TryParseRfc3339(parts[1], out var end))
        {
            throw new CommandValidationException(
                "--timespan intervals must contain closed RFC 3339 start and end timestamps.");
        }

        if (start >= end)
        {
            throw new CommandValidationException("--timespan start must be earlier than its end.");
        }

        if (start >= now)
        {
            throw new CommandValidationException("--timespan cannot be entirely in the future.");
        }

        if (end - start > MaximumTimespan)
        {
            throw new CommandValidationException("--timespan cannot exceed 30 days.");
        }

        return new(start, end);
    }

    private static bool TryParseRfc3339(string value, out DateTimeOffset result)
    {
        if (!value.Contains('T') ||
            !(value.EndsWith('Z') || HasNumericOffset(value)))
        {
            result = default;
            return false;
        }

        return DateTimeOffset.TryParseExact(
            value,
            s_rfc3339Formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out result);
    }

    private static bool HasNumericOffset(string value)
    {
        if (value.Length < 6)
        {
            return false;
        }

        var offsetStart = value.Length - 6;
        return (value[offsetStart] is '+' or '-') &&
            char.IsAsciiDigit(value[offsetStart + 1]) &&
            char.IsAsciiDigit(value[offsetStart + 2]) &&
            value[offsetStart + 3] == ':' &&
            char.IsAsciiDigit(value[offsetStart + 4]) &&
            char.IsAsciiDigit(value[offsetStart + 5]);
    }
}
