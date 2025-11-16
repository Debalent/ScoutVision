using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ScoutVision.API.Services
{
    public interface IStripeService
    {
        Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency, string? customerId = null);
        Task<Customer> CreateCustomerAsync(string email, string name, Dictionary<string, string>? metadata = null);
        Task<Customer?> GetCustomerAsync(string customerId);
        Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId, Dictionary<string, string>? metadata = null);
        Task<Subscription?> GetSubscriptionAsync(string subscriptionId);
        Task<Subscription> UpdateSubscriptionAsync(string subscriptionId, string? newPriceId = null, Dictionary<string, string>? metadata = null);
        Task<Subscription> CancelSubscriptionAsync(string subscriptionId, bool immediately = false);
        Task<Stripe.Invoice> CreateInvoiceAsync(string customerId, Dictionary<string, string>? metadata = null);
        Task<StripeList<Stripe.Invoice>> GetInvoicesAsync(string customerId, int limit = 10);
        Task<Session> CreateCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl, string mode = "subscription");
        Task<Stripe.Coupon> CreateCouponAsync(string couponId, decimal percentOff, int? durationInMonths = null);
        Task<PromotionCode> CreatePromotionCodeAsync(string couponId, string code);
        Task<bool> ValidateWebhookSignatureAsync(string payload, string signature);
    }

    public class StripeService : IStripeService
    {
        private readonly string _secretKey;
        private readonly string _webhookSecret;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IConfiguration config, ILogger<StripeService> logger)
        {
            _secretKey = config["Stripe:SecretKey"] ?? throw new ArgumentNullException("Stripe:SecretKey");
            _webhookSecret = config["Stripe:WebhookSecret"] ?? "";
            _logger = logger;
            StripeConfiguration.ApiKey = _secretKey;
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(long amount, string currency, string? customerId = null)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" },
                Customer = customerId,
                SetupFutureUsage = "off_session"
            };

            var service = new PaymentIntentService();
            return await service.CreateAsync(options);
        }

        public async Task<Customer> CreateCustomerAsync(string email, string name, Dictionary<string, string>? metadata = null)
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var service = new CustomerService();
            return await service.CreateAsync(options);
        }

        public async Task<Customer?> GetCustomerAsync(string customerId)
        {
            try
            {
                var service = new CustomerService();
                return await service.GetAsync(customerId);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to get customer {CustomerId}", customerId);
                return null;
            }
        }

        public async Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId, Dictionary<string, string>? metadata = null)
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions { Price = priceId }
                },
                PaymentBehavior = "default_incomplete",
                PaymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    SaveDefaultPaymentMethod = "on_subscription"
                },
                Expand = new List<string> { "latest_invoice.payment_intent" },
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var service = new SubscriptionService();
            return await service.CreateAsync(options);
        }

        public async Task<Subscription?> GetSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var service = new SubscriptionService();
                return await service.GetAsync(subscriptionId);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to get subscription {SubscriptionId}", subscriptionId);
                return null;
            }
        }

        public async Task<Subscription> UpdateSubscriptionAsync(string subscriptionId, string? newPriceId = null, Dictionary<string, string>? metadata = null)
        {
            var service = new SubscriptionService();
            var subscription = await service.GetAsync(subscriptionId);

            var options = new SubscriptionUpdateOptions
            {
                Metadata = metadata ?? subscription.Metadata,
                ProrationBehavior = "create_prorations"
            };

            if (!string.IsNullOrEmpty(newPriceId))
            {
                options.Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Id = subscription.Items.Data[0].Id,
                        Price = newPriceId
                    }
                };
            }

            return await service.UpdateAsync(subscriptionId, options);
        }

        public async Task<Subscription> CancelSubscriptionAsync(string subscriptionId, bool immediately = false)
        {
            var service = new SubscriptionService();
            
            if (immediately)
            {
                return await service.CancelAsync(subscriptionId);
            }
            else
            {
                var options = new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = true
                };
                return await service.UpdateAsync(subscriptionId, options);
            }
        }

        public async Task<Stripe.Invoice> CreateInvoiceAsync(string customerId, Dictionary<string, string>? metadata = null)
        {
            var options = new InvoiceCreateOptions
            {
                Customer = customerId,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            var service = new InvoiceService();
            var invoice = await service.CreateAsync(options);
            return await service.FinalizeInvoiceAsync(invoice.Id);
        }

        public async Task<StripeList<Stripe.Invoice>> GetInvoicesAsync(string customerId, int limit = 10)
        {
            var options = new InvoiceListOptions
            {
                Customer = customerId,
                Limit = limit,
                Expand = new List<string> { "data.payment_intent" }
            };

            var service = new InvoiceService();
            return await service.ListAsync(options);
        }

        public async Task<Session> CreateCheckoutSessionAsync(string customerId, string priceId, string successUrl, string cancelUrl, string mode = "subscription")
        {
            var options = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                },
                Mode = mode,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                AllowPromotionCodes = true
            };

            var service = new SessionService();
            return await service.CreateAsync(options);
        }

        public async Task<Stripe.Coupon> CreateCouponAsync(string couponId, decimal percentOff, int? durationInMonths = null)
        {
            var options = new CouponCreateOptions
            {
                Id = couponId,
                PercentOff = percentOff,
                Duration = durationInMonths.HasValue ? "repeating" : "forever",
                DurationInMonths = durationInMonths
            };

            var service = new Stripe.CouponService();
            return await service.CreateAsync(options);
        }

        public async Task<PromotionCode> CreatePromotionCodeAsync(string couponId, string code)
        {
            var options = new PromotionCodeCreateOptions
            {
                Code = code,
                Active = true
            };
            options.AddExtraParam("coupon", couponId);

            var service = new PromotionCodeService();
            return await service.CreateAsync(options);
        }

        public async Task<bool> ValidateWebhookSignatureAsync(string payload, string signature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(payload, signature, _webhookSecret);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Invalid webhook signature");
                return false;
            }
        }
    }
}
