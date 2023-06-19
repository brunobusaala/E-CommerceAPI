using CrudeApi.Models.DomainModels;
using System.ComponentModel.DataAnnotations;

namespace CrudeApi.Models.ShoppingCartModels
{
    public class CartItem
    {
        [Key]
        public string ItemId { get; set; }

        //The id for the user that is making a purchase
        //We will add a code to create this userId when the user accesses the shopping cart
        //The Id will be stored as a ASP.Net Session variable
        public string CartId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid ProductId { get; set; }
        public virtual Pizza Pizza { get; set; }
    }
}
