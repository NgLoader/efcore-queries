using NgLoader.Queries.Dto;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NgLoader.Queries.Converters
{
    public class QueryFilterConverter : JsonConverter<IQueryFilter>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IQueryFilter) == typeToConvert;
        }

        public override IQueryFilter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement rootElement = document.RootElement;
                if (Enum.TryParse(rootElement.GetProperty("filterType").GetString(), true, out QueryFilterType filterType))
                {
                    return filterType switch
                    {
                        QueryFilterType.Condition => JsonSerializer.Deserialize<QueryFilterCondition>(rootElement.GetRawText(), options),
                        QueryFilterType.Composition => JsonSerializer.Deserialize<QueryFilterComposition>(rootElement.GetRawText(), options),
                        _ => throw new JsonException($"Unkown filter type: {filterType}")
                    };
                }
            }
            throw new JsonException("property filterType is missing.");
        }

        public override void Write(Utf8JsonWriter writer, IQueryFilter value, JsonSerializerOptions options)
        {
            if (value.FilterType == QueryFilterType.Condition)
            {
                writer.WriteRawValue(JsonSerializer.Serialize((QueryFilterCondition)value));
            } else if (value.FilterType == QueryFilterType.Composition)
            {
                writer.WriteRawValue(JsonSerializer.Serialize((QueryFilterComposition)value));
            } else
            {
                throw new JsonException("unknown value type");
            }
        }
    }
}
