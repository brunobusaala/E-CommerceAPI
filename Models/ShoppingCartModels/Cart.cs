using CrudeApi.Models.DomainModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrudeApi.Models.ShoppingCartModels
{
    public class Cart
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartId { get; set; }

        public int UserId { get; set; }

        public UsersModel? UsersModel { get; set; }

        public List<CartItem> CartItems { get; set; }
    }
}
