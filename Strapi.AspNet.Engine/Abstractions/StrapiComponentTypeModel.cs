using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal class StrapiComponentTypeModel
    {
        [JsonProperty("collectionName")]
        public string CollectionName { get; private set; }

        [JsonProperty("info")]
        public StrapiComponentTypeInfo Info { get; private set; }

        [JsonProperty("options")]
        public object Options { get; private set; }

        [JsonProperty("attributes")]
        public Dictionary<string, StrapiAttributeDescription> Attributes { get; private set; }

        public StrapiComponentTypeModel(string name, string collectionName, Dictionary<string, StrapiAttributeDescription> attributes)
        {
            Options = new object();
            CollectionName = collectionName;
            Info = new StrapiComponentTypeInfo(name, "puzzle-piece");
            Attributes = attributes.ToDictionary(x => x.Key, x => x.Value.ToStrapiAttributeModel());
        }

        public class StrapiComponentTypeInfo
        {
            [JsonProperty("name")]
            public string Name { get; private set; }

            [JsonProperty("icon")]
            public string Icon { get; private set; }

            public StrapiComponentTypeInfo(string name, string icon)
            {
                Name = name;
                Icon = icon;
            }
        }
    }
}