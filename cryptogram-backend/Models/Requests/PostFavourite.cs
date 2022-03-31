using System.ComponentModel.DataAnnotations;

namespace cryptogram_backend.Models.Requests
{
    public class PostFavourite
    {
        [Required]
        public int img_id { get; set; }
    }
}
