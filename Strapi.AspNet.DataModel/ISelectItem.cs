namespace Strapi.AspNet.DataModel
{
    public interface ISelectItem
    {
        /* TODO: Strapi currently does not support setting Label.
           In the future, when they do we need to uncomment this property and provide implementation. */
        // public string Label { get; }

        public string Value { get; }
    }

    internal class SelectItem : ISelectItem
    {
        public string Value { get; set; }
    }
}