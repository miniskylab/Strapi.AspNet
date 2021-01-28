using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Strapi.AspNet.DataModel
{
    [JsonConverter(typeof(MediaJsonConverter))]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Media : MediaInfo
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("alternativeText")]
        public string AlternativeText { get; private set; }

        [JsonProperty("caption")]
        public string Caption { get; private set; }

        [CanBeNull]
        [JsonProperty("formats")]
        public MediaFormat Formats { get; private set; }
    }

    public class MediaFormat
    {
        [CanBeNull]
        [JsonProperty("large")]
        public MediaInfo Large { get; private set; }

        [CanBeNull]
        [JsonProperty("medium")]
        public MediaInfo Medium { get; private set; }

        [CanBeNull]
        [JsonProperty("small")]
        public MediaInfo Small { get; private set; }

        [CanBeNull]
        [JsonProperty("thumbnail")]
        public MediaInfo Thumbnail { get; private set; }
    }

    public class MediaInfo
    {
        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("ext")]
        public string Extension { get; private set; }

        [JsonProperty("width")]
        public string PxWidth { get; private set; }

        [JsonProperty("height")]
        public string PxHeight { get; private set; }

        [JsonProperty("mime")]
        public string MimeType { get; private set; }

        [JsonProperty("size")]
        public double KbSize { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }
    }
}