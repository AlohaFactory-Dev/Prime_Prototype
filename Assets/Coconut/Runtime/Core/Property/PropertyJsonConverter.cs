using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aloha.Coconut
{
    public class PropertyJsonConverter : JsonConverter<Property>
    {
        public override void WriteJson(JsonWriter writer, Property value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("property_alias");
            writer.WriteValue(value.type.alias);
            writer.WritePropertyName("amount");
            writer.WriteValue(value.amount.ToString());
            writer.WriteEndObject();
        }

        public override Property ReadJson(JsonReader reader, Type objectType, Property existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var propertyType = jObject.ContainsKey("property_alias") && jObject["property_alias"] != null
                ? PropertyType.Get(jObject["property_alias"].Value<string>()) : null;
            
            var amount = jObject.ContainsKey("amount") && jObject["amount"] != null 
                ? BigInteger.Parse(jObject["amount"].Value<string>()) : 0;
            
            return new Property(propertyType, amount);
        }
    }
}
