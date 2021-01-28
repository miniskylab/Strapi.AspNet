using Newtonsoft.Json;

namespace Strapi.AspNet.Engine
{
    internal class StrapiDefaultAdminRegistrationDto
    {
        [JsonProperty("email")]
        public string Email { get; private set; }

        [JsonProperty("firstname")]
        public string FirstName { get; private set; }

        [JsonProperty("lastname")]
        public string LastName { get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }

        public StrapiDefaultAdminRegistrationDto()
        {
            Email = "admin@orbital.com";
            FirstName = "Orbital";
            LastName = "Admin";
            Password = "Orbital@Admin1";
        }
    }
}