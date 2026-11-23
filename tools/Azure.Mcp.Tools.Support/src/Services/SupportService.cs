// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Support.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Support;
using Azure.ResourceManager.Support.Models;

namespace Azure.Mcp.Tools.Support.Services;

public sealed class SupportService(IAzureService azureService) : BaseAzureService(azureService), ISupportService
{
    public async Task<List<SupportTicket>> ListTicketsAsync(
        string subscription,
        string? filter,
        int? top,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        var tickets = new List<SupportTicket>();

        await foreach (var ticket in subscriptionResource.GetSubscriptionSupportTickets()
            .GetAllAsync(top, filter, cancellationToken))
        {
            tickets.Add(MapTicket(ticket.Data));
            if (top.HasValue && tickets.Count >= top.Value)
            {
                break;
            }
        }

        return tickets;
    }

    public async Task<SupportTicket> GetTicketAsync(
        string subscription,
        string ticketName,
        bool includeCommunications,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(ticketName), ticketName));

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        var response = await subscriptionResource.GetSubscriptionSupportTicketAsync(ticketName, cancellationToken);
        var ticketResource = response.Value;

        List<SupportTicketCommunication>? communications = null;
        if (includeCommunications)
        {
            communications = [];
            await foreach (var communication in ticketResource.GetSupportTicketCommunications()
                .GetAllAsync(cancellationToken: cancellationToken))
            {
                communications.Add(MapCommunication(communication.Data));
            }
        }

        return MapTicket(ticketResource.Data) with { Communications = communications };
    }

    public async Task<SupportTicket> CreateTicketAsync(
        string subscription,
        SupportTicketCreateRequest request,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(request.Title), request.Title),
            (nameof(request.Description), request.Description),
            (nameof(request.ProblemClassificationId), request.ProblemClassificationId));

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);

        var contact = new SupportContactProfile(
            request.FirstName,
            request.LastName,
            new PreferredContactMethod(request.ContactMethod.ToLowerInvariant()),
            request.Email,
            request.Timezone,
            request.Country,
            request.Language)
        {
            PhoneNumber = request.PhoneNumber
        };

        var data = new SupportTicketData(
            request.Description,
            request.ProblemClassificationId,
            new SupportSeverityLevel(request.Severity.ToLowerInvariant()),
            AdvancedDiagnosticConsent.No,
            contact,
            request.Title,
            GetServiceIdFromProblemClassificationId(request.ProblemClassificationId));

        if (!string.IsNullOrWhiteSpace(request.TechnicalResourceId))
        {
            data.TechnicalTicketDetailsResourceId = new ResourceIdentifier(request.TechnicalResourceId);
        }

        var ticketName = string.IsNullOrWhiteSpace(request.TicketName) ? Guid.NewGuid().ToString() : request.TicketName;

        var operation = await subscriptionResource.GetSubscriptionSupportTickets()
            .CreateOrUpdateAsync(WaitUntil.Started, ticketName, data, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);

        return MapTicket(operation.Value.Data);
    }

    public async Task<List<SupportServiceInfo>> ListServicesAsync(string? tenant, CancellationToken cancellationToken)
    {
        var tenantResource = await ResolveTenantResourceAsync(tenant, cancellationToken);
        var services = new List<SupportServiceInfo>();

        await foreach (var service in tenantResource.GetSupportAzureServices().GetAllAsync(cancellationToken))
        {
            services.Add(new SupportServiceInfo(
                service.Data.Name,
                service.Data.Id?.ToString(),
                service.Data.DisplayName,
                service.Data.ResourceTypes?.ToList()));
        }

        return services;
    }

    public async Task<List<SupportProblemClassification>> ListProblemClassificationsAsync(
        string serviceName,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters((nameof(serviceName), serviceName));

        var tenantResource = await ResolveTenantResourceAsync(tenant, cancellationToken);
        var serviceResponse = await tenantResource.GetSupportAzureServiceAsync(serviceName, cancellationToken);
        var classifications = new List<SupportProblemClassification>();

        await foreach (var classification in serviceResponse.Value.GetProblemClassifications().GetAllAsync(cancellationToken))
        {
            classifications.Add(new SupportProblemClassification(
                classification.Data.Name,
                classification.Data.Id?.ToString(),
                classification.Data.DisplayName));
        }

        return classifications;
    }

    internal static string GetServiceIdFromProblemClassificationId(string problemClassificationId)
    {
        const string Separator = "/problemClassifications/";
        var index = problemClassificationId.IndexOf(Separator, StringComparison.OrdinalIgnoreCase);
        if (index <= 0)
        {
            throw new ArgumentException(
                "The problem classification ID must be in the format '/providers/Microsoft.Support/services/{serviceName}/problemClassifications/{problemClassificationName}'.",
                nameof(problemClassificationId));
        }

        return problemClassificationId[..index];
    }

    private async Task<TenantResource> ResolveTenantResourceAsync(string? tenant, CancellationToken cancellationToken)
    {
        var tenants = await AzureService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found for the current credential.");
        }

        if (!string.IsNullOrWhiteSpace(tenant))
        {
            var resolvedTenantId = await AzureService.ResolveTenantIdAsync(tenant, cancellationToken)
                ?? throw new InvalidOperationException($"Could not resolve tenant '{tenant}'.");
            var tenantId = Guid.Parse(resolvedTenantId);
            return tenants.FirstOrDefault(candidate => candidate.Data.TenantId == tenantId)
                ?? throw new InvalidOperationException($"No accessible tenant found for tenant '{tenant}'.");
        }

        // The Azure support service catalog is global, so any accessible tenant can be used to read it.
        return tenants[0];
    }

    private static SupportTicket MapTicket(SupportTicketData data) => new(
        data.Name,
        data.Id?.ToString(),
        data.SupportTicketId,
        data.Title,
        data.Description,
        data.Status?.ToString(),
        data.Severity.ToString(),
        data.ServiceId,
        data.ServiceDisplayName,
        data.ProblemClassificationId,
        data.ProblemClassificationDisplayName,
        data.TechnicalTicketDetailsResourceId?.ToString(),
        data.SupportPlanType?.ToString(),
        data.SupportEngineerEmailAddress,
        data.CreatedOn,
        data.ModifiedOn);

    private static SupportTicketCommunication MapCommunication(SupportTicketCommunicationData data) => new(
        data.Name,
        data.Subject,
        data.Body,
        data.Sender,
        data.CommunicationType?.ToString(),
        data.CommunicationDirection?.ToString(),
        data.CreatedOn);
}
