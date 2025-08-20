// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Kusto.Services;

public class KustoResult
{
    public JsonDocument? JsonDocument { get; private set; }

    public static KustoResult FromHttpResponseMessage(HttpResponseMessage response)
    {
        var ret = new KustoResult();
        var stream = response.Content.ReadAsStream();
        var jsonDocument = JsonDocument.Parse(stream);
        ret.JsonDocument = jsonDocument;
        return ret;
    }
}
