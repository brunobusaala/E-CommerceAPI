using CrudeApi.Models.DomainModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrudeApi.Models.ShoppingCartModels
{
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }

        public int CartId { get; set; }

        public int Quantity { get; set; }

        public DateTime DateCreated { get; set; }

        public Guid ProductId { get; set; }

        public decimal Cost { get; set; }

        public virtual Pizza? Pizza { get; set; }


        public UsersModel? UsersModel { get; set; }

        public Cart Cart { get; set; }
    }
}
