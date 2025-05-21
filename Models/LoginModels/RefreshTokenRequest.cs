using System.ComponentModel.DataAnnotations;

namespace CrudeApi.Models.LoginModels
{
    public class RefreshTokenRequest
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
