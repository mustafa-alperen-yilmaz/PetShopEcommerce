using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PetShopEcommerce.Models;
using System.Collections.Generic;
using PetShopEcommerce.Extensions;

namespace PetShopEcommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private List<Product> _cartItems;

        public CartController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            // Retrieve the cart items from session, or create a new list if it doesn't exist
            _cartItems = _httpContextAccessor.HttpContext.Session.GetObject<List<Product>>("CartItems") ?? new List<Product>();
        }

        public IActionResult Index()
        {
            return View(_cartItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, string productName, decimal price, int quantity)
        {
            var existingProduct = _cartItems.FirstOrDefault(p => p.Id == productId);

            if (existingProduct == null)
            {
                var product = new Product
                {
                    Id = productId,
                    Name = productName,
                    Price = price,
                    Quantity = quantity 
                };
                _cartItems.Add(product);
            }
            else
            {
                existingProduct.Quantity += quantity; 
            }

            // Store the updated cart items back into session
            _httpContextAccessor.HttpContext.Session.SetObject("CartItems", _cartItems);

            return RedirectToAction("Index", "Cart");
        }

    }
}