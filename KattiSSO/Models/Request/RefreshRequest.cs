﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KattiSSO.Models.Request
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
