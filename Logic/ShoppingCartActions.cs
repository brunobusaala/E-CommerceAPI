using CrudeApi.Data;
using CrudeApi.Models.ShoppingCartModels;

namespace CrudeApi.Logic
{
    public class ShoppingCartActions : IDisposable
    {
        private PizzaContext _db;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ShoppingCartActions(PizzaContext _db, IHttpContextAccessor httpContextAccessor)
        {
            this._db=_db;
            this.httpContextAccessor=httpContextAccessor;
        }
        public string ShoppingCartId { get; set; }


        public const string CartSessionKey = "CartId";
        public void AddToCart(Guid id)
        {
            //Retrieve the product from the database
            ShoppingCartId=GetCartId();

            var cartItem = _db.ShoppingCartItems.SingleOrDefault(c => c.CartId==ShoppingCartId&&c.ProductId==id);
            if ( cartItem==null )
            {
                //create a new cart item if no cart item exists.
                cartItem=new CartItem
                {
                    ItemId=Guid.NewGuid().ToString(),
                    ProductId=id,
                    CartId=ShoppingCartId,
                    Pizza=_db.Product.SingleOrDefault(p => p.Id==id),
                    Quantity=1,
                    DateCreated=DateTime.Now,
                };
                _db.ShoppingCartItems.Add(cartItem);
                _db.SaveChangesAsync();
            }
            else
            {
                //if the item does exist in the cart,
                //then add one to the quantity
                cartItem.Quantity++;
                _db.SaveChangesAsync();
            }
        }
        public void Dispose()
        {
            if ( _db!=null )
            {
                _db.Dispose();
                _db=null;
            }
        }
        public string GetCartId()
        {
            var httpContext = httpContextAccessor.HttpContext;
            if ( httpContext.Session.GetString(CartSessionKey)==null )
            {
                if ( !string.IsNullOrWhiteSpace(httpContext.User.Identity.Name) )
                {
                    httpContext.Session.SetString(CartSessionKey, httpContext.User.Identity.Name);
                }
                else
                {
                    //Generate a new random Guid using System.Guid class.
                    Guid tempCardId = Guid.NewGuid();
                    httpContext.Session.SetString(CartSessionKey, tempCardId.ToString());
                }
            }
            return httpContext.Session.GetString(CartSessionKey);
        }
        public List<CartItem> GetCartItems()
        {
            ShoppingCartId=GetCartId();
            return _db.ShoppingCartItems.Where(c => c.CartId==ShoppingCartId).ToList();
        }

    }
}
