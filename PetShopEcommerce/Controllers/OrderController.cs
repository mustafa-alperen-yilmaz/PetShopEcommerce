using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Mvc;
using PetShopEcommerce.Models;

namespace PetShopEcommerce.Controllers
{
    public class OrderController : Controller
    {
        private readonly string iyzicoRefundBaseUrl = "https://sandbox-api.iyzipay.com/";
        private readonly string iyzicoApiKey = "sandbox-bNz0cUEE9j39vHnsUPcnwF6S8bHcm4Y7";
        private readonly string iyzicoSecretKey = "ESIIACicEITvWi1gai8cyrrbzaUsmoSt";
        public IActionResult Index()
        {
            Payment payment = GetPaymentFromAPI();

           

            return View(payment);
        }

        private Payment GetPaymentFromAPI()
        {
            Options options = new Options();
            options.ApiKey = iyzicoApiKey;
            options.SecretKey = iyzicoSecretKey;
            options.BaseUrl = iyzicoRefundBaseUrl;

            RetrievePaymentRequest request = new RetrievePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "123456789";
            request.PaymentId = "1";
            request.PaymentConversationId = "123456789";

            Payment payment = Payment.Retrieve(request, options);

            return payment;
        }
    }
}
