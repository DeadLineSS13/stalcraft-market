using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;


namespace SlacrafratMarketDiscordBot.Objects
{
    public struct ItemInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("name")]
        public Name NameInfo { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("status")]
        public Status StatusInfo { get; set; }

        [JsonProperty("infoBlocks")]
        public InfoBlock[] InfoBlocks { get; set; }

        public partial class InfoBlock
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("title")]
            public Name Title { get; set; }

            [JsonProperty("elements", NullValueHandling = NullValueHandling.Ignore)]
            public Element[] Elements { get; set; }

            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public Name Text { get; set; }
        }

        public partial class Element
        {
            [JsonProperty("type")]
            public ElementType Type { get; set; }

            [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
            public Name Key { get; set; }

            [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
            public Value? Value { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public Name Name { get; set; }

            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public Name Text { get; set; }
        }

        public partial class Name
        {
            [JsonProperty("type")]
            public NameType Type { get; set; }

            [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
            public string Key { get; set; }

            [JsonProperty("args", NullValueHandling = NullValueHandling.Ignore)]
            public Args Args { get; set; }

            [JsonProperty("lines", NullValueHandling = NullValueHandling.Ignore)]
            public Lines Lines { get; set; }

            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public string Text { get; set; }
        }

        public partial class Args
        {
        }

        public partial class Lines
        {
            [JsonProperty("ru")]
            public string Ru { get; set; }

            [JsonProperty("en")]
            public string En { get; set; }
        }

        public partial class Status
        {
            [JsonProperty("state")]
            public string State { get; set; }
        }

        public enum NameType { Text, Translation };

        public enum ElementType { KeyValue, Numeric, Text, Item, Range };

        public partial struct Value
        {
            public double? Double;
            public Name Name;

            public static implicit operator Value(double Double) => new Value { Double = Double };
            public static implicit operator Value(Name Name) => new Value { Name = Name };
        }

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                NameTypeConverter.Singleton,
                ElementTypeConverter.Singleton,
                ValueConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        internal class NameTypeConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(NameType) || t == typeof(NameType?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value)
                {
                    case "text":
                        return NameType.Text;
                    case "translation":
                        return NameType.Translation;
                }
                throw new Exception("Cannot unmarshal type NameType");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (NameType)untypedValue;
                switch (value)
                {
                    case NameType.Text:
                        serializer.Serialize(writer, "text");
                        return;
                    case NameType.Translation:
                        serializer.Serialize(writer, "translation");
                        return;
                }
                throw new Exception("Cannot marshal type NameType");
            }

            public static readonly NameTypeConverter Singleton = new NameTypeConverter();
        }

        internal class ElementTypeConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(ElementType) || t == typeof(ElementType?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value)
                {
                    case "key-value":
                        return ElementType.KeyValue;
                    case "numeric":
                        return ElementType.Numeric;
                    case "text":
                        return ElementType.Text;
                    case "item":
                        return ElementType.Item;
                    case "range":
                        return ElementType.Range;
                }
                throw new Exception("Cannot unmarshal type ElementType");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (ElementType)untypedValue;
                switch (value)
                {
                    case ElementType.KeyValue:
                        serializer.Serialize(writer, "key-value");
                        return;
                    case ElementType.Numeric:
                        serializer.Serialize(writer, "numeric");
                        return;
                    case ElementType.Text:
                        serializer.Serialize(writer, "text");
                        return;
                }
                throw new Exception("Cannot marshal type ElementType");
            }

            public static readonly ElementTypeConverter Singleton = new ElementTypeConverter();
        }

        internal class ValueConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(Value) || t == typeof(Value?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Integer:
                    case JsonToken.Float:
                        var doubleValue = serializer.Deserialize<double>(reader);
                        return new Value { Double = doubleValue };
                    case JsonToken.StartObject:
                        var objectValue = serializer.Deserialize<Name>(reader);
                        return new Value { Name = objectValue };
                }
                throw new Exception("Cannot unmarshal type Value");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                var value = (Value)untypedValue;
                if (value.Double != null)
                {
                    serializer.Serialize(writer, value.Double.Value);
                    return;
                }
                if (value.Name != null)
                {
                    serializer.Serialize(writer, value.Name);
                    return;
                }
                throw new Exception("Cannot marshal type Value");
            }

            public static readonly ValueConverter Singleton = new ValueConverter();
        }
    }
}
