// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Azure.Mcp.Tools.DocumentDb.Commands;

internal static class DocumentDbHelpers
{
    public static BsonDocument? ParseBsonDocument(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return BsonDocument.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    public static BsonDocument? ParseBsonDocument(object? value)
    {
        if (value == null)
            return null;

        if (value is string str)
            return ParseBsonDocument(str);

        if (value is BsonDocument doc)
            return doc;

        try
        {
            var json = DocumentDbResponseHelper.SerializeToJson(value);
            return BsonDocument.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    public static List<BsonDocument>? ParseBsonDocumentList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var bsonArray = BsonSerializer.Deserialize<BsonArray>(json);
            return bsonArray.Select(item => item.AsBsonDocument).ToList();
        }
        catch
        {
            return null;
        }
    }

    public static List<BsonDocument>? ParseBsonDocumentList(object? value)
    {
        if (value == null)
            return null;

        if (value is string str)
            return ParseBsonDocumentList(str);

        if (value is List<BsonDocument> list)
            return list;

        try
        {
            var json = DocumentDbResponseHelper.SerializeToJson(value);
            return ParseBsonDocumentList(json);
        }
        catch
        {
            return null;
        }
    }

    public static bool ParseBoolean(string? value, bool defaultValue = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        if (bool.TryParse(value, out var result))
            return result;

        // Handle common string representations
        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "1" or "yes" => true,
            "false" or "0" or "no" => false,
            _ => defaultValue
        };
    }

    public static int ParseInt(string? value, int defaultValue = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    public static string SerializeBsonToJson(BsonDocument document)
    {
        var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson };
        return document.ToJson(jsonWriterSettings);
    }

    public static string SerializeBsonToJson(object obj)
    {
        if (obj is BsonDocument doc)
            return SerializeBsonToJson(doc);

        return DocumentDbResponseHelper.SerializeToJson(obj);
    }
}
