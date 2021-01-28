using System;
using System.Data;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Engine
{
    internal class StrapiContentTypeMetadata
    {
        public string ApiId { get; }

        public Type CorrespondingDotnetPageType { get; }

        public string Uid { get; }

        public StrapiContentTypeMetadata(string strapiContentTypeUid, string strapiContentTypeApiId, Type correspondingDotnetPageType)
        {
            Uid = strapiContentTypeUid;
            ApiId = strapiContentTypeApiId;
            CorrespondingDotnetPageType = correspondingDotnetPageType;

            var correspondingDotnetPageTypeGuid = correspondingDotnetPageType?.GetGuid();
            if (correspondingDotnetPageTypeGuid != null && correspondingDotnetPageTypeGuid != strapiContentTypeApiId)
                throw new InvalidConstraintException(
                    $"Uid stored by Strapi [{strapiContentTypeApiId}] " +
                    "is different from " +
                    $"GUID set in .Net [{correspondingDotnetPageTypeGuid}]"
                );
        }
    }
}