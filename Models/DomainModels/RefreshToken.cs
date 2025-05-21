using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrudeApi.Models.DomainModels
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        public string Token { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; }

        public bool IsRevoked { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public UsersModel User { get; set; }
    }
}
