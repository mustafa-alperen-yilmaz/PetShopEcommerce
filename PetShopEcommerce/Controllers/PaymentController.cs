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
using System.Diagnostics;

namespace PetShopEcommerce.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;
        private readonly string iyzicoPaymentBaseUrl = "https://sandbox-api.iyzipay.com/";
        private readonly string iyzicoApiKey = "sandbox-bNz0cUEE9j39vHnsUPcnwF6S8bHcm4Y7";
        private readonly string iyzicoSecretKey = "sandbox-LIGrmv8wXRNhsz4diJm2dqPEHYOZOrlH";
        private List<Product> _cartItems;
        public static string conversation_id = "";
        public static string token = "";

        public PaymentController(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _cartItems = _httpContextAccessor.HttpContext.Session.GetObject<List<Product>>("CartItems") ?? new List<Product>();
            _dbContext = dbContext;
        }

        public IActionResult Index(string? token, decimal totalPrice)
        {
            ViewBag.TotalPrice = totalPrice;

            if (token != null)
            {
                Options options = new Options();
                options.ApiKey = iyzicoApiKey;
                options.SecretKey = iyzicoSecretKey;
                options.BaseUrl = iyzicoPaymentBaseUrl;

                RetrieveCheckoutFormRequest request2 = new RetrieveCheckoutFormRequest();
                request2.Locale = "tr";
                request2.Token = token;

                CheckoutForm checkoutForm = CheckoutForm.Retrieve(request2, options);
                if (checkoutForm.PaymentStatus == "SUCCESS")
                {
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
        public IActionResult GetPay(string returnUrl)
        {

            Options options = new Options();
            options.ApiKey = iyzicoApiKey;
            options.SecretKey = iyzicoSecretKey;
            options.BaseUrl = iyzicoPaymentBaseUrl;

            decimal totalPrice = _cartItems.Sum(product => product.Price * product.Quantity);

            decimal paidPrice = totalPrice + 1m; // Use decimal literal "1m" to indicate a decimal value

            CreateCheckoutFormInitializeRequest request = new CreateCheckoutFormInitializeRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";
            request.Price = "10";
            request.PaidPrice = "20";
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
            buyer.Id = "BY789";
            buyer.Name = "John2";
            buyer.Surname = "Doe2";
            buyer.GsmNumber = "+905350000000";
            buyer.Email = "email@email.com";
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
            shippingAddress.ContactName = "Jane Doe";
            shippingAddress.City = "Istanbul";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "Jane Doe";
            billingAddress.City = "Istanbul";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;


            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem();
            firstBasketItem.Id = "BI101";
            firstBasketItem.Name = "Binocular";
            firstBasketItem.Category1 = "Collectibles";
            firstBasketItem.Category2 = "Accessories";
            firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            firstBasketItem.Price = "10";
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
    }
}
