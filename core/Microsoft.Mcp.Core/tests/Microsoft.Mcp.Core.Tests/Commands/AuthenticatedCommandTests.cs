// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Identity;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

/// <summary>
/// Tests for <see cref="AuthenticatedCommand{TOptions, TResult}"/> error handling. These lock in the
/// clear, actionable messaging surfaced for the common "not signed in" credential failure so it does
/// not regress back to the generic authentication message.
/// </summary>
public sealed class AuthenticatedCommandTests
{
    [CommandMetadata(
        Id = "00000000-0000-0000-0000-0000000000a1",
        Name = "test-auth",
        Title = "Test Auth Command",
        Description = "A command used only to exercise AuthenticatedCommand error handling in tests.")]
    private sealed class AuthTestCommand : AuthenticatedCommand<EmptyOptions, string>
    {
        public override Task<CommandResponse> ExecuteAsync(
            CommandContext context, EmptyOptions options, CancellationToken cancellationToken)
            => Task.FromResult(context.Response);

        public string InvokeGetErrorMessage(Exception ex) => GetErrorMessage(ex);

        public HttpStatusCode InvokeGetStatusCode(Exception ex) => GetStatusCode(ex);
    }

    [Fact]
    public void GetErrorMessage_CredentialUnavailable_ReturnsActionableMessage()
    {
        var command = new AuthTestCommand();
        var ex = new CredentialUnavailableException("DefaultAzureCredential failed to retrieve a token.");

        var message = command.InvokeGetErrorMessage(ex);

        Assert.Contains("az login", message);
        Assert.Contains("Azure credentials not found or unavailable", message);
    }

    [Fact]
    public void GetStatusCode_CredentialUnavailable_ReturnsUnauthorized()
    {
        var command = new AuthTestCommand();
        var ex = new CredentialUnavailableException("DefaultAzureCredential failed to retrieve a token.");

        Assert.Equal(HttpStatusCode.Unauthorized, command.InvokeGetStatusCode(ex));
    }

    [Fact]
    public void GetErrorMessage_AuthenticationFailed_ReturnsGenericAuthMessage()
    {
        var command = new AuthTestCommand();
        var ex = new AuthenticationFailedException("token acquisition failed");

        var message = command.InvokeGetErrorMessage(ex);

        Assert.Contains("Authentication failed", message);
        Assert.Contains("az login", message);
    }
}
