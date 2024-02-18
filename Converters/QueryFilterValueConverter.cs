using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imprex.Queries.Converters
{
    internal class QueryFilterValueConverter : JsonConverter<object>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    if (reader.TryGetDateTimeOffset(out DateTimeOffset dateTimeOffset))
                    {
                        return dateTimeOffset;
                    }

                    return reader.GetString();

                case JsonTokenType.Number:
                    return reader.GetSingle();

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException($"Unsupported json type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
