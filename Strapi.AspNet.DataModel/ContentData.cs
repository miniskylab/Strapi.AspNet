using JetBrains.Annotations;

namespace Strapi.AspNet.DataModel
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
    public abstract class ContentData
    {
        public virtual void SetDefaultValues()
        {
            /* Do nothing */
        }
    }
}