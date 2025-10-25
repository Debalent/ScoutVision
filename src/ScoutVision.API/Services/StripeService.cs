using Stripe;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ScoutVision.API.Services
{
    public class StripeService
    {
        private readonly string _secretKey;
        public StripeService(IConfiguration config)
        {
            _secretKey = config["Stripe:SecretKey"] ?? "";
            StripeConfiguration.ApiKey = _secretKey;
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                PaymentMethodTypes = new[] { "card" },
            };
            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }

        // Add methods for subscriptions, customers, and webhook handling as needed
    }
}
