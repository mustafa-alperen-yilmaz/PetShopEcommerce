using Microsoft.AspNetCore.Mvc;

namespace PetShopEcommerce.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
