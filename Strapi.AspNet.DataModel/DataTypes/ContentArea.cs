using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Strapi.AspNet.DataModel
{
    [JsonConverter(typeof(ContentAreaJsonConverter))]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ContentArea
    {
        public BlockData[] Items { get; }

        public ContentArea(BlockData[] items) { Items = items; }
    }
}