// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppLens.Models;

/// <summary>
/// Represents an AppLens conversation session about a particular resource.
/// </summary>
/// <param name="SessionId">The unique ID of this conversation.</param>
/// <param name="ResourceId">The ARM resource ID of the resource in question.</param>
/// <param name="Token">The authorization token.</param>
/// <param name="ExpiresIn">The length of time the Token will remain valid.</param>
public record AppLensSession(string SessionId, string ResourceId, string Token, int ExpiresIn);

/// <summary>
/// Represents the result of trying to obtain an AppLens session.
/// </summary>
public abstract record GetAppLensSessionResult;

/// <summary>
/// Represents the successful creation of an AppLens session.
/// </summary>
/// <param name="Session">The new AppLens session.</param>
public sealed record SuccessfulAppLensSessionResult(AppLensSession Session) : GetAppLensSessionResult;

/// <summary>
/// Represents a failure while trying to create an AppLens session.
/// </summary>
/// <param name="Message">A message about the failure suitable for display to the user.</param>
public sealed record FailedAppLensSessionResult(string Message) : GetAppLensSessionResult;

/// <summary>
/// Result of Azure Resource Graph query for resource lookup.
/// </summary>
/// <param name="Id">Resource ID.</param>
/// <param name="SubscriptionId">Subscription ID.</param>
/// <param name="ResourceGroup">Resource group name.</param>
/// <param name="ResourceType">Resource type.</param>
/// <param name="ResourceKind">Resource kind.</param>
/// <param name="ResourceName">Resource name.</param>
public record AppLensArgQueryResult(
    string Id,
    string SubscriptionId,
    string ResourceGroup,
    string ResourceType,
    string ResourceKind,
    string ResourceName);

/// <summary>
/// Abstract base for resource finding results.
/// </summary>
public abstract record FindResourceIdResult;

/// <summary>
/// Successful resource finding result.
/// </summary>
/// <param name="ResourceId">The resource ID.</param>
/// <param name="ResourceTypeAndKind">The resource type and kind.</param>
/// <param name="Message">Optional message.</param>
public sealed record FoundResourceResult(string ResourceId, string ResourceTypeAndKind, string? Message) : FindResourceIdResult;

/// <summary>
/// Failed resource finding result.
/// </summary>
/// <param name="Message">Error message.</param>
public sealed record DidNotFindResourceResult(string Message) : FindResourceIdResult;

/// <summary>
/// Options controlling the behavior of the AppLens service.
/// </summary>
public class AppLensOptions
{
    /// <summary>
    /// Gets or sets the time in seconds to wait for the next SignalR message from AppLens.
    /// </summary>
    public int MessageTimeOutInSeconds { get; set; } = 15;

    /// <summary>
    /// The client name to report to AppLens when making a request.
    /// </summary>
    public string ClientName { get; set; } = "AzureMCP";
}

/// <summary>
/// Diagnostic insights and solutions from AppLens.
/// </summary>
/// <param name="Insights">List of diagnostic insights.</param>
/// <param name="Solutions">List of proposed solutions.</param>
/// <param name="ResourceId">The resource ID that was diagnosed.</param>
/// <param name="ResourceType">The type of resource that was diagnosed.</param>
public record DiagnosticResult(
    List<string> Insights,
    List<string> Solutions,
    string ResourceId,
    string ResourceType);

/// <summary>
/// Command result for resource diagnose operation.
/// </summary>
/// <param name="Result">The diagnostic result.</param>
public record ResourceDiagnoseCommandResult(DiagnosticResult Result);
