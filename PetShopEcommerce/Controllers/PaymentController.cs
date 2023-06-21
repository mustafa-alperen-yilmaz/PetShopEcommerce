using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PetShopEcommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using PetShopEcommerce.Extensions;

namespace PetShopEcommerce.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string iyzicoPaymentBaseUrl = "https://sandbox-api.iyzipay.com/";
        private readonly string iyzicoApiKey = "sandbox-bNz0cUEE9j39vHnsUPcnwF6S8bHcm4Y7";
        private readonly string iyzicoSecretKey = "sandbox-LIGrmv8wXRNhsz4diJm2dqPEHYOZOrlP";

        public PaymentController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            List<Product> cartItems = _httpContextAccessor.HttpContext.Session.GetObject<List<Product>>("CartItems") ?? new List<Product>();
            decimal totalPayment = CalculateTotalPayment(cartItems);
            ViewBag.TotalPayment = totalPayment;
            return View();
        }

        [HttpPost]
        public IActionResult ProcessPayment(string buyerName, string buyerSurname, string buyerEmail, string buyerId, decimal paymentAmount, string currency)
        {
            List<Product> cartItems = _httpContextAccessor.HttpContext.Session.GetObject<List<Product>>("CartItems") ?? new List<Product>();

            if (cartItems == null || cartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "There are no items in the cart.";
                return RedirectToAction("Index", "Cart");
            }

            // Create payment request
            CreatePaymentRequest request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = "123456789",
                Price = paymentAmount.ToString(),
                PaidPrice = paymentAmount.ToString(),
                Currency = currency,
                Installment = 1,
                BasketId = Guid.NewGuid().ToString(),
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString()
            };

            // Set buyer information
            Buyer buyer = new Buyer
            {
                Id = buyerId,
                Name = buyerName,
                Surname = buyerSurname,
                Email = buyerEmail,
                IdentityNumber = buyerId,
                RegistrationAddress = "Istanbul",
                City = "Istanbul",
                Country = "Turkey"
            };
            request.Buyer = buyer;

            // Set shipping address
            Address shippingAddress = new Address
            {
                ContactName = buyerName + " " + buyerSurname,
                City = "Istanbul",
                Country = "Turkey",
                Description = "Home",
                ZipCode = "34742"
            };
            request.ShippingAddress = shippingAddress;

            // Set billing address
            Address billingAddress = new Address
            {
                ContactName = buyerName + " " + buyerSurname,
                City = "Istanbul",
                Country = "Turkey",
                Description = "Invoice",
                ZipCode = "34742"
            };
            request.BillingAddress = billingAddress;

            // Set basket items
            List<BasketItem> basketItems = new List<BasketItem>();
            foreach (var product in cartItems)
            {
                BasketItem item = new BasketItem
                {
                    Id = product.Id.ToString(),
                    Name = product.Name,
                    Category1 = "Pets",
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = product.Price.ToString()
                };
                basketItems.Add(item);
            }
            request.BasketItems = basketItems;

            // Make payment request
            Options options = new Options
            {
                BaseUrl = iyzicoPaymentBaseUrl,
                ApiKey = iyzicoApiKey,
                SecretKey = iyzicoSecretKey
            };
            Payment payment = Payment.Create(request, options);

            if (payment.Status == "success")
            {
                // Payment is successful
                TempData["SuccessMessage"] = "Payment is successful!";
                // Clear the cart
                cartItems.Clear();
                _httpContextAccessor.HttpContext.Session.SetObject("CartItems", cartItems);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Payment is not successful
                TempData["ErrorMessage"] = "Payment failed. Please try again.";
                return RedirectToAction("Index", "Payment");
            }
        }

        private decimal CalculateTotalPayment(List<Product> cartItems)
        {
            decimal totalPayment = 0;
            foreach (var product in cartItems)
            {
                totalPayment += product.Price * product.Quantity;
            }
            return totalPayment;
        }
    }
}
