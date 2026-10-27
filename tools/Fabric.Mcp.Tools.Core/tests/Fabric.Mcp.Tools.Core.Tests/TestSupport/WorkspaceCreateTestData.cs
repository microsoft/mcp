// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Fabric.Mcp.Tools.Core.Models;
using NSubstitute;

namespace Fabric.Mcp.Tools.Core.Tests.TestSupport;

internal static class WorkspaceCreateTestData
{
    public const string WorkspaceId = "cfafbeb1-8037-4d0c-896e-a46fb22287ff";
    public const string CapacityId = "f4031b2e-318f-4a14-9a3e-103e9bcfc953";
    public const string DomainId = "88d8f15b-5105-449b-98d3-681345f00326";
    public const string Location = "https://api.fabric.microsoft.com/v1/workspaces/" + WorkspaceId;
    public const string SecretMarker = "private-backend-detail";
    public const string MinimalMetadata = """
        {
          "id": "cfafbeb1-8037-4d0c-896e-a46fb22287ff",
          "displayName": "New workspace",
          "type": "Workspace"
        }
        """;
    public const string FullMetadata = """
        {
          "id": "cfafbeb1-8037-4d0c-896e-a46fb22287ff",
          "displayName": "New workspace",
          "description": "",
          "type": "FutureWorkspaceType",
          "capacityId": "f4031b2e-318f-4a14-9a3e-103e9bcfc953",
          "capacityRegion": "Future Region",
          "domainId": "88d8f15b-5105-449b-98d3-681345f00326",
          "apiEndpoint": "https://metadata-only.example/workspace",
          "tags": [
            { "id": "de627df0-2328-4c05-98f7-b03457a2daef", "displayName": "Planning", "unknown": "ignored" }
          ],
          "capacityAssignmentProgress": "Not part of this response contract",
          "workspaceIdentity": { "secret": "private-backend-detail" },
          "unknown": "private-backend-detail"
        }
        """;

    public static TokenCredential CreateCredential()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("offline-token", DateTimeOffset.UtcNow.AddHours(1)));
        return credential;
    }

    public static HttpResponseMessage CreateResponse(string metadata = MinimalMetadata, string? location = Location)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent(metadata, Encoding.UTF8, "application/json")
        };
        if (location is not null)
        {
            response.Headers.TryAddWithoutValidation("Location", location);
        }

        return response;
    }

    public static WorkspaceCreateResult CreateResult() => new(
        JsonSerializer.Deserialize(MinimalMetadata, CoreJsonContext.Default.CreatedWorkspaceMetadata)!,
        Location);

    public static string WithMetadataProperty(string name, JsonNode? value)
    {
        var metadata = JsonNode.Parse(MinimalMetadata)!.AsObject();
        metadata[name] = value;
        return metadata.ToJsonString();
    }

    public static string WithoutMetadataProperty(string name)
    {
        var metadata = JsonNode.Parse(MinimalMetadata)!.AsObject();
        metadata.Remove(name);
        return metadata.ToJsonString();
    }
}
