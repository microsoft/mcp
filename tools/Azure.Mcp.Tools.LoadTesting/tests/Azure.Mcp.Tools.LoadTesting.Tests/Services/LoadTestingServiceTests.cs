// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.Mcp.Tools.LoadTesting.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.LoadTesting.Tests.Services;

public class LoadTestingServiceTests
{
    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(429)]
    [InlineData(500)]
    [InlineData(503)]
    public void EnsureSuccessfulResponse_ErrorResponse_PreservesStatusAndDetails(int status)
    {
        const string errorCode = "LoadTestRequestFailed";
        const string errorMessage = "The load test request failed.";
        using var content = new MemoryStream(Encoding.UTF8.GetBytes(
            """{"error":{"code":"LoadTestRequestFailed","message":"The load test request failed."}}"""));
        var response = Substitute.ForPartsOf<Response>();
        response.Status.Returns(status);
        response.ReasonPhrase.Returns("Request failed");
        response.IsError.Returns(true);
        response.ContentStream.Returns(content);

        var exception = Assert.Throws<RequestFailedException>(() =>
            LoadTestingService.EnsureSuccessfulResponse(response, "Failed to retrieve Azure Load Test."));

        Assert.Equal(status, exception.Status);
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Contains(errorMessage, exception.Message);
        Assert.Same(response, exception.GetRawResponse());
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(204)]
    public void EnsureSuccessfulResponse_SuccessfulResponse_ReturnsOriginalResponse(int status)
    {
        var response = Substitute.For<Response>();
        response.Status.Returns(status);
        response.IsError.Returns(false);

        Assert.Same(response, LoadTestingService.EnsureSuccessfulResponse(response, "Unexpected failure."));
    }

    [Fact]
    public void EnsureSuccessfulResponse_NullResponse_ThrowsInvalidOperationException()
    {
        const string message = "Failed to create Azure Load Test.";

        var exception = Assert.Throws<InvalidOperationException>(() =>
            LoadTestingService.EnsureSuccessfulResponse(null, message));

        Assert.Equal(message, exception.Message);
    }
}
