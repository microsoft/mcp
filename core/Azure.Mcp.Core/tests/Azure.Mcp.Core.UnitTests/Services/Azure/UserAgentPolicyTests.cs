// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Services.Azure;

public class UserAgentPolicyTests
{
    [Fact]
    public void Constructor_SetsUserAgent()
    {
        // Arrange
        const string ExpectedUserAgent = "TestUserAgent/1.0";

        // Act
        var policy = new UserAgentPolicy(ExpectedUserAgent);

        // Assert - Implicit test that constructor doesn't throw
        Assert.NotNull(policy);
    }

    [Fact]
    public void Constructor_ThrowsEmptyUserAgent()
    {
        Assert.Throws<ArgumentException>(() => new UserAgentPolicy(""));
    }

    [Fact]
    public void OnSendingRequest_SetsUserAgentHeader()
    {
        // Arrange
        const string ExpectedUserAgent = "TestUserAgent/1.0";
        var policy = new UserAgentPolicy(ExpectedUserAgent);
        var request = new MockHttpRequest();
        var message = new HttpMessage(request, new MockResponseClassifier());

        // Act
        policy.OnSendingRequest(message);

        // Assert

        var headers = request.Headers;

        Assert.Single(headers);

        var actual = headers.Single();
        Assert.Equal(UserAgentPolicy.UserAgentHeader, actual.Name);
        Assert.Equal(ExpectedUserAgent, actual.Value);
    }

    [Fact]
    public void OnSendingRequest_CallsBaseMethod()
    {
        // Arrange
        const string UserAgent = "TestUserAgent/1.0";
        var derivedPolicy = new TestableUserAgentPolicy(UserAgent);
        var request = new MockHttpRequest();
        var message = new HttpMessage(request, new MockResponseClassifier());

        // Act
        derivedPolicy.OnSendingRequest(message);

        // Assert
        Assert.True(derivedPolicy.BaseOnSendingRequestCalled);
    }

    // Helper class for testing the base method call
    private class TestableUserAgentPolicy(string userAgent) : UserAgentPolicy(userAgent)
    {
        public bool BaseOnSendingRequestCalled { get; private set; }

        public override void OnSendingRequest(HttpMessage message)
        {
            base.OnSendingRequest(message);
            BaseOnSendingRequestCalled = true;
        }
    }

    // Mock response classifier for creating HttpMessage instances
    private class MockResponseClassifier : ResponseClassifier
    {
        public override bool IsErrorResponse(HttpMessage message)
        {
            return false;
        }
    }

    private class MockHttpRequest : Request
    {
        public override string ClientRequestId { get; set; } = "Test-Request-Id";

        public List<HttpHeader> ReceivedHeaders { get; } = [];

        public override void Dispose()
        {
        }

        protected override void AddHeader(string name, string value) => ReceivedHeaders.Add(new HttpHeader(name, value));

        protected override bool ContainsHeader(string name) => ReceivedHeaders.Any(x => x.Name == name);

        protected override IEnumerable<HttpHeader> EnumerateHeaders()
        {
            return ReceivedHeaders;
        }

        protected override bool RemoveHeader(string name)
        {
            return false;
        }

        protected override bool TryGetHeader(string name, [NotNullWhen(true)] out string? value)
        {
            value = "";
            return false;

        }

        protected override bool TryGetHeaderValues(string name, [NotNullWhen(true)] out IEnumerable<string>? values)
        {
            values = null;
            return false;
        }
    }
}
