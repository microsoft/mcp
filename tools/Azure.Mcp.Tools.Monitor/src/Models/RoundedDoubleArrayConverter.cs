// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models
{
    /// <summary>
    /// Custom JSON converter that rounds double arrays to 2 decimal places
    /// </summary>
    public class RoundedDoubleArrayConverter : JsonConverter<double[]?>
    {
        public override double[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var list = new List<double>();
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    list.Add(reader.GetDouble());
                }
            }
            return [.. list];
        }

        public override void Write(Utf8JsonWriter writer, double[]? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteNumberValue(Math.Round(item, 2));
            }
            writer.WriteEndArray();
        }
    }
}
