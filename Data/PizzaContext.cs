using CrudeApi.Models.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace CrudeApi.Data
{
    public class PizzaContext : DbContext
    {
        public PizzaContext(DbContextOptions<PizzaContext> options)
            : base(options)
        {

        }
        public DbSet<Pizza> Product { get; set; }
    }
}
