using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Orbital.Core;
using Strapi.AspNet.Annotations;

namespace Strapi.AspNet.DataModel
{
    public static class StrapiExtensions
    {
        public static string GetGuid(this Type dotnetType)
        {
            dotnetType.EnsureContentType();

            var contentTypeAttribute = dotnetType.GetCustomAttribute<ContentTypeAttribute>();
            if (contentTypeAttribute == null)
                throw new InvalidConstraintException($"[{dotnetType.Name}] type must be decorated with [{nameof(ContentTypeAttribute)}].");

            return contentTypeAttribute.Guid;
        }

        public static string GetCorrespondingStrapiTypeId(this Type dotnetType)
        {
            dotnetType.EnsureContentType();

            switch (dotnetType)
            {
                case var dotnetPageType when dotnetPageType.IsSubclassOf(typeof(PageData)):
                {
                    var dotnetPageTypeGuid = dotnetPageType.GetGuid();
                    return $"application::{dotnetPageTypeGuid}.{dotnetPageTypeGuid}".ToLowerInvariant();
                }

                case var dotnetBlockType when dotnetBlockType.IsSubclassOf(typeof(BlockData)):
                {
                    var strapiComponentTypeCategory = dotnetBlockType.GetCorrespondingStrapiComponentTypeCategory();
                    var dotnetBlockTypeGuid = dotnetBlockType.GetGuid();
                    return $"{strapiComponentTypeCategory}.{dotnetBlockTypeGuid}".ToLowerInvariant();
                }

                default:
                    throw new NotSupportedException(
                        $"[{dotnetType}] is neither subclass of [{nameof(PageData)}] nor [{nameof(BlockData)}]"
                    );
            }
        }

        public static JToken GetToken(this JObject jObject, string jPath)
        {
            var jToken = jObject.SelectToken(jPath);
            if (jToken == null)
                throw new FormatException($"[{jPath}] entry not found. Probably because Strapi API has changed.");

            return jToken;
        }

        public static bool TryGetToken(this JObject jObject, string jPath, out JToken jToken)
        {
            jToken = jObject.SelectToken(jPath);
            return jToken != null;
        }

        public static string GetCorrespondingStrapiComponentTypeCategory(this Type dotnetBlockType)
        {
            dotnetBlockType.EnsureBlockType();

            var contentTypeAttribute = dotnetBlockType.GetCustomAttribute<ContentTypeAttribute>();
            var strapiComponentTypeCategory = contentTypeAttribute?.Category ?? "Uncategorized";

            return strapiComponentTypeCategory;
        }

        public static IEnumerable<Type> GetAllowedBlockTypes(this PropertyInfo dotnetProperty)
        {
            if (dotnetProperty.PropertyType != typeof(ContentArea))
            {
                throw new Exception($"Cannot [{nameof(GetAllowedBlockTypes)}] for [{dotnetProperty.PropertyType}] data type");
            }

            dotnetProperty.DeclaringType.EnsureContentType();

            var allowedBlocksAttribute = dotnetProperty.GetCustomAttribute<AllowedBlocksAttribute>();
            if (allowedBlocksAttribute?.AllowedBlockTypes == null)
            {
                return Array.Empty<Type>();
            }

            var allowedBlockTypesIncludingInheritors = new HashSet<Type>(allowedBlocksAttribute.AllowedBlockTypes);
            foreach (var allowedBlockType in allowedBlocksAttribute.AllowedBlockTypes)
            foreach (var derivedBlockType in IAssemblyScanner.Instance.Types.Where(x => x.IsSubclassOf(allowedBlockType)))
            {
                allowedBlockTypesIncludingInheritors.Add(derivedBlockType);
            }

            return allowedBlockTypesIncludingInheritors
                .Where(x => !x.IsAbstract)
                .ToArray();
        }

        public static IEnumerable<ISelectItem> GetSelections(this Type selectionFactoryType)
        {
            if (!selectionFactoryType.IsAssignableTo<ISelectionFactory>())
            {
                throw new Exception(
                    $"Type [{selectionFactoryType.Name}] must implement [{nameof(ISelectionFactory)}] " +
                    "in order to be used as a Selection Factory."
                );
            }

            var selectionFactory = Activator.CreateInstance(selectionFactoryType) as ISelectionFactory;
            return selectionFactory?.GetSelections() ?? Array.Empty<ISelectItem>();
        }

        static void EnsureBlockType(this Type type)
        {
            if (!type.IsSubclassOf(typeof(BlockData)))
                throw new InvalidConstraintException($"Type {type} is not subclass of {typeof(BlockData)}");
        }

        static void EnsureContentType(this Type type)
        {
            if (!type.IsSubclassOf(typeof(PageData)) && !type.IsSubclassOf(typeof(BlockData)))
                throw new InvalidConstraintException($"Type {type} is not subclass of {typeof(PageData)} or {typeof(BlockData)}");
        }
    }
}