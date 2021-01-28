using System.ComponentModel.DataAnnotations;
using Strapi.AspNet.Annotations;
using Strapi.AspNet.DataModel;

namespace Strapi.AspNet.Cms
{
    [ContentType("ad0d8894-dbb4-4616-a136-7ce578dcce86", "Content Page")]
    public class ContentPage : PageData
    {
        [Display(Name = "Main Contents", Order = 10)]
        [AllowedBlocks(typeof(MainBlock))]
        public ContentArea MainContents { get; private set; }
    }
}