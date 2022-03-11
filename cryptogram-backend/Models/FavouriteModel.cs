using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cryptogram_backend.Models
{
    public class FavouriteModel
    {
        [Required]
        public int fav_id { get; set; }
        [Required]
        public int img_id { get; set; }
        [Required]
        public int user_id { get; set; }
    }
}
