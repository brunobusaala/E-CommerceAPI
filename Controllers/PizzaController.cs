using AutoMapper;
using CrudeApi.Data;
using CrudeApi.DTO;
using CrudeApi.Models.DomainModels;
using CrudeApi.Models.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CrudeApi.Controllers
{

    //[Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("Api/[controller]")]
    public class PizzaController : ControllerBase
    {
        private readonly PizzaContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<UsersModel> _userManager;

        public PizzaController(PizzaContext context, IMapper mapper, UserManager<UsersModel> userManager)
        {
            _context=context;
            _mapper=mapper;
            _userManager=userManager;
        }

        [HttpGet]
        [Route("AllPizza")]
        public IActionResult AllPizza()
        {
            var pizzas = _context.Products.ToList();
            var pizzaDto = _mapper.Map<IEnumerable<PizzaDto>>(pizzas);
            return Ok(pizzaDto);
        }

        [HttpGet("GetAPizza/{Id}")]
        public async Task<IActionResult> GetAPizza(Guid Id)
        {
            var pizza = await _context.Products.FindAsync(Id);
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
            await _context.Products.AddAsync(pizza);
            await _context.SaveChangesAsync();
            return Ok($"{pizza.Name} was added successfully!");
        }

        [HttpPut("EditPizza/{id}")]
        public async Task<IActionResult> EditPizza(Guid id, EditPizza edit)
        {
            var pizza = await _context.Products.FindAsync(id);
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

                await _context.SaveChangesAsync();
                return Ok($"{pizza.Name} was edited succesfully!");
            }
            return Ok();

        }
        [Authorize]
        [HttpDelete("DeletePizza/{Id}")]
        public async Task<IActionResult> DeletePizza(Guid Id)
        {
            var pizza = await _context.Products.FindAsync(Id);
            if ( pizza==null )
            {
                return NotFound($"The pizza is not currently in the Database!");
            }
            else if ( pizza!=null )
            {
                _context.Products.Remove(pizza);
                await _context.SaveChangesAsync();
                return Ok($"{pizza.Name} was deleted succesfully!");

            }
            return Ok();
        }


    }
}
