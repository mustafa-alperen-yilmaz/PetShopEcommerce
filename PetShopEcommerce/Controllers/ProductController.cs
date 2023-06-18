using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using PetShopEcommerce.Models;
using PetShopEcommerce.Data;

namespace PetShopEcommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            List<Product> products = _dbContext.Products.ToList();

            return View(products);
        }

        public IActionResult Details(int id)
        {
            Product product = _dbContext.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        public IActionResult Search(string searchQuery)
        {
            List<Product> products;

            if (string.IsNullOrEmpty(searchQuery))
            {
                products = new List<Product>(); // Create an empty list when searchQuery is empty
            }
            else
            {
                products = _dbContext.Products
                    .Where(p => p.Name.Contains(searchQuery) || p.Description.Contains(searchQuery))
                    .ToList();
            }

            if (string.IsNullOrEmpty(searchQuery) || products.Count == 0)
            {
                products = new List<Product>();
            }

            return View(products);
        }
    }


}