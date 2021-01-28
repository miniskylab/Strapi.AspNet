using System.Collections.Generic;
using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal class StrapiComponentTypeDto
    {
        [JsonProperty("component")]
        public StrapiComponentTypeDescription ComponentTypeDescription { get; private set; }

        [JsonProperty("components")]
        public StrapiComponentTypeDescription[] ComponentTypeDescriptions { get; private set; }

        public StrapiComponentTypeDto(StrapiComponentTypeDescription strapiComponentTypeDescription)
        {
            ComponentTypeDescription = strapiComponentTypeDescription;
            ComponentTypeDescriptions = System.Array.Empty<StrapiComponentTypeDescription>();
        }

        public class StrapiComponentTypeDescription
        {
            [JsonProperty("attributes")]
            public Dictionary<string, StrapiAttributeDescription> Attributes { get; private set; }

            [JsonProperty("category")]
            public string Category { get; private set; }

            [JsonProperty("collectionName")]
            public string CollectionName { get; private set; }

            [JsonProperty("icon")]
            public string Icon { get; private set; }

            [JsonProperty("name")]
            public string Name { get; private set; }

            public StrapiComponentTypeDescription(string name, string category, Dictionary<string, StrapiAttributeDescription> attributes,
                string collectionName)
            {
                Icon = "puzzle-piece";

                Name = name;
                Category = category;
                Attributes = attributes;
                CollectionName = collectionName;
            }
        }
    }
}