// Dominus.Application.Services.RazorpayService.cs
using Dominus.Application.Settings;     // <-- ensure this is the correct namespace for RazorpaySettings
using Dominus.Application.Settings.Dominus.Application.Settings;
using Microsoft.Extensions.Options;
using Razorpay.Api;

namespace Dominus.Application.Services
{
    public class RazorpayService
    {
        private readonly RazorpaySettings _settings;
        private readonly RazorpayClient _client;

        public RazorpayService(IOptions<RazorpaySettings> options)
        {
            _settings = options.Value;
            _client = new RazorpayClient(_settings.Key, _settings.Secret);
        }

        public Razorpay.Api.Order CreateOrder(decimal amount, string receipt)
        {
            var options = new Dictionary<string, object>
            {
                { "amount", (int)(amount * 100) },
                { "currency", "INR" },
                { "receipt", receipt },
                { "payment_capture", 1 }
            };

            return _client.Order.Create(options);
        }

        public bool Verify(string razorOrderId, string paymentId, string signature)
        {
            var attributes = new Dictionary<string, string>
            {
                { "razorpay_order_id", razorOrderId },
                { "razorpay_payment_id", paymentId },
                { "razorpay_signature", signature }
            };

            Utils.verifyPaymentSignature(attributes); // throws if invalid
            return true;
        }

        public string GetKey() => _settings.Key;
    }
}
