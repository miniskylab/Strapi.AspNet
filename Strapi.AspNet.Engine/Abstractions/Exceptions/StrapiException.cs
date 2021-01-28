using System;

namespace Strapi.AspNet.Engine
{
    internal class StrapiException : Exception
    {
        public StrapiException(string message) : base(message) { }
    }
}