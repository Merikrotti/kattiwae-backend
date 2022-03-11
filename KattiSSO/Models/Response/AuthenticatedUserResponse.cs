using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Models.Response
{
    public class AuthenticatedUserResponse
    {
        public string AccessToken { get; set; }
    }
}
