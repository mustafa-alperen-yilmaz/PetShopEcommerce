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

        public IActionResult Index(string sortOrder)
        {
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParam"] = sortOrder == "price" ? "price_desc" : "price";

            IQueryable<Product> products = _dbContext.Products.AsQueryable();

            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            return View(products.ToList());
        }

        public IActionResult Details(int id)
        {
            Product product = _dbContext.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View("Details", product);
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