using System;
using System.Data;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Engine
{
    internal class StrapiComponentTypeMetadata
    {
        public Type CorrespondingDotnetBlockType { get; }

        public string Uid { get; }

        public StrapiComponentTypeMetadata(string strapiComponentTypeUid, string strapiComponentTypeApiId,
            Type correspondingDotnetBlockType)
        {
            Uid = strapiComponentTypeUid;
            CorrespondingDotnetBlockType = correspondingDotnetBlockType;

            var correspondingDotnetBlockTypeGuid = correspondingDotnetBlockType?.GetGuid();
            if (correspondingDotnetBlockTypeGuid != null && correspondingDotnetBlockTypeGuid != strapiComponentTypeApiId)
                throw new InvalidConstraintException(
                    $"ApiId stored by Strapi [{strapiComponentTypeApiId}] " +
                    "is different from " +
                    $"GUID set in .Net [{correspondingDotnetBlockTypeGuid}]"
                );
        }
    }
}