// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Mock HA client used until the HA skill is deployed on the Discovery Service. It makes no calls and returns a started HA session
/// whose id starts with <see cref="SessionIdPrefix"/>, which the HA status tool recognises and answers with a mocked result.
/// Set <see cref="MockSettingName"/> to <c>false</c> to use <see cref="DsHaSessionClient"/> instead.
/// </summary>
public sealed class MockHaSessionClient(ILogger<MockHaSessionClient> logger) : IHaSessionClient
{
    internal const string MockSettingName = "AZUREMIGRATE_HA_MOCK";
    internal const string SessionIdPrefix = "ha-mock-";

    private readonly ILogger<MockHaSessionClient> _logger = logger;

    /// <summary>True unless <see cref="MockSettingName"/> is set to <c>false</c>.</summary>
    internal static bool IsEnabled =>
        !string.Equals(Environment.GetEnvironmentVariable(MockSettingName)?.Trim(), "false", StringComparison.OrdinalIgnoreCase);

    internal static bool IsMockSession(string? sessionId) =>
        sessionId is not null && sessionId.StartsWith(SessionIdPrefix, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public Task<HaSessionResult> StartAsync(string token, HaInput input, CancellationToken cancellationToken)
    {
        string sessionId = SessionIdPrefix + Guid.NewGuid().ToString("N")[..12];
        string operationId = Guid.NewGuid().ToString("D");
        _logger.LogInformation("HA is mocked: returning session {SessionId}, operation {OperationId} without calling the Discovery Service.",
            sessionId, operationId);
        return Task.FromResult(new HaSessionResult(null, (int)HttpStatusCode.Accepted, sessionId, false, operationId, Guid.NewGuid().ToString(), null));
    }
}
