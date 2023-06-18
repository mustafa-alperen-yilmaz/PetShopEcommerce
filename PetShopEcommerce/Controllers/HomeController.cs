using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShopEcommerce.Data;
using PetShopEcommerce.Models;
using System.Diagnostics;

namespace PetShopEcommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger , ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        
        public IActionResult Index()
        {
            List<Product> products = _dbContext.Products.ToList();

            return View(products);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}