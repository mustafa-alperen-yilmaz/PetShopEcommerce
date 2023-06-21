using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PetShopEcommerce.Models;
using System.Collections.Generic;
using PetShopEcommerce.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PetShopEcommerce.Data;
using Microsoft.EntityFrameworkCore;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;

namespace PetShopEcommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        
        private readonly string iyzicoPaymentBaseUrl = "https://sandbox-api.iyzipay.com/";
        private readonly string iyzicoApiKey = "sandbox-bNz0cUEE9j39vHnsUPcnwF6S8bHcm4Y7";
        private readonly string iyzicoSecurityKey = "sandbox-LIGrmv8wXRNhsz4diJm2dqPEHYOZOrlP";
        
        
        private List<Product> _cartItems;

        public CartController(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;

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
        [HttpPost]
        public IActionResult UpdateCartItem(int productId, int quantity)
        {
            var cartItems = _httpContextAccessor.HttpContext.Session.GetObject<List<Product>>("CartItems");
            var cartItem = cartItems.FirstOrDefault(item => item.Id == productId);

            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    cartItems.Remove(cartItem);
                }
            }

            _httpContextAccessor.HttpContext.Session.SetObject("CartItems", cartItems);

            return RedirectToAction("Index");
        }
    }
}