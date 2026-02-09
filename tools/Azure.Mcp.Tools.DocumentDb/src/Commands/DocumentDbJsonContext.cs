// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using MongoDB.Bson;
using Azure.Mcp.Tools.DocumentDb.Models;

namespace Azure.Mcp.Tools.DocumentDb.Commands;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(List<Dictionary<string, object>>))]
[JsonSerializable(typeof(List<Dictionary<string, object?>>))]
[JsonSerializable(typeof(System.Text.Json.JsonElement))]
internal partial class DocumentDbJsonContext : JsonSerializerContext;

/// <summary>
/// Helper class for creating ResponseResult from JSON strings
/// </summary>
internal static class DocumentDbResponseHelper
{
    public static Microsoft.Mcp.Core.Models.Command.ResponseResult CreateFromJson(string json)
    {
        // Parse the JSON string to a JsonElement to get proper serialization
        var element = System.Text.Json.JsonSerializer.Deserialize(json, DocumentDbJsonContext.Default.JsonElement);
        return Microsoft.Mcp.Core.Models.Command.ResponseResult.Create(element, DocumentDbJsonContext.Default.JsonElement);
    }

    public static string SerializeToJson(object value)
    {
        return value switch
        {
            // Handle BsonDocument by converting to JSON first
            MongoDB.Bson.BsonDocument bsonDoc => bsonDoc.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson }),
            List<MongoDB.Bson.BsonDocument> bsonList => "[" + string.Join(",", bsonList.Select(doc => doc.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.RelaxedExtendedJson }))) + "]",

            // Handle standard types
            Dictionary<string, object?> dict => System.Text.Json.JsonSerializer.Serialize(dict, DocumentDbJsonContext.Default.DictionaryStringObject),
            List<Dictionary<string, object?>> list => System.Text.Json.JsonSerializer.Serialize(list, DocumentDbJsonContext.Default.ListDictionaryStringObject),
            List<string> strList => System.Text.Json.JsonSerializer.Serialize(strList, DocumentDbJsonContext.Default.ListString),
            string str => System.Text.Json.JsonSerializer.Serialize(str, DocumentDbJsonContext.Default.String),
            int i => System.Text.Json.JsonSerializer.Serialize(i, DocumentDbJsonContext.Default.Int32),
            long l => System.Text.Json.JsonSerializer.Serialize(l, DocumentDbJsonContext.Default.Int64),
            bool b => System.Text.Json.JsonSerializer.Serialize(b, DocumentDbJsonContext.Default.Boolean),
            System.Text.Json.JsonElement element => System.Text.Json.JsonSerializer.Serialize(element, DocumentDbJsonContext.Default.JsonElement),

            // Handle IEnumerable<string> (LINQ results)
            System.Collections.Generic.IEnumerable<string> enumStr => System.Text.Json.JsonSerializer.Serialize(enumStr.ToList(), DocumentDbJsonContext.Default.ListString),

            _ => throw new NotSupportedException($"Type {value.GetType().FullName} is not supported for AOT serialization. Please add it to DocumentDbJsonContext.")
        };
    }

    public static T? DeserializeFromJson<T>(string json) where T : class
    {
        // Only supports object type for AOT compatibility
        if (typeof(T) == typeof(object))
        {
            return System.Text.Json.JsonSerializer.Deserialize(json, DocumentDbJsonContext.Default.Object) as T;
        }

        throw new NotSupportedException($"Type {typeof(T).Name} is not supported. Only 'object' type is AOT-compatible.");
    }

    /// <summary>
    /// Processes a DocumentDb service response and applies it to the command context.
    /// </summary>
    /// <param name="context">The command context to update.</param>
    /// <param name="serviceResult">The service result object.</param>
    public static void ProcessResponse(Microsoft.Mcp.Core.Models.Command.CommandContext context, object? serviceResult)
    {
        var response = DocumentDbResponse.FromDictionary(serviceResult);
        if (response == null)
        {
            return;
        }

        context.Response.Status = response.StatusCode;

        if (response.Success)
        {
            // For success with no data, create an empty result with the message
            var dataToSerialize = response.Data ?? new Dictionary<string, object?>
            {
                ["message"] = response.Message
            };
            context.Response.Results = CreateFromJson(SerializeToJson(dataToSerialize));
        }
        else
        {
            context.Response.Message = response.Message ?? "Unknown error";
        }
    }
}
