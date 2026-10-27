// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

// GoalMembersData treats id as read-only, but the update action requires the discovered goal-resource IDs.
internal sealed class GoalResourceIdPolicy(IReadOnlyList<string> resourceIds) : HttpPipelineSynchronousPolicy
{
    public override void OnSendingRequest(HttpMessage message)
    {
        if (message.Request.Method != RequestMethod.Post ||
            !message.Request.Uri.Path.EndsWith("/updateGoalResources", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (message.Request.Content is null)
        {
            throw new InvalidOperationException("The goal resource update request has no content.");
        }

        using var input = new MemoryStream();
        message.Request.Content.WriteTo(input, message.CancellationToken);
        input.Position = 0;
        using JsonDocument document = JsonDocument.Parse(input);
        JsonElement resources = document.RootElement.GetProperty("resources");
        if (resources.GetArrayLength() != resourceIds.Count)
        {
            throw new InvalidOperationException("The goal resource update does not match the discovered resource IDs.");
        }

        using var output = new MemoryStream();
        using (var writer = new Utf8JsonWriter(output))
        {
            writer.WriteStartObject();
            writer.WriteStartArray("resources");
            int index = 0;
            foreach (JsonElement resource in resources.EnumerateArray())
            {
                writer.WriteStartObject();
                writer.WriteString("id", resourceIds[index++]);
                foreach (JsonProperty property in resource.EnumerateObject())
                {
                    if (property.Name != "id")
                    {
                        property.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        message.Request.Content = RequestContent.Create(output.ToArray());
    }
}
