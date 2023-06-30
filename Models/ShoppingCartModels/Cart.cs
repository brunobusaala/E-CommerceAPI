using CrudeApi.Models.DomainModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrudeApi.Models.ShoppingCartModels
{
    public class Cart
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartId { get; set; }

        // Foreign key for the User
        public int UserId { get; set; }

        // Navigation property for the User
        public UsersModel? UsersModel { get; set; }

        // Collection navigation property for CartItems
        public List<CartItem> CartItems { get; set; }
    }
}
