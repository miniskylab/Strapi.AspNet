using System;
using System.Collections.Generic;
using System.Linq;

namespace Strapi.AspNet.DataModel
{
    public class EnumSelectionFactory<TEnum> : ISelectionFactory where TEnum : Enum
    {
        public IEnumerable<ISelectItem> GetSelections()
        {
            return Enum.GetNames(typeof(TEnum)).Select(x => new SelectItem { Value = x }).ToArray();
        }
    }
}