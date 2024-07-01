using System.ComponentModel.DataAnnotations;

namespace CrudeApi.Models.LoginModels
{
    public class Login
    {
        [Required]
        public string? UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        //[EmailAddress]
        //public string? Email { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
