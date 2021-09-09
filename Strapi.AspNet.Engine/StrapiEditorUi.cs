using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using MiniSkyLab.Core;
using Strapi.AspNet.Annotations;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Engine
{
    [UsedImplicitly]
    internal class StrapiEditorUi : IStrapiEditorUi
    {
        public StrapiEditorUi(IAppSettings appSettings, IHttpClient httpClient, IStrapiAdmin strapiAdmin,
            IStrapiRepository strapiRepository)
        {
            _httpClient = httpClient;
            _strapiAdmin = strapiAdmin;
            _strapiRepository = strapiRepository;

            strapiAdmin.Authorize(_httpClient);
        }

        public void Configure()
        {
            ConfigureStrapiComponentEditView();
            ConfigureStrapiContentEditView();
        }

        void ConfigureStrapiComponentEditView()
        {
            foreach (var strapiComponentType in _strapiRepository.GetStrapiComponentTypeMetadata())
            {
                var configurationUri = $"{_strapiAdmin.ContentManagerUrl}/components/{strapiComponentType.Uid}/configuration";
                var configurationInfo = _httpClient.SendHttpRequest(HttpMethod.Get, configurationUri);

                var jObject = JObject.Parse(configurationInfo);
                var settingsJObject = (JObject) jObject.GetToken("data.component.settings");
                var layoutsJObject = (JObject) jObject.GetToken("data.component.layouts");
                var metadatasJObject = (JObject) jObject.GetToken("data.component.metadatas");

                SetMainField(settingsJObject, strapiComponentType.CorrespondingDotnetBlockType);

                var editLayout = (JArray) layoutsJObject.GetToken("edit");
                editLayout.Clear();

                var dotnetBlockType = strapiComponentType.CorrespondingDotnetBlockType;
                var dotnetProperties = dotnetBlockType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .OrderBy(x => x.GetCustomAttribute<DisplayAttribute>()?.GetOrder());

                foreach (var dotnetProperty in dotnetProperties)
                {
                    SetFieldLabel(metadatasJObject, dotnetProperty);
                    SetFieldDescription(metadatasJObject, dotnetProperty);
                    SetFieldPlaceholderText(metadatasJObject, dotnetProperty);
                    AddFieldToLayout(editLayout, dotnetProperty);
                }

                configurationInfo = new JObject
                {
                    { "settings", settingsJObject },
                    { "metadatas", metadatasJObject },
                    { "layouts", layoutsJObject }
                }.ToString();

                _httpClient.SendHttpRequest(HttpMethod.Put, configurationUri, configurationInfo);
            }
        }

        void ConfigureStrapiContentEditView()
        {
            foreach (var strapiContentType in _strapiRepository.GetStrapiContentTypeMetadata())
            {
                var configurationUri = $"{_strapiAdmin.ContentManagerUrl}/content-types/{strapiContentType.Uid}/configuration";
                var configurationInfo = _httpClient.SendHttpRequest(HttpMethod.Get, configurationUri);

                var jObject = JObject.Parse(configurationInfo);
                var settingsJObject = (JObject) jObject.GetToken("data.contentType.settings");
                var layoutsJObject = (JObject) jObject.GetToken("data.contentType.layouts");
                var metadatasJObject = (JObject) jObject.GetToken("data.contentType.metadatas");

                SetMainField(settingsJObject, strapiContentType.CorrespondingDotnetPageType);
                SetContentTableColumnVisibility(layoutsJObject);

                var editLayout = (JArray) layoutsJObject.GetToken("edit");
                editLayout.Clear();

                var dotnetPageType = strapiContentType.CorrespondingDotnetPageType;
                var dotnetProperties = dotnetPageType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.GetCustomAttribute<StrapiReservedAttribute>() == null)
                    .OrderBy(x => x.GetCustomAttribute<DisplayAttribute>()?.GetOrder());

                foreach (var dotnetProperty in dotnetProperties)
                {
                    SetFieldLabel(metadatasJObject, dotnetProperty);
                    SetFieldDescription(metadatasJObject, dotnetProperty);
                    SetFieldPlaceholderText(metadatasJObject, dotnetProperty);
                    AddFieldToLayout(editLayout, dotnetProperty);
                }

                configurationInfo = new JObject
                {
                    { "settings", settingsJObject },
                    { "metadatas", metadatasJObject },
                    { "layouts", layoutsJObject }
                }.ToString();

                _httpClient.SendHttpRequest(HttpMethod.Put, configurationUri, configurationInfo);
            }
        }

        static void SetMainField(JObject settingsJObject, IReflect dotnetType)
        {
            var firstProperty = dotnetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.PropertyType == typeof(string))
                .Where(x =>
                {
                    var stringTypeAttribute = x.GetCustomAttribute<StringTypeAttribute>();
                    return stringTypeAttribute == null ||
                           !(StrapiStringType.RichText | StrapiStringType.Password).HasFlag(stringTypeAttribute.StrapiStringType);
                })
                .OrderBy(x => x.GetCustomAttribute<DisplayAttribute>()?.GetOrder())
                .FirstOrDefault();

            if (firstProperty == null)
            {
                return;
            }

            var mainFieldSettings = settingsJObject.GetToken("mainField");
            mainFieldSettings.Replace(firstProperty.Name);
        }

        static void SetFieldLabel(JObject metadatasJObject, MemberInfo dotnetProperty)
        {
            var strapiAttributeName = dotnetProperty.GetCorrespondingStrapiAttributeName();

            var editLabelJToken = metadatasJObject.GetToken($"{dotnetProperty.Name}.edit.label");
            editLabelJToken.Replace(strapiAttributeName);

            var listLabelJToken = metadatasJObject.GetToken($"{dotnetProperty.Name}.list.label");
            listLabelJToken.Replace(strapiAttributeName);
        }

        static void SetFieldDescription(JObject metadatasJObject, MemberInfo dotnetProperty)
        {
            if (!metadatasJObject.TryGetToken($"{dotnetProperty.Name}.edit.description", out var editDescriptionJToken))
            {
                return;
            }

            var strapiAttributeDescription = dotnetProperty.GetCorrespondingStrapiAttributeDescription();
            editDescriptionJToken.Replace(strapiAttributeDescription);
        }

        static void SetFieldPlaceholderText(JObject metadatasJObject, MemberInfo dotnetProperty)
        {
            if (!metadatasJObject.TryGetToken($"{dotnetProperty.Name}.edit.placeholder", out var editPlaceholderJToken))
            {
                return;
            }

            var strapiAttributePlaceholderText = dotnetProperty.GetCorrespondingStrapiAttributePlaceholderText();
            editPlaceholderJToken.Replace(strapiAttributePlaceholderText);
        }

        static void AddFieldToLayout(JArray editLayout, PropertyInfo dotnetProperty)
        {
            editLayout.Add(
                new JArray(
                    new JObject(
                        new JProperty("name", dotnetProperty.Name),
                        new JProperty("size", GetFieldWidth(dotnetProperty))
                    )
                )
            );
        }

        static void SetContentTableColumnVisibility(JObject layoutsJObject)
        {
            var listLayout = (JArray) layoutsJObject.GetToken("list");
            listLayout.Clear();
            listLayout.Add(new JValue("id"));
            listLayout.Add(new JValue(nameof(PageData.PageInstanceName)));
            listLayout.Add(new JValue(nameof(PageData.NameInUrl)));
            listLayout.Add(new JValue("created_at"));
            listLayout.Add(new JValue("updated_at"));
        }

        static int GetFieldWidth(PropertyInfo dotnetProperty)
        {
            if (
                dotnetProperty.PropertyType == typeof(bool) ||
                dotnetProperty.PropertyType == typeof(ContentArea) ||
                dotnetProperty.PropertyType == typeof(string) &&
                dotnetProperty.GetCustomAttribute<StringTypeAttribute>()?.StrapiStringType == StrapiStringType.RichText
            )
            {
                return 12;
            }

            return 6;
        }

        #region Injected Services

        readonly IHttpClient _httpClient;
        readonly IStrapiAdmin _strapiAdmin;
        readonly IStrapiRepository _strapiRepository;

        #endregion
    }
}