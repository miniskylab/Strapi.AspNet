using System.Collections.Generic;
using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal class StrapiContentTypeDto
    {
        [JsonProperty("components")]
        public StrapiComponentTypeDto.StrapiComponentTypeDescription[] ComponentTypeDescriptions { get; private set; }

        [JsonProperty("contentType")]
        public StrapiContentTypeDescription ContentTypeDescription { get; private set; }

        public StrapiContentTypeDto(StrapiContentTypeDescription strapiContentTypeDescription)
        {
            ContentTypeDescription = strapiContentTypeDescription;
            ComponentTypeDescriptions = System.Array.Empty<StrapiComponentTypeDto.StrapiComponentTypeDescription>();
        }

        public class StrapiContentTypeDescription
        {
            [JsonProperty("attributes")]
            public Dictionary<string, StrapiAttributeDescription> Attributes { get; private set; }

            [JsonProperty("collectionName")]
            public string CollectionName { get; private set; }

            [JsonProperty("description")]
            public string Description { get; private set; }

            [JsonProperty("draftAndPublish")]
            public bool EnableDraftAndPublish { get; private set; }

            [JsonProperty("kind")]
            public StrapiContentTypeKind Kind { get; private set; }

            [JsonProperty("name")]
            public string Name { get; private set; }

            public StrapiContentTypeDescription(string name, Dictionary<string, StrapiAttributeDescription> attributes,
                string collectionName)
            {
                Kind = StrapiContentTypeKind.CollectionType;
                EnableDraftAndPublish = true;
                Description = string.Empty;

                Name = name;
                Attributes = attributes;
                CollectionName = collectionName;
            }
        }
    }
}