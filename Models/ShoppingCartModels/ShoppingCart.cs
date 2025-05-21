using CrudeApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CrudeApi.Models.ShoppingCartModels
{
    public class ShoppingCart
    {
        public PizzaContext dbContext;

        public ShoppingCart(PizzaContext dbContext)
        {
            this.dbContext = dbContext;

        }

        public List<CartItem> GetCartItems()
        {
            return dbContext.CartItems
                .Include(c => c.Pizza)
                .ToList();
        }
    }
}



