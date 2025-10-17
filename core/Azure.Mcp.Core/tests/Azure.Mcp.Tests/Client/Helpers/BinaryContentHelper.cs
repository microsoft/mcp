using System.ClientModel;
using System.Text.Json;

namespace Azure.Mcp.Tests.Client.Helpers;

/// <summary>
/// This entire class is a placeholder. Azure.Mcp.Core takes a dependency on Azure.Core, so what I should do is generate the test-proxy client
/// taking an Azure.Core dependency to generate, versus System.ClientModel (which is what it is rn)
/// </summary>
internal static class BinaryContentHelper
{
    private static readonly JsonSerializerOptions _defaultJsonOptions = new()
    {
        WriteIndented = false
    };

    /// <summary>
    /// Serialize object to JSON UTF8 bytes and wrap into BinaryContent via BinaryData factory.
    /// Avoid generic Create<T> which expects IPersistableModel.
    /// </summary>
    public static BinaryContent FromObject<T>(T value, JsonSerializerOptions? jsonOptions = null)
    {
        if (value is null)
        {
            return BinaryContent.Create(BinaryData.FromString("null"));
        }
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, jsonOptions ?? _defaultJsonOptions);
        return BinaryContent.Create(new BinaryData(bytes));
    }

    public static BinaryContent FromDictionary(IDictionary<string, string> dict, JsonSerializerOptions? jsonOptions = null)
        => FromObject(dict, jsonOptions);

    public static BinaryContent FromJsonString(string json)
        => BinaryContent.Create(BinaryData.FromString(string.IsNullOrEmpty(json) ? "null" : json));
}
