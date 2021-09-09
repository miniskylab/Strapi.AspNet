using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MiniSkyLab.Core;

namespace Strapi.AspNet.Engine
{
    public static class StrapiExtensions
    {
        public static string SendHttpRequest(this IHttpClient httpClient, HttpMethod httpMethod, string uri, string body = null)
        {
            var httpRequestMessage = new HttpRequestMessage(httpMethod, uri);
            var requestBodyIsRequired = httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put;
            if (requestBodyIsRequired)
            {
                if (string.IsNullOrEmpty(body))
                {
                    throw new ArgumentNullException($"{httpMethod.Method} requires body.");
                }

                httpRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            var httpResponseMessage = httpClient.SendAsync(httpRequestMessage).Result;
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new StrapiException(
                    $"{httpMethod.Method} {uri} {(int) httpResponseMessage.StatusCode}" +
                    $"\n\n{httpResponseMessage.RequestMessage}" +
                    (requestBodyIsRequired ? $"\n\nRequest Body:\n{JObject.Parse(body).ToString(Formatting.Indented)}" : string.Empty) +
                    $"\n\nResponse content: {httpResponseMessage.Content.ReadAsStringAsync().Result}\n"
                );
            }

            return httpResponseMessage.Content.ReadAsStringAsync().Result;
        }

        internal static string GetCorrespondingStrapiAttributeName(this MemberInfo dotnetProperty)
        {
            var displayAttribute = dotnetProperty.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.GetName() ?? dotnetProperty.Name;
        }

        internal static string GetCorrespondingStrapiAttributeDescription(this MemberInfo dotnetProperty)
        {
            var displayAttribute = dotnetProperty.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.GetDescription() ?? string.Empty;
        }

        internal static string GetCorrespondingStrapiAttributePlaceholderText(this MemberInfo dotnetProperty)
        {
            var displayAttribute = dotnetProperty.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.GetPrompt() ?? string.Empty;
        }
    }
}