using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PetShopEcommerce.Models;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PetShopEcommerce.Extensions;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PetShopEcommerce.Data;
using System.Diagnostics;
using Azure.Core;


namespace PetShopEcommerce.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private readonly string iyzicoPaymentBaseUrl = "https://sandbox-api.iyzipay.com/";
        private readonly string iyzicoApiKey = "sandbox-bNz0cUEE9j39vHnsUPcnwF6S8bHcm4Y7";
        private readonly string iyzicoSecretKey = "ESIIACicEITvWi1gai8cyrrbzaUsmoSt";
        private List<Product> _cartItems;
        public static string conversation_id = "";
        public static string token = "";



        public PaymentController(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _cartItems = _httpContextAccessor.HttpContext.Session.GetObject<List<Product>>("CartItems") ?? new List<Product>();
            _dbContext = dbContext;
        }

        public IActionResult Index(string token, decimal totalPrice)
        {
            ViewBag.TotalPrice = totalPrice;

            if (token != null)
            {
                Options options = new Options();
                options.ApiKey = iyzicoApiKey;
                options.SecretKey = iyzicoSecretKey;
                options.BaseUrl = iyzicoPaymentBaseUrl;

                RetrieveCheckoutFormRequest request = new RetrieveCheckoutFormRequest();
                request.Locale = "tr";
                request.ConversationId = "123456789";
                request.Token = token;

                CheckoutForm checkoutForm = CheckoutForm.Retrieve(request, options);
                if (checkoutForm.PaymentStatus == "SUCCESS")
                {
                    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    CreateOrder(userId, totalPrice, _cartItems);
                    TempData["Payment_Status"] = "Ödeme işlemi başarıyla gerçekleşti.";
                }
                else
                {
                    TempData["Payment_Status"] = checkoutForm.ErrorMessage;
                }

            }

            return View();
        }

        [HttpPost]
        public IActionResult GetPay(string returnUrl, string shippingContactName, string shippingCity, string shippingCountry,
    string shippingDescription, string shippingZipCode,
    string billingContactName, string billingCity, string billingCountry,
    string billingDescription, string billingZipCode)
        {

            Options options = new Options();
            options.ApiKey = iyzicoApiKey;
            options.SecretKey = iyzicoSecretKey;
            options.BaseUrl = iyzicoPaymentBaseUrl;



            decimal totalPrice = _cartItems.Sum(product => product.Price * product.Quantity);
            decimal test = _cartItems.Sum(_ => _.Price * _.Quantity );
            decimal paidPrice = test + 1m;

            int intPrice = Convert.ToInt32(test);
            string stringPrice = intPrice.ToString();
            int intPaidPrice = Convert.ToInt32(paidPrice); 
            string stringPaidPrice = intPaidPrice.ToString();

            var user = _dbContext.Users.FirstOrDefault();


            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";
            request.Price = stringPrice;
            request.PaidPrice = stringPaidPrice;
            request.Currency = Currency.TRY.ToString();
            request.BasketId = "B67832";
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            request.CallbackUrl = "https://localhost:7177/";

            List<int> enabledInstallments = new List<int>();
            enabledInstallments.Add(2);
            enabledInstallments.Add(3);
            enabledInstallments.Add(6);
            enabledInstallments.Add(9);
            request.EnabledInstallments = enabledInstallments;

            Buyer buyer = new Buyer();
            buyer.Id = user.Id;
            buyer.Name = user.UserName;
            buyer.Surname = user.NormalizedUserName;
            buyer.GsmNumber = "+905350000000";
            buyer.Email = user.Email;
            buyer.IdentityNumber = "74300864791";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            buyer.Ip = "85.34.78.112";
            buyer.City = "Istanbul";
            buyer.Country = "Turkey";
            buyer.ZipCode = "34732";
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = shippingContactName;
            shippingAddress.City = shippingCity;
            shippingAddress.Country = shippingCountry;
            shippingAddress.Description = shippingDescription;
            shippingAddress.ZipCode = shippingZipCode;
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = billingContactName;
            billingAddress.City = billingCity;
            billingAddress.Country = billingCountry;
            billingAddress.Description = billingDescription;
            billingAddress.ZipCode = billingZipCode;
            request.BillingAddress = billingAddress;


            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem();
            firstBasketItem.Id = "BI101";
            firstBasketItem.Name = "Binocular";
            firstBasketItem.Category1 = "Collectibles";
            firstBasketItem.Category2 = "Accessories";
            firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            firstBasketItem.Price = stringPrice;
            basketItems.Add(firstBasketItem);


            request.BasketItems = basketItems;
            CheckoutFormInitialize checkoutFormInitialize = CheckoutFormInitialize.Create(request, options);
            conversation_id = checkoutFormInitialize.ConversationId;
            TempData["Checkout_Form"] = checkoutFormInitialize.CheckoutFormContent;

            return RedirectToAction("Index", new { token = checkoutFormInitialize.Token, totalPrice = totalPrice });
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
        private void CreateOrder(string userId, decimal totalAmount, List<Product> cartItems)
        {
            // Create a new Order instance
            Order order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "Completed"
            };

            // Add the order to the database
            _dbContext.Orders.Add(order);
            _dbContext.SaveChanges();

            // Create OrderItem instances for each item in the cart
            foreach (Product cartItem in cartItems)
            {
                Models.OrderItem orderItem = new Models.OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.Id,
                    Quantity = cartItem.Quantity
                };

                // Add the order item to the database
                _dbContext.OrderItems.Add(orderItem);
            }

            _dbContext.SaveChanges();
        }
    }
}
