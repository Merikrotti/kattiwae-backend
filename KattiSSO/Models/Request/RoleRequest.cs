using System.ComponentModel.DataAnnotations;

namespace KattiSSO.Models.Request
{
    public class RoleRequest
    {
        [Required]
        public string SecretToken { get; set; }
    }
}
