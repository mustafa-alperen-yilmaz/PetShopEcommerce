using Microsoft.AspNetCore.Mvc;
using PetShopEcommerce.Data;
using PetShopEcommerce.Models;

namespace PetShopEcommerce.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public SearchController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(string searchQuery)
        {
            if (searchQuery == null)
            {
                searchQuery = string.Empty;
            }

            List<Product> products = _dbContext.Products
                .Where(p => p.Name.Contains(searchQuery) || p.Description.Contains(searchQuery))
                .ToList();

            return View(products);
        }
    }
}
