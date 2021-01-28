using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Strapi.AspNet.DataModel
{
    public class AssumedUtcDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var dateTime = JToken.Load(reader).Value<DateTime>();
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            return dateTime;
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer) { new JValue(value).WriteTo(writer); }
    }
}