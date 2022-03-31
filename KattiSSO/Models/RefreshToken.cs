using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KattiSSO.Models
{
    public class RefreshToken
    {
        public int ref_id { get; set; }
        public int user_id { get; set; }
        public string Token { get; set; }

    }
}
