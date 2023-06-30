using CrudeApi.Models.ShoppingCartModels;
using Microsoft.AspNetCore.Identity;

namespace CrudeApi.Models.DomainModels
{
    public class UsersModel : IdentityUser<int>
    {
        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public Cart? Cart { get; set; }


    };
}
