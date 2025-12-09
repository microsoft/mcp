// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Kusto.Services;

public sealed class KustoResult
{
    public JsonElement RootElement { get; }

    private KustoResult(JsonElement rootElement)
    {
        RootElement = rootElement;
    }

    public static KustoResult FromHttpResponseMessage(HttpResponseMessage response)
    {
        using (response)
        {
            using (var content = response.Content.ReadAsStream())
            {
                using (var jsonDoc = JsonDocument.Parse(content))
                {
                    return new KustoResult(jsonDoc.RootElement.Clone());
                }
            }
        }
    }
}
