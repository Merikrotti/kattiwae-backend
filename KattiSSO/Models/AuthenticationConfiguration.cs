using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KattiSSO.Models
{
    public class AuthenticationConfiguration
    {
        public string AccessTokenSecret { get; set; }
        public string RefreshTokenSecret { get; set; }
        public int AccessTokenExpirationHours { get; set; }
        public int RefreshTokenExpirationHours { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
