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

        //Foreign Key
        public int CartId { get; set; }

        public int Quantity { get; set; }

        public DateTime DateCreated { get; set; }

        public Guid ProductId { get; set; }

        public decimal Cost { get; set; }//Added cost property

        public virtual Pizza? Pizza { get; set; }


        // Navigation property for the User
        public UsersModel? UsersModel { get; set; } // navigation property

        // Collection navigation property for CartItems
        public Cart Cart { get; set; }
    }
}
