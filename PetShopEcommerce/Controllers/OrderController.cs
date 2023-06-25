using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.AspNetCore.Mvc;

public class OrderController : Controller
{
    private readonly string iyzicoPaymentBaseUrl = "https://sandbox-api.iyzipay.com/";
    private readonly string iyzicoApiKey = "sandbox-bNz0cUEE9j39vHnsUPcnwF6S8bHcm4Y7";
    private readonly string iyzicoSecretKey = "SSKsMTrv0C6bKnoVe3vthnk9u5StTAm7";

    public ActionResult Index()
    {
        RetrievePaymentRequest paymentRequest = new RetrievePaymentRequest();
        paymentRequest.Locale = Locale.TR.ToString();
        paymentRequest.ConversationId = "123456789";
        paymentRequest.PaymentId = "1";
        paymentRequest.PaymentConversationId = "123456789";

        Options options = new Options();
        options.ApiKey = iyzicoApiKey;
        options.SecretKey = iyzicoSecretKey;
        options.BaseUrl = iyzicoPaymentBaseUrl;

        Payment payment = Payment.Retrieve(paymentRequest, options);

        return View(payment);
    }

    public ActionResult CreateRefund()
    {
        CreateRefundRequest refundRequest = new CreateRefundRequest();
        refundRequest.ConversationId = "123456789";
        refundRequest.Locale = Locale.TR.ToString();
        refundRequest.PaymentTransactionId = "1";
        refundRequest.Price = "0.5";
        refundRequest.Ip = "85.34.78.112";
        refundRequest.Currency = Currency.TRY.ToString();

        Options options = new Options();
        options.ApiKey = iyzicoApiKey;
        options.SecretKey = iyzicoSecretKey;
        options.BaseUrl = iyzicoPaymentBaseUrl;

        Refund refund = Refund.Create(refundRequest, options);

        return View(refund);
    }
}
