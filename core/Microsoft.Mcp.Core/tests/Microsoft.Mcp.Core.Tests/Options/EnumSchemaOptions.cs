// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Microsoft.Mcp.Core.Tests.Options;

public sealed class EnumSchemaOptions()
{
    [Option(Description = "The selected day.")]
    public required DayOfWeek Day { get; set; }

    [Option(Description = "An optional day.")]
    public DayOfWeek? OptionalDay { get; set; }

    [Option(Description = "The selected days.")]
    public required DayOfWeek[] Days { get; set; }

    [Option(Description = "Optional days.")]
    public DayOfWeek[]? OptionalDays { get; set; }

    [Option(Description = "Unrestricted text.")]
    public string? Text { get; set; }
}
