using CrudeApi.Models.DomainModels;
using CrudeApi.Models.ShoppingCartModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrudeApi.Data
{
    public class PizzaContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
    {
        public PizzaContext(DbContextOptions<PizzaContext> options)
            : base(options)
        {
        }

        public DbSet<Pizza> Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Cart> Carts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationships between the entities
            modelBuilder.Entity<UsersModel>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.UsersModel)
                .HasForeignKey<Cart>(c => c.UserId);

            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(c => c.Cart)
                .HasForeignKey(ci => ci.CartId);
        }
    }
}
