using System.Collections.Generic;

namespace Strapi.AspNet.DataModel
{
    public interface ISelectionFactory
    {
        IEnumerable<ISelectItem> GetSelections();
    }
}