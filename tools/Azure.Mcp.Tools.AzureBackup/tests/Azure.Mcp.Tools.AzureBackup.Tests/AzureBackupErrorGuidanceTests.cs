// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests;

public class AzureBackupErrorGuidanceTests
{
    private const string InvalidParameter = "UserErrorInvalidRequestParameter";

    [Fact]
    public void TryDescribeInvalidParameter_UnnamedParameter_ListsSuppliedOptionsAndLimits()
    {
        var ex = new RequestFailedException(400,
            "Parameter NO_PARAM in request is invalid. Please provide correct value for parameter NO_PARAM",
            InvalidParameter, null);

        var described = AzureBackupErrorGuidance.TryDescribeInvalidParameter(
            ex, "policy update", ["--instant-rp-retention-days", "--daily-retention-days"], "Limits hint.", out var message);

        Assert.True(described);
        Assert.Contains("policy update", message);
        Assert.Contains(InvalidParameter, message);
        Assert.Contains("did not identify which one", message);
        Assert.Contains("Options supplied: --instant-rp-retention-days, --daily-retention-days.", message);
        Assert.EndsWith("Limits hint.", message);
        Assert.DoesNotContain("NO_PARAM", message);
    }

    [Fact]
    public void TryDescribeInvalidParameter_NamedParameter_IncludesOnlyTheParameterName()
    {
        var ex = new RequestFailedException(400,
            "Parameter instantRpRetentionRangeInDays in request is invalid. Backend detail that should not be echoed.",
            InvalidParameter, null);

        var described = AzureBackupErrorGuidance.TryDescribeInvalidParameter(ex, "policy create", [], null, out var message);

        Assert.True(described);
        Assert.Contains("request parameter 'instantRpRetentionRangeInDays' is invalid", message);
        Assert.Contains("Options supplied: none.", message);
        Assert.DoesNotContain("Backend detail", message);
    }

    [Fact]
    public void TryDescribeInvalidParameter_UnrecognizedMessage_TreatsParameterAsUnnamed()
    {
        var ex = new RequestFailedException(400, "Something else went wrong.", InvalidParameter, null);

        Assert.True(AzureBackupErrorGuidance.TryDescribeInvalidParameter(ex, "vault update", ["--redundancy"], null, out var message));
        Assert.Contains("did not identify which one", message);
        Assert.DoesNotContain("Something else", message);
    }

    [Theory]
    [InlineData("BMSUserErrorInvalidPolicyInput")]
    [InlineData(null)]
    public void TryDescribeInvalidParameter_OtherErrorCodes_ReturnsFalse(string? errorCode)
    {
        var ex = new RequestFailedException(400, "Parameter NO_PARAM in request is invalid.", errorCode, null);

        Assert.False(AzureBackupErrorGuidance.TryDescribeInvalidParameter(ex, "policy update", [], null, out var message));
        Assert.Null(message);
    }

    [Fact]
    public void TryDescribeInvalidParameter_NonAzureException_ReturnsFalse()
    {
        Assert.False(AzureBackupErrorGuidance.TryDescribeInvalidParameter(
            new ArgumentException("Parameter NO_PARAM in request is invalid."), "policy update", [], null, out _));
    }

    [Fact]
    public void TryDescribeInvalidParameter_ExceptionAlreadyWrappedWithContext_ReturnsFalse()
    {
        var inner = new RequestFailedException(400, "Parameter NO_PARAM in request is invalid.", InvalidParameter, null);
        var wrapped = new RequestFailedException(400, "Vault 'v' was created, but setting storage redundancy failed.", InvalidParameter, inner);

        Assert.False(AzureBackupErrorGuidance.TryDescribeInvalidParameter(wrapped, "vault create", ["--storage-type"], null, out _));
    }

    [Fact]
    public void Supplied_ReturnsSuppliedOptionsInOrder()
    {
        var supplied = AzureBackupErrorGuidance.Supplied(("--a", true), ("--b", false), ("--c", true));

        Assert.Equal(["--a", "--c"], supplied);
    }
}
