using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ScoutVision.API.Services
{
    public interface ICouponService
    {
        Task<CouponValidationResult> ValidateCouponAsync(string couponCode);
        Task<Coupon> CreateCouponAsync(string code, CouponType type, decimal value, DateTime? expiryDate = null);
        Task<List<Coupon>> GetActiveCouponsAsync();
        Task<bool> DeactivateCouponAsync(string couponCode);
        Task<CouponUsage> UseCouponAsync(string couponCode, string customerId);
    }

    public class CouponService : ICouponService
    {
        private readonly ILogger<CouponService> _logger;
        private readonly IStripeService _stripeService;

        public CouponService(ILogger<CouponService> logger, IStripeService stripeService)
        {
            _logger = logger;
            _stripeService = stripeService;
        }

        public async Task<CouponValidationResult> ValidateCouponAsync(string couponCode)
        {
            try
            {
                // Check predefined coupons
                var predefinedCoupons = GetPredefinedCoupons();
                var predefinedCoupon = predefinedCoupons.FirstOrDefault(c => 
                    c.Code.Equals(couponCode, StringComparison.OrdinalIgnoreCase));

                if (predefinedCoupon != null)
                {
                    if (predefinedCoupon.ExpiryDate.HasValue && predefinedCoupon.ExpiryDate < DateTime.UtcNow)
                    {
                        return new CouponValidationResult
                        {
                            IsValid = false,
                            ErrorMessage = "Coupon has expired"
                        };
                    }

                    if (predefinedCoupon.UsageLimit.HasValue && predefinedCoupon.UsageCount >= predefinedCoupon.UsageLimit)
                    {
                        return new CouponValidationResult
                        {
                            IsValid = false,
                            ErrorMessage = "Coupon usage limit reached"
                        };
                    }

                    return new CouponValidationResult
                    {
                        IsValid = true,
                        Coupon = predefinedCoupon,
                        DiscountAmount = CalculateDiscount(predefinedCoupon, 299.99m) // Default price for validation
                    };
                }

                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid coupon code"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating coupon {CouponCode}", couponCode);
                return new CouponValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Error validating coupon"
                };
            }
        }

        public async Task<Coupon> CreateCouponAsync(string code, CouponType type, decimal value, DateTime? expiryDate = null)
        {
            var coupon = new Coupon
            {
                Code = code.ToUpper(),
                Type = type,
                Value = value,
                ExpiryDate = expiryDate,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UsageCount = 0
            };

            // Create in Stripe if it's a percentage discount
            if (type == CouponType.Percentage)
            {
                try
                {
                    await _stripeService.CreateCouponAsync(code, value);
                    await _stripeService.CreatePromotionCodeAsync(code, code);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create Stripe coupon for {CouponCode}", code);
                }
            }

            // TODO: Save to database
            await Task.Delay(10);

            return coupon;
        }

        public async Task<List<Coupon>> GetActiveCouponsAsync()
        {
            // TODO: Replace with database query
            await Task.Delay(10);
            return GetPredefinedCoupons().Where(c => c.IsActive).ToList();
        }

        public async Task<bool> DeactivateCouponAsync(string couponCode)
        {
            // TODO: Update in database
            await Task.Delay(10);
            return true;
        }

        public async Task<CouponUsage> UseCouponAsync(string couponCode, string customerId)
        {
            var validation = await ValidateCouponAsync(couponCode);
            if (!validation.IsValid)
            {
                return new CouponUsage
                {
                    Success = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            // TODO: Record usage in database
            await Task.Delay(10);

            return new CouponUsage
            {
                Success = true,
                CouponCode = couponCode,
                CustomerId = customerId,
                DiscountAmount = validation.DiscountAmount,
                UsedDate = DateTime.UtcNow
            };
        }

        private List<Coupon> GetPredefinedCoupons()
        {
            return new List<Coupon>
            {
                new Coupon
                {
                    Code = "SCOUT10",
                    Type = CouponType.Percentage,
                    Value = 10,
                    Description = "10% off first month",
                    IsActive = true,
                    ExpiryDate = DateTime.UtcNow.AddMonths(3)
                },
                new Coupon
                {
                    Code = "WELCOME20",
                    Type = CouponType.Percentage,
                    Value = 20,
                    Description = "20% off first month for new users",
                    IsActive = true,
                    ExpiryDate = DateTime.UtcNow.AddMonths(6)
                },
                new Coupon
                {
                    Code = "TRIAL50",
                    Type = CouponType.FixedAmount,
                    Value = 50,
                    Description = "$50 off first payment",
                    IsActive = true,
                    UsageLimit = 100
                }
            };
        }

        private decimal CalculateDiscount(Coupon coupon, decimal originalAmount)
        {
            return coupon.Type switch
            {
                CouponType.Percentage => originalAmount * (coupon.Value / 100),
                CouponType.FixedAmount => Math.Min(coupon.Value, originalAmount),
                _ => 0
            };
        }
    }

    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public Coupon? Coupon { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class Coupon
    {
        public string Code { get; set; } = "";
        public CouponType Type { get; set; }
        public decimal Value { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UsageCount { get; set; }
        public int? UsageLimit { get; set; }
    }

    public class CouponUsage
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string CouponCode { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public decimal DiscountAmount { get; set; }
        public DateTime UsedDate { get; set; }
    }

    public enum CouponType
    {
        Percentage,
        FixedAmount
    }
}