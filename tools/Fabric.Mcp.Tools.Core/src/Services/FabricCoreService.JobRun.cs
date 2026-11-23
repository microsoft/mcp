// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<ItemJobRunCommandResult> RunOnDemandItemJobAsync(
        string workspaceId, string itemId, string jobType, string? executionData = null,
        string? jobParameters = null, CancellationToken cancellationToken = default)
    {
        var validation = new ValidationResult();
        ItemJobRunInput.ValidateContext(workspaceId, itemId, jobType, validation);
        if (!validation.IsValid)
        {
            throw new FabricItemInputException(string.Join(' ', validation.Errors));
        }
        var body = ItemJobRunInput.CreateBody(executionData, jobParameters, cancellationToken);
        var workspace = Guid.Parse(workspaceId);
        var item = Guid.Parse(itemId);
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/items/{item:D}/jobs/{Uri.EscapeDataString(jobType)}/instances";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Post, url, body, completionOption: HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken);
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            throw new ItemJobRunRequestException(
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode, retryAfter);
        }

        // Headers confirm acceptance. Do not read the body or let late cancellation discard
        // that fact, and never interpret this job receipt as the /operations LRO protocol.
        var jobInstanceId = GetSubmittedJobInstanceId(response, workspace, item, out var trackingIssue);
        return new(workspace, item, jobType, jobInstanceId,
            retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date,
            trackingIssue, retryAfter is null ? "MissingOrInvalidRetryAfter" : null);
    }

    private static Guid? GetSubmittedJobInstanceId(
        HttpResponseMessage response, Guid workspaceId, Guid itemId, out string? issue)
    {
        issue = "MissingLocation";
        if (!response.Headers.NonValidated.TryGetValues("Location", out var values))
        {
            return null;
        }
        issue = "MultipleLocations";
        if (values.Count != 1)
        {
            return null;
        }

        issue = "InvalidLocation";
        var value = values.Single();
        // Reject alternate representations before Uri normalizes dot segments or escapes.
        // Only the documented job-instance path (absolute or origin-relative) can supply an ID.
        if (value.Length is 0 or > 4096 || value.Any(static ch => char.IsWhiteSpace(ch) || char.IsControl(ch)) ||
            value.Contains('\\') || value.Contains('%') || value.Contains("/./", StringComparison.Ordinal) ||
            value.Contains("/../", StringComparison.Ordinal) || value.StartsWith("//", StringComparison.Ordinal))
        {
            return null;
        }
        var origin = new Uri(FabricEndpoints.GetFabricApiBaseUrl());
        Uri? location;
        var validUri = value.StartsWith('/')
            ? Uri.TryCreate(origin, value, out location)
            : Uri.TryCreate(value, UriKind.Absolute, out location);
        if (!validUri || location is null || location.Scheme != origin.Scheme ||
            !string.Equals(location.Host, origin.Host, StringComparison.OrdinalIgnoreCase) || location.Port != origin.Port ||
            location.UserInfo.Length != 0 || location.Fragment.Length != 0)
        {
            return null;
        }

        var collectionPath = $"{origin.AbsolutePath}/workspaces/{workspaceId:D}/items/{itemId:D}/jobs/instances";
        if (string.Equals(location.AbsolutePath, collectionPath, StringComparison.OrdinalIgnoreCase))
        {
            // Some published parameter examples return the collection, not an instance.
            issue = "CollectionLocation";
            return null;
        }
        if (location.Query.Length != 0 ||
            !location.AbsolutePath.StartsWith(collectionPath + "/", StringComparison.OrdinalIgnoreCase) ||
            !Guid.TryParseExact(location.AbsolutePath[(collectionPath.Length + 1)..], "D", out var id) || id == Guid.Empty)
        {
            return null;
        }
        issue = null;
        return id;
    }
}
