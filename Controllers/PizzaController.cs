using CrudeApi.Data;
using CrudeApi.Models.DomainModels;
using CrudeApi.Models.RequestModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudeApi.Controllers
{
    [EnableCors]
    [ApiController]
    [Route("Api/[controller]")]
    public class PizzaController : ControllerBase
    {
        private readonly PizzaContext context;

        public PizzaController(PizzaContext context)
        {
            this.context=context;
        }
        [HttpGet]
        public async Task<IActionResult> AllPizza()
        {
            return Ok(await context.Product.ToListAsync());
        }

        [HttpGet("GetAPizza{Id}")]
        public async Task<IActionResult> GetAPizza(Guid Id)
        {
            var pizza = await context.Product.FindAsync(Id);
            if ( pizza==null )
            {
                return NotFound("The pizza is not currently in Stock!");
            }
            else if ( pizza!=null )
            {
                return Ok($"{pizza.Name} was succesfully retrieved from the database");
            }
            return Ok();
        }

        [HttpPost]
        [Route("AddPizza")]
        public async Task<IActionResult> AddNewPizza(AddPizza addPizza)
        {
            var pizza = new Pizza()
            {
                Id=new Guid(),
                ModifiedDate=DateTime.Now,
                Description=addPizza.Description,
                SizeID=addPizza.SizeID,
                ImageName=addPizza.ImageName,
                Name=addPizza.Name,
                Price=addPizza.Price
            };
            await context.Product.AddAsync(pizza);
            await context.SaveChangesAsync();
            return Ok($"{pizza.Name} was added successfully!");
        }

        [HttpPut("EditPizza{id}")]
        public async Task<IActionResult> EditPizza(Guid id, EditPizza edit)
        {
            var pizza = await context.Product.FindAsync(id);
            if ( pizza==null )
            {
                return NotFound("The pizza you are trying to edit is currently not in the system!");
            }
            else if ( pizza!=null )
            {
                pizza.SizeID=edit.SizeID;
                pizza.Price=edit.Price;
                pizza.ModifiedDate=DateTime.Now;
                pizza.Description=edit.Description;
                pizza.Name=edit.Name;
                pizza.ImageName=edit.ImageName;

                await context.SaveChangesAsync();
                return Ok($"{pizza.Name} was edited succesfully!");
            }
            return Ok();

        }
        [HttpDelete("DeletePizza{Id}")]
        public async Task<IActionResult> DeletePizza(Guid Id)
        {
            var pizza = await context.Product.FindAsync(Id);
            if ( pizza==null )
            {
                return NotFound($"The pizza is not currently in the Database!");
            }
            else if ( pizza!=null )
            {
                context.Product.Remove(pizza);
                await context.SaveChangesAsync();
                return Ok($"{pizza.Name} was deleted succesfully!");
            }
            return Ok();
        }

    }
}
