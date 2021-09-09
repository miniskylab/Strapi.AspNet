using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MiniSkyLab.Core;

namespace Strapi.AspNet.DataModel
{
    internal class MediaJsonConverter : JsonConverter<Media>
    {
        readonly ThreadLocal<bool> _skipThisAndUseDefaultJsonConverter = new(() => false);

        public override bool CanRead => !_skipThisAndUseDefaultJsonConverter.Value || (_skipThisAndUseDefaultJsonConverter.Value = false);

        public override bool CanWrite => !_skipThisAndUseDefaultJsonConverter.Value || (_skipThisAndUseDefaultJsonConverter.Value = false);

        public override Media ReadJson(JsonReader reader, Type objectType, Media existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var httpsEnabled = IAppSettings.Instance.GetSection("EnableHttps").Exists() && IAppSettings.Instance.Get<bool>("EnableHttps");
            var protocol = httpsEnabled ? "https" : "http";
            var strapiBaseUrl = IAppSettings.Instance.GetSection("Strapi:DomainName").Exists()
                ? $"{protocol}://{IAppSettings.Instance.Get("Strapi:DomainName")}"
                : $"{protocol}://{INetwork.Instance.LocalIpAddress}:{IAppSettings.Instance.Get("Strapi:Port")}";

            _skipThisAndUseDefaultJsonConverter.Value = true;
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var media = JObject.Load(reader).ToObject<Media>(serializer);
            if (string.IsNullOrWhiteSpace(media?.Url))
            {
                return media;
            }

            ConvertRelativeToAbsoluteUrl(media, strapiBaseUrl);
            ConvertRelativeToAbsoluteUrl(media.Formats?.Large, strapiBaseUrl);
            ConvertRelativeToAbsoluteUrl(media.Formats?.Medium, strapiBaseUrl);
            ConvertRelativeToAbsoluteUrl(media.Formats?.Small, strapiBaseUrl);
            ConvertRelativeToAbsoluteUrl(media.Formats?.Thumbnail, strapiBaseUrl);

            return media;
        }

        public override void WriteJson(JsonWriter writer, Media value, JsonSerializer serializer)
        {
            var jObject = JObject.FromObject(value);
            jObject.WriteTo(writer);
        }

        static void ConvertRelativeToAbsoluteUrl(MediaInfo mediaInfo, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(mediaInfo?.Url))
            {
                return;
            }

            var absoluteMediaUrl = $"{baseUrl}{mediaInfo.Url}";
            var mediaUrlProperty = typeof(MediaInfo).GetProperty(nameof(MediaInfo.Url));
            mediaUrlProperty!.SetValue(mediaInfo, absoluteMediaUrl);
        }
    }
}