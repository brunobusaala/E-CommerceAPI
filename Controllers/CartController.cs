using Microsoft.AspNetCore.Mvc;

namespace CrudeApi.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
