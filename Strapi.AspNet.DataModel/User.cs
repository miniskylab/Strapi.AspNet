using Newtonsoft.Json;

namespace Strapi.AspNet.DataModel
{
    public sealed class User
    {
        [JsonProperty("id")]
        public int Id { get; private set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; private set; }

        [JsonProperty("email")]
        public string Email { get; private set; }

        [JsonProperty("firstname")]
        public string FirstName { get; private set; }

        [JsonProperty("lastname")]
        public string LastName { get; private set; }
    }
}