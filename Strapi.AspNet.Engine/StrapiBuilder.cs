using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orbital.Core;
using Strapi.AspNet.Annotations;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Engine
{
    [UsedImplicitly]
    internal class StrapiBuilder : IStrapiBuilder
    {
        readonly string _strapiContentTypeBuilderUri;
        readonly string _pathToStrapiInstallationDirectory;

        public StrapiBuilder(IAppSettings appSettings, IStrapiRepository strapiRepository, IStrapiProcess strapiProcess,
            IHttpClient httpClient, IStrapiAdmin strapiAdmin, IAssemblyScanner assemblyScanner)
        {
            _httpClient = httpClient;
            _strapiProcess = strapiProcess;
            _assemblyScanner = assemblyScanner;
            _strapiRepository = strapiRepository;

            strapiAdmin.Authorize(_httpClient);
            _strapiContentTypeBuilderUri = $"{strapiAdmin.BaseUrl}/content-type-builder";
            _pathToStrapiInstallationDirectory = Path.Combine(appSettings.PathToWorkingDirectory, "strapi");
        }

        public void BuildStrapiTypesFromDotnetTypes()
        {
            DeleteOrphanedStrapiComponentTypes();
            DeleteOrphanedStrapiContentTypes();

            CreateOrOverwriteStrapiComponentTypes();

            CreateNewStrapiContentTypes();
            RefineExistingStrapiContentTypes();

            _strapiProcess.Restart();
        }

        void DeleteOrphanedStrapiComponentTypes()
        {
            var strapiComponentTypes = _strapiRepository.GetStrapiComponentTypeMetadata();
            var orphanedStrapiComponentTypes = strapiComponentTypes.Where(x => x.CorrespondingDotnetBlockType == null).ToList();

            foreach (var orphanedStrapiComponentType in orphanedStrapiComponentTypes)
            {
                _httpClient.SendHttpRequest(
                    HttpMethod.Delete,
                    $"{_strapiContentTypeBuilderUri}/components/{orphanedStrapiComponentType.Uid}"
                );
            }
        }

        void DeleteOrphanedStrapiContentTypes()
        {
            var strapiContentTypes = _strapiRepository.GetStrapiContentTypeMetadata();
            var orphanedStrapiContentTypes = strapiContentTypes.Where(x => x.CorrespondingDotnetPageType == null).ToList();

            foreach (var orphanedStrapiContentType in orphanedStrapiContentTypes)
            {
                _httpClient.SendHttpRequest(
                    HttpMethod.Delete,
                    $"{_strapiContentTypeBuilderUri}/content-types/{orphanedStrapiContentType.Uid}"
                );
            }
        }

        void CreateOrOverwriteStrapiComponentTypes()
        {
            var dotnetBlockTypes = _assemblyScanner.Types.Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(BlockData))).ToList();
            foreach (var notMappedDotnetBlockType in dotnetBlockTypes)
            {
                var strapiComponentTypeModel = ToStrapiComponentTypeModel(notMappedDotnetBlockType);
                var strapiComponentTypeCategory = notMappedDotnetBlockType.GetCorrespondingStrapiComponentTypeCategory();
                var pathToStrapiComponentTypeJsonFile = Path.Combine(
                    _pathToStrapiInstallationDirectory,
                    $"components/{strapiComponentTypeCategory}/{strapiComponentTypeModel.CollectionName}.json"
                );

                var strapiComponentTypeJsonFileInfo = new FileInfo(pathToStrapiComponentTypeJsonFile);
                strapiComponentTypeJsonFileInfo.Directory!.Create();
                File.WriteAllText(
                    pathToStrapiComponentTypeJsonFile,
                    strapiComponentTypeModel.ToJson(Formatting.Indented, Json.DefaultJsonSerializerSettings)
                );
            }

            _strapiProcess.Restart();
        }

        void CreateNewStrapiContentTypes()
        {
            var mappedDotnetPageTypes = _strapiRepository.GetStrapiContentTypeMetadata().Select(x => x.CorrespondingDotnetPageType);
            var notMappedDotnetPageTypes = _assemblyScanner.Types
                .Where(x => x.IsSubclassOf(typeof(PageData)))
                .Except(mappedDotnetPageTypes)
                .ToList();

            foreach (var notMappedDotnetPageType in notMappedDotnetPageTypes)
            {
                var strapiContentTypesUri = $"{_strapiContentTypeBuilderUri}/content-types";
                var strapiContentTypeDto = ToStrapiContentTypeDto(notMappedDotnetPageType, true);

                _httpClient.SendHttpRequest(
                    HttpMethod.Post,
                    strapiContentTypesUri,
                    strapiContentTypeDto.ToJson(settings: Json.DefaultJsonSerializerSettings)
                );
            }

            _strapiProcess.Restart();
        }

        void RefineExistingStrapiContentTypes()
        {
            foreach (var strapiContentType in _strapiRepository.GetStrapiContentTypeMetadata())
            {
                var strapiContentTypeUri = $"{_strapiContentTypeBuilderUri}/content-types/{strapiContentType.Uid}";
                var strapiContentTypeDto = ToStrapiContentTypeDto(strapiContentType.CorrespondingDotnetPageType);

                _httpClient.SendHttpRequest(
                    HttpMethod.Put,
                    strapiContentTypeUri,
                    strapiContentTypeDto.ToJson(settings: Json.DefaultJsonSerializerSettings)
                );

                SetStrapiContentTypeRoute(strapiContentType);
            }
        }

        static StrapiComponentTypeModel ToStrapiComponentTypeModel(Type dotnetBlockType)
        {
            var dotnetBlockTypeGuid = dotnetBlockType.GetGuid();
            var strapiComponentTypeName = GetCorrespondingStrapiTypeName(dotnetBlockType);

            var strapiComponentTypeAttributes = dotnetBlockType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(x => x.Name, ToStrapiAttributeDescription);

            return new StrapiComponentTypeModel(strapiComponentTypeName, dotnetBlockTypeGuid, strapiComponentTypeAttributes);
        }

        static StrapiContentTypeDto ToStrapiContentTypeDto(Type dotnetPageType, bool useDotnetPageTypeGuidAsName = false)
        {
            var dotnetPageTypeGuid = dotnetPageType.GetGuid();
            var strapiContentTypeName = useDotnetPageTypeGuidAsName
                ? dotnetPageTypeGuid
                : GetCorrespondingStrapiTypeName(dotnetPageType);

            var strapiContentTypeAttributes = dotnetPageType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<StrapiReservedAttribute>() == null)
                .ToDictionary(x => x.Name, ToStrapiAttributeDescription);

            return new StrapiContentTypeDto(
                new StrapiContentTypeDto.StrapiContentTypeDescription(
                    strapiContentTypeName,
                    strapiContentTypeAttributes,
                    dotnetPageTypeGuid
                )
            );
        }

        static string GetCorrespondingStrapiTypeName(MemberInfo dotnetBlockType)
        {
            var contentTypeAttribute = dotnetBlockType.GetCustomAttribute<ContentTypeAttribute>();
            return contentTypeAttribute?.DisplayName ?? dotnetBlockType.Name;
        }

        static StrapiAttributeDescription ToStrapiAttributeDescription(PropertyInfo dotnetProperty)
        {
            var isUnique = dotnetProperty.GetCustomAttribute<UniqueAttribute>() != null;
            var isPrivate = dotnetProperty.GetCustomAttribute<PrivateAttribute>() != null;
            var isDateOnly = dotnetProperty.GetCustomAttribute<DateOnlyAttribute>() != null;
            var isRequired = dotnetProperty.GetCustomAttribute<RequiredAttribute>() != null;
            var minLength = dotnetProperty.GetCustomAttribute<MinLengthAttribute>()?.Length;
            var maxLength = dotnetProperty.GetCustomAttribute<MaxLengthAttribute>()?.Length;
            var minItemCount = dotnetProperty.GetCustomAttribute<MinItemCountAttribute>()?.MinItemCount;
            var maxItemCount = dotnetProperty.GetCustomAttribute<MaxItemCountAttribute>()?.MaxItemCount;
            var targetFieldName = dotnetProperty.GetCustomAttribute<UidForAttribute>()?.TargetFieldName;
            var stringType = dotnetProperty.GetCustomAttribute<StringTypeAttribute>()?.StrapiStringType ?? StrapiStringType.String;
            var selectionFactoryType = dotnetProperty.GetCustomAttribute<SelectOneAttribute>()?.SelectionFactory;

            GetDefaultValue(dotnetProperty, out var defaultValue);
            GetMinValueAndMaxValue(dotnetProperty, out var minValue, out var maxValue);
            GetAllowedMediaTypes(dotnetProperty, out var allowedMediaTypes);

            var allowedBlockTypes = Array.Empty<string>();
            if (dotnetProperty.PropertyType == typeof(ContentArea))
            {
                GetAllowedBlockTypes(dotnetProperty, out allowedBlockTypes);
            }

            switch (dotnetProperty.PropertyType)
            {
                case var __ when __ == typeof(string):
                {
                    if (selectionFactoryType == null)
                    {
                        return targetFieldName != null
                            ? new StrapiUidAttributeDescription(
                                (string) defaultValue,
                                targetFieldName,
                                minLength,
                                maxLength,
                                isPrivate,
                                isRequired
                            )
                            : new StrapiStringAttributeDescription(
                                (string) defaultValue,
                                stringType,
                                minLength,
                                maxLength,
                                isPrivate,
                                isRequired,
                                isUnique
                            );
                    }

                    return new StrapiEnumerationAttributeDescription(
                        (string) defaultValue,
                        selectionFactoryType.GetSelections().Select(x => x.Value),
                        isPrivate,
                        isRequired,
                        isUnique
                    );
                }

                case var __ when __ == typeof(bool):
                    return new StrapiBooleanAttributeDescription((bool) defaultValue, isPrivate, isUnique);

                case var __ when __ == typeof(bool?):
                    return new StrapiNullableBooleanAttributeDescription((bool?) defaultValue, isPrivate, isUnique);

                case var __ when __ == typeof(int):
                case var ___ when ___ == typeof(short):
                    return new StrapiIntegerAttributeDescription((int) defaultValue, minValue, maxValue, isPrivate, isUnique);

                case var __ when __ == typeof(int?):
                case var ___ when ___ == typeof(short?):
                    return new StrapiNullableIntegerAttributeDescription(
                        (int?) defaultValue,
                        minValue,
                        maxValue,
                        isPrivate,
                        isRequired,
                        isUnique
                    );

                case var __ when __ == typeof(long):
                    return new StrapiBigIntegerAttributeDescription((long) defaultValue, minValue, maxValue, isPrivate, isUnique);

                case var __ when __ == typeof(long?):
                    return new StrapiNullableBigIntegerAttributeDescription(
                        (long?) defaultValue,
                        minValue,
                        maxValue,
                        isPrivate,
                        isRequired,
                        isUnique
                    );

                case var __ when __ == typeof(float):
                case var ___ when ___ == typeof(double):
                    return new StrapiDoubleAttributeDescription((double) defaultValue, minValue, maxValue, isPrivate, isUnique);

                case var __ when __ == typeof(float?):
                case var ___ when ___ == typeof(double?):
                    return new StrapiNullableDoubleAttributeDescription(
                        (double?) defaultValue,
                        minValue,
                        maxValue,
                        isPrivate,
                        isRequired,
                        isUnique
                    );

                case var __ when __ == typeof(decimal):
                    return new StrapiDecimalAttributeDescription((decimal) defaultValue, minValue, maxValue, isPrivate, isUnique);

                case var __ when __ == typeof(decimal?):
                    return new StrapiNullableDecimalAttributeDescription(
                        (decimal?) defaultValue,
                        minValue,
                        maxValue,
                        isPrivate,
                        isRequired,
                        isUnique
                    );

                case var __ when __ == typeof(DateTimeOffset):
                    return new StrapiDateTimeOffsetAttributeDescription();

                case var __ when __ == typeof(DateTimeOffset?):
                    return new StrapiNullableDateTimeOffsetAttributeDescription();

                case var __ when __ == typeof(DateTime):
                    return new StrapiDateTimeAttributeDescription((DateTime) defaultValue, isPrivate, isUnique, isDateOnly);

                case var __ when __ == typeof(DateTime?):
                    return new StrapiNullableDateTimeAttributeDescription(
                        (DateTime?) defaultValue,
                        isPrivate,
                        isRequired,
                        isUnique,
                        isDateOnly
                    );

                case var __ when __ == typeof(TimeSpan):
                    return new StrapiTimeAttributeDescription((TimeSpan) defaultValue, isPrivate, isUnique);

                case var __ when __ == typeof(TimeSpan?):
                    return new StrapiNullableTimeAttributeDescription((TimeSpan?) defaultValue, isPrivate, isRequired, isUnique);

                case var __ when __ == typeof(Media):
                    return new StrapiMediaAttributeDescription(allowedMediaTypes, false, isPrivate, isRequired, isUnique);

                case var propertyType when propertyType.IsArray && propertyType.GetElementType() == typeof(Media):
                    return new StrapiMediaAttributeDescription(allowedMediaTypes, true, isPrivate, isRequired, isUnique);

                case var propertyType when propertyType == typeof(ContentArea):
                {
                    if (dotnetProperty.DeclaringType.IsSubclassOf(typeof(PageData)))
                    {
                        return new StrapiDynamicZoneAttributeDescription(allowedBlockTypes, minItemCount, maxItemCount, isRequired);
                    }

                    throw new NotSupportedException(
                        $"[{dotnetProperty.DeclaringType}] does not support [{nameof(ContentArea)}] data type. " +
                        $"[{nameof(ContentArea)}] data type is only supported on types that inherit from [{typeof(PageData)}] type"
                    );
                }

                case var propertyType when propertyType.IsArray && propertyType.GetElementType().IsSubclassOf(typeof(BlockData)):
                {
                    return new StrapiComponentAttributeDescription(
                        propertyType.GetElementType().GetCorrespondingStrapiTypeId(),
                        true,
                        minItemCount,
                        maxItemCount,
                        isRequired
                    );
                }

                case var propertyType when propertyType.IsSubclassOf(typeof(BlockData)):
                {
                    return new StrapiComponentAttributeDescription(
                        propertyType.GetCorrespondingStrapiTypeId(),
                        false,
                        null,
                        null,
                        isRequired
                    );
                }

                default:
                    throw new NotSupportedException($"{dotnetProperty.PropertyType} data type is not supported");
            }
        }

        static void GetDefaultValue(PropertyInfo dotnetProperty, out object defaultValue)
        {
            if (!InMemoryCache.TryGet(dotnetProperty.ReflectedType!.AssemblyQualifiedName, out var reflectedObject))
            {
                reflectedObject = Activator.CreateInstance(dotnetProperty.ReflectedType!);
                dotnetProperty.ReflectedType.GetMethod(nameof(ContentData.SetDefaultValues))!.Invoke(reflectedObject, null);
                InMemoryCache.Store(dotnetProperty.ReflectedType!.AssemblyQualifiedName, reflectedObject);
            }

            defaultValue = dotnetProperty.GetValue(reflectedObject);
        }

        static void GetMinValueAndMaxValue(MemberInfo dotnetProperty, out int? minValue, out int? maxValue)
        {
            minValue = null;
            maxValue = null;

            var rangeAttribute = dotnetProperty.GetCustomAttribute<RangeAttribute>();
            if (!(rangeAttribute?.Minimum is int minimum) || !(rangeAttribute.Maximum is int maximum))
            {
                return;
            }

            minValue = minimum;
            maxValue = maximum;
        }

        static void GetAllowedMediaTypes(MemberInfo dotnetProperty, out StrapiMediaType[] allowedMediaTypes)
        {
            var allowedMediaTypesAttribute = dotnetProperty.GetCustomAttribute<AllowedMediaTypesAttribute>();
            allowedMediaTypes = allowedMediaTypesAttribute?.AllowedMediaTypes != null && allowedMediaTypesAttribute.AllowedMediaTypes.Any()
                ? new HashSet<StrapiMediaType>(allowedMediaTypesAttribute.AllowedMediaTypes).ToArray()
                : null;
        }

        static void GetAllowedBlockTypes(PropertyInfo dotnetProperty, out string[] allowedBlockTypes)
        {
            allowedBlockTypes = dotnetProperty.GetAllowedBlockTypes()
                .Select(x => x.GetCorrespondingStrapiTypeId())
                .ToArray();
        }

        void SetStrapiContentTypeRoute(StrapiContentTypeMetadata strapiContentType)
        {
            var pathToStrapiContentTypeRouteSettingsFile = Path.Combine(
                _pathToStrapiInstallationDirectory,
                $"api/{strapiContentType.ApiId}/config/routes.json"
            );

            var routeObject = JObject.Parse(File.ReadAllText(pathToStrapiContentTypeRouteSettingsFile));
            var routeArray = (JArray) routeObject.GetToken("routes");
            foreach (var route in routeArray.Children<JObject>())
            {
                var routeToken = route.GetToken("path");
                var routeValue = (string) routeToken;

                if (string.IsNullOrEmpty(routeValue))
                    continue;

                routeValue = Regex.Replace(routeValue, "^.+(?=/)|^.+", $"/{strapiContentType.Uid}");
                routeToken.Replace(routeValue);
            }

            File.WriteAllText(pathToStrapiContentTypeRouteSettingsFile, routeObject.ToString(Formatting.Indented));
        }

        #region Injected Services

        readonly IHttpClient _httpClient;
        readonly IStrapiProcess _strapiProcess;
        readonly IAssemblyScanner _assemblyScanner;
        readonly IStrapiRepository _strapiRepository;

        #endregion
    }
}