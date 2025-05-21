using AutoMapper;
using CrudeApi.Data;
using CrudeApi.DTO;
using CrudeApi.Models.DomainModels;
using CrudeApi.Models.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;

namespace CrudeApi.Controllers
{

    [ApiController]
    [Route("Api/[controller]")]
    public class PizzaController : ControllerBase
    {
        private readonly PizzaContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<UsersModel> _userManager;
        private readonly ILogger<PizzaController> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;

        public PizzaController(
            PizzaContext context,
            IMapper mapper,
            UserManager<UsersModel> userManager,
            ILogger<PizzaController> logger,
            IDistributedCache distributedCache,
            IMemoryCache memoryCache)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
        }


        [HttpGet("Serilog")]
        public async Task<IActionResult> FetchNumbers()
        {
            _logger.LogInformation("Requested the Serilog Api");

            var number = 4;
            try
            {
                if (number == 4)
                {
                    throw new Exception("RandomException");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught");
            }


            return Ok();
        }

        [HttpGet("redis")]
        public async Task<IActionResult> GetAllPizzasUsingRedisCache()
        {
            var pizzas = _context.Products.ToList();

            var cacheKey = "PizzaList";

            string serializedPizzaList;

            var pizzaList = new List<Pizza>();

            var startTime = DateTime.Now;

            var redisCustomerList = await _distributedCache.GetAsync(cacheKey);

            if (redisCustomerList != null)
            {
                serializedPizzaList = Encoding.UTF8.GetString(redisCustomerList);
                pizzaList = JsonConvert.DeserializeObject<List<Pizza>>(serializedPizzaList);
                var endTime = DateTime.Now;
                _logger.Log(LogLevel.Warning, $"list with redis retrieved in {(endTime - startTime).TotalMilliseconds}");

            }
            else
            {
                pizzaList = _context.Products.ToList();
                serializedPizzaList = JsonConvert.SerializeObject(pizzaList);
                redisCustomerList = Encoding.UTF8.GetBytes(serializedPizzaList);
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTime.Now.AddSeconds(30))
                    .SetSlidingExpiration(TimeSpan.FromSeconds(10));
                await _distributedCache.SetAsync(cacheKey, redisCustomerList, options);
                var endTime = DateTime.Now;
                _logger.Log(LogLevel.Warning, $"list without redis retrieved in {(endTime - startTime).TotalMilliseconds}");
            }
            return Ok(serializedPizzaList);
        }

        [HttpGet("InMemory")]
        public async Task<IActionResult> GetAll()
        {
            var cacheKey = "customerList";
            if (!_memoryCache.TryGetValue(cacheKey, out List<Pizza> pizzaList))
            {
                pizzaList = _context.Products.ToList();
                var cacheExpiryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.Now.AddSeconds(30),
                    Priority = CacheItemPriority.High,
                    SlidingExpiration = TimeSpan.FromSeconds(10)

                };
                _memoryCache.Set(cacheKey, pizzaList, cacheExpiryOptions);
            }
            return Ok(pizzaList);
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
            if (pizza == null)
            {
                return NotFound("The pizza is not currently in Stock!");
            }
            else if (pizza != null)
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
                Id = new Guid(),
                ModifiedDate = DateTime.Now,
                Description = addPizza.Description,
                SizeID = addPizza.SizeID,
                ImageName = addPizza.ImageName,
                Name = addPizza.Name,
                Price = addPizza.Price
            };
            await _context.Products.AddAsync(pizza);
            await _context.SaveChangesAsync();
            return Ok($"{pizza.Name} was added successfully!");
        }

        [HttpPut("EditPizza/{id}")]
        public async Task<IActionResult> EditPizza(Guid id, EditPizza edit)
        {
            var pizza = await _context.Products.FindAsync(id);
            if (pizza == null)
            {
                return NotFound("The pizza you are trying to edit is currently not in the system!");
            }
            else if (pizza != null)
            {
                pizza.SizeID = edit.SizeID;
                pizza.Price = edit.Price;
                pizza.ModifiedDate = DateTime.Now;
                pizza.Description = edit.Description;
                pizza.Name = edit.Name;
                pizza.ImageName = edit.ImageName;

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
            if (pizza == null)
            {
                return NotFound($"The pizza is not currently in the Database!");
            }
            else if (pizza != null)
            {
                _context.Products.Remove(pizza);
                await _context.SaveChangesAsync();
                return Ok($"{pizza.Name} was deleted succesfully!");

            }
            return Ok();
        }
    }
}
