// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

public sealed class ValidationResultTests
{
    [Fact]
    public void AddError_PreservesUserMessageAndTracksTelemetryMessage()
    {
        var result = new ValidationResult();

        result.AddError("Invalid value 'private input'.", "Invalid value.");

        Assert.Equal(["Invalid value 'private input'."], result.Errors);
        Assert.Equal("Invalid value.", result.TelemetrySafeMessage);
    }

    [Fact]
    public void LegacyError_UsesGenericTelemetryMessage()
    {
        var result = new ValidationResult();
        result.AddError("Invalid value 'private input'.", "Invalid value.");
        result.Errors.Add("Another private input.");

        Assert.Equal("Invalid options.", result.TelemetrySafeMessage);
    }

    [Fact]
    public void ChangedUserMessage_UsesGenericTelemetryMessage()
    {
        var result = new ValidationResult();
        result.AddError("Invalid value 'private input'.", "Invalid value.");
        result.Errors[0] = "Another private input.";

        Assert.Equal("Invalid options.", result.TelemetrySafeMessage);
    }
}
