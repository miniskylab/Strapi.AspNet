using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MiniSkyLab.Core;

namespace Strapi.AspNet.DataModel
{
    internal class ContentAreaJsonConverter : JsonConverter<ContentArea>
    {
        public override ContentArea ReadJson(JsonReader reader, Type objectType, ContentArea existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var jArray = JArray.Load(reader);

            return new ContentArea((
                from jObject in jArray.Children<JObject>()
                let strapiComponentId = (string) jObject.GetToken("__component")
                let dotnetBlockType = IAssemblyScanner.Instance.Types.Single(x =>
                    !x.IsAbstract &&
                    x.IsSubclassOf(typeof(BlockData)) &&
                    x.GetCorrespondingStrapiTypeId() == strapiComponentId
                )
                select (BlockData) jObject.ToObject(dotnetBlockType, serializer)
            ).ToArray());
        }

        public override void WriteJson(JsonWriter writer, ContentArea value, JsonSerializer serializer)
        {
            var jArray = JArray.FromObject(value.Items);
            var jObjects = jArray.Children<JObject>().ToArray();
            for (var i = 0; i < jObjects.Length; i++)
            {
                jObjects[i].Add("__component", new JValue(value.Items[i].GetType().GetCorrespondingStrapiTypeId()));
            }

            jArray.WriteTo(writer);
        }
    }
}