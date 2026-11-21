// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Support.Models;

namespace Azure.Mcp.Tools.Support.Services;

public interface ISupportService
{
    Task<List<SupportTicket>> ListTicketsAsync(
        string subscription,
        string? filter,
        int? top,
        string? tenant,
        CancellationToken cancellationToken);

    Task<SupportTicket> GetTicketAsync(
        string subscription,
        string ticketName,
        bool includeCommunications,
        string? tenant,
        CancellationToken cancellationToken);

    Task<SupportTicket> CreateTicketAsync(
        string subscription,
        SupportTicketCreateRequest request,
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<SupportServiceInfo>> ListServicesAsync(
        string? tenant,
        CancellationToken cancellationToken);

    Task<List<SupportProblemClassification>> ListProblemClassificationsAsync(
        string serviceName,
        string? tenant,
        CancellationToken cancellationToken);
}
