using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PetShopEcommerce.Models;
using System.Collections.Generic;
using PetShopEcommerce.Extensions;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PetShopEcommerce.Data;

namespace PetShopEcommerce.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private readonly string iyzicoPaymentBaseUrl = "https://sandbox-api.iyzipay.com/";
        private readonly string iyzicoApiKey = "sandbox-bNz0cUEE9j39vHnsUPcnwF6S8bHcm4Y7";
        private readonly string iyzicoSecretKey = "sandbox-LIGrmv8wXRNhsz4diJm2dqPEHYOZOrlP";
        private List<Product> _cartItems;

        public PaymentController(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _cartItems = _httpContextAccessor.HttpContext.Session.GetObject<List<Product>>("CartItems") ?? new List<Product>();
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            decimal totalPayment = CalculateTotalPayment();
            ViewBag.TotalPayment = totalPayment;

            return View();
        }

        [HttpPost]
        public IActionResult ProcessPayment(string buyerName, string buyerSurname, string buyerEmail, string buyerId, decimal paymentAmount, string currency)
        {
            Options options = new Options
            {
                BaseUrl = iyzicoPaymentBaseUrl,
                ApiKey = iyzicoApiKey,
                SecretKey = iyzicoSecretKey
            };

            if (_cartItems == null || _cartItems.Count == 0)
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
                BasketId = "B67832",
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
            foreach (var product in _cartItems)
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
            Payment payment = Payment.Create(request, options);

            if (payment.Status == "success")
            {
                // Payment is successful
                TempData["SuccessMessage"] = "Payment is successful!";
                // Clear the cart
                basketItems.Clear();
                _httpContextAccessor.HttpContext.Session.SetObject("CartItems", basketItems);

                // Construct the checkout form HTML manually
                string checkoutFormHtml = "<div id=\"iyzipay-checkout-form\" class=\"responsive\">" +
                                         "</div>";

                // Pass the checkout form content to the view
                TempData["CheckoutFormContent"] = checkoutFormHtml;

                // Save the order and order items to the database
                var userId = "UserId"; // Replace with the actual user ID
                var orderDate = DateTime.Now;
                var totalAmount = payment.PaidPrice; // Assuming paid price is the total amount
                var convertDecimal = Convert.ToDecimal(totalAmount);

                // Create a new order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = orderDate,
                    TotalAmount = convertDecimal,
                    Status = "Success" // Set the initial status as success, you can update it later if needed
                };

                // Add the order to the database
                // You need to have an instance of your DbContext here, let's assume it's called "dbContext"
                _dbContext.Orders.Add(order);
                _dbContext.SaveChanges();

                // Create order items and associate them with the order
                foreach (var product in basketItems)
                {
                    var orderItem = new Models.OrderItem
                    {
                        OrderId = order.Id,
                        ProductId =short.Parse(product.Id),
                        Quantity = 1
                    };

                    _dbContext.OrderItems.Add(orderItem);
                }

                _dbContext.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Payment is not successful
                TempData["ErrorMessage"] = "Payment failed. Please try again.";
                return RedirectToAction("Index", "Payment");
            }
        }

        private decimal CalculateTotalPayment()
        {
            decimal totalPayment = 0;
            foreach (var product in _cartItems)
            {
                totalPayment += product.Price * product.Quantity;
            }
            return totalPayment;
        }
    }
}
