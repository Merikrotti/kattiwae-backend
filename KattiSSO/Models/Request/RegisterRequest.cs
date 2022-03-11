﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Models
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(5)]
        [RegularExpression(@"^[A-Za-z0-9]+$")]
        public string username { get; set; }
        [Required]
        [MinLength(7)]
        public string password { get; set; }
        [Required]
        [MinLength(7)]
        public string confirmpassword { get; set; }
    }
}
