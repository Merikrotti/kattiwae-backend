using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Models
{
    public class AuthenticationConfiguration
    {
        public string AccessTokenSecret { get; set; }
        public int AccessTokenExpirationHours { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
