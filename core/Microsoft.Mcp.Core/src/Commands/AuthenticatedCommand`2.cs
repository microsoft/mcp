// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure;
using Azure.Identity;
using Microsoft.Identity.Client;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// A base class for commands that require authentication.
/// </summary>
/// <typeparam name="TOptions">The type of the options for the command.</typeparam>
/// <typeparam name="TResult">The type of the result for the command.</typeparam>
public abstract class AuthenticatedCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult> : BaseCommand<TOptions, TResult>
    where TOptions : class
{
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        AuthenticationFailedException authEx =>
            $"Authentication failed. Please run 'az login' to sign in to Azure. Details: {authEx.Message}",
        RequestFailedException rfEx => HandleRequestFailedException(rfEx),
        HttpRequestException httpEx =>
            $"Service unavailable or network connectivity issues. Details: {httpEx.Message}",
        TimeoutException timeoutEx =>
            $"The operation timed out. Details: {timeoutEx.Message.TrimEnd('.')}",
        TaskCanceledException canceledEx =>
            $"The operation timed out or was canceled. Details: {canceledEx.Message.TrimEnd('.')}",
        _ => ex.Message  // Just return the actual exception message
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        KeyNotFoundException => HttpStatusCode.NotFound,
        AuthenticationFailedException => HttpStatusCode.Unauthorized,
        RequestFailedException rfEx => (HttpStatusCode)rfEx.Status,
        MsalServiceException msalServiceEx => (HttpStatusCode)msalServiceEx.StatusCode,
        HttpRequestException httpEx => httpEx.StatusCode ?? HttpStatusCode.ServiceUnavailable,
        InvalidOperationException => HttpStatusCode.UnprocessableEntity,
        TimeoutException => HttpStatusCode.GatewayTimeout,
        TaskCanceledException => HttpStatusCode.GatewayTimeout,
        _ => HttpStatusCode.InternalServerError
    };

    private static string HandleRequestFailedException(RequestFailedException ex)
    {
        string message = ex.Message ?? string.Empty;

        if (ex.Status == 401 && message.Contains("InvalidAuthenticationTokenTenant", StringComparison.OrdinalIgnoreCase))
        {
            return "Authentication failed due to a tenant mismatch. " +
            "Your credential is authenticated to a different Azure tenant than the one required by this subscription. " +
            "To resolve: " +
            "1. Authenticate to the target tenant using one of the supported credential types: " +
            "   - Azure CLI: Run 'az login --tenant <tenant_id>' and set AZURE_TOKEN_CREDENTIALS=AzureCliCredential, " +
            "   - Azure PowerShell: Run 'Connect-AzAccount -Tenant <tenant_id>' and set AZURE_TOKEN_CREDENTIALS=AzurePowerShellCredential, " +
            "   - Azure Developer CLI: Run 'azd auth login --tenant-id <tenant_id>' and set AZURE_TOKEN_CREDENTIALS=AzureDeveloperCliCredential, " +
            "2. Restart the MCP Server. " +
            "For the complete list of supported credentials, see: https://aka.ms/azmcp/auth";
        }

        return message;
    }
}
