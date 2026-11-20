// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Mock HA client used until the HA skill is deployed on the Discovery Service. It makes no calls and returns a started HA session
/// whose id starts with <see cref="SessionIdPrefix"/>, which the HA status tool recognises and answers with a mocked result.
/// Off by default; set <see cref="MockSettingName"/> to <c>true</c> to use it instead of <see cref="DsHaSessionClient"/>.
/// </summary>
public sealed class MockHaSessionClient(ILogger<MockHaSessionClient> logger) : IHaSessionClient
{
    internal const string MockSettingName = "AZUREMIGRATE_HA_MOCK";
    internal const string SessionIdPrefix = "ha-mock-";

    private readonly ILogger<MockHaSessionClient> _logger = logger;

    /// <summary>True only when <see cref="MockSettingName"/> is set to <c>true</c>.</summary>
    internal static bool IsEnabled =>
        string.Equals(Environment.GetEnvironmentVariable(MockSettingName)?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

    internal static bool IsMockSession(string? sessionId) =>
        sessionId is not null && sessionId.StartsWith(SessionIdPrefix, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public Task<HaSessionResult> StartAsync(string token, HaInput input, CancellationToken cancellationToken) =>
        StartInSessionAsync(token, input, SessionIdPrefix + Guid.NewGuid().ToString("N")[..12], cancellationToken);

    /// <inheritdoc/>
    public Task<HaSessionResult> StartInSessionAsync(string token, HaInput input, string sessionId, CancellationToken cancellationToken)
    {
        if (!IsMockSession(sessionId))
        {
            // Keep within the Discovery Service limit of 20 characters.
            sessionId = SessionIdPrefix + sessionId[Math.Max(0, sessionId.Length - 12)..];
        }

        string operationId = Guid.NewGuid().ToString("D");
        _logger.LogInformation("HA is mocked: returning session {SessionId}, operation {OperationId} without calling the Discovery Service.",
            sessionId, operationId);
        return Task.FromResult(new HaSessionResult(null, (int)HttpStatusCode.Accepted, sessionId, false, operationId, Guid.NewGuid().ToString(), null));
    }

    /// <inheritdoc/>
    public Task<HaOperationStatus> GetOperationStatusAsync(string token, string sessionId, Guid operationId, CancellationToken cancellationToken) =>
        Task.FromResult(new HaOperationStatus((int)HttpStatusCode.OK, "Succeeded", null, null, null, null, null));

    /// <inheritdoc/>
    public Task<Inventory.InventoryExportResult> ExportAsync(string token, string sessionId, CancellationToken cancellationToken) =>
        Task.FromResult(new Inventory.InventoryExportResult(null, null, "HA is mocked, so there is no report to export."));

    /// <inheritdoc/>
    public Task<string?> DownloadReportAsync(string reportUri, CancellationToken cancellationToken) => Task.FromResult<string?>(null);
}
