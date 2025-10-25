using System;
using System.Threading.Tasks;

namespace ScoutVision.API.Services
{
    public class BillingService
    {
        public async Task<Invoice> GenerateInvoiceAsync(string orgId, int userCount, bool annual)
        {
            var pricePerSeat = PricingService.GetPerSeatPrice(userCount);
            var total = annual ? PricingService.GetAnnualDiscount(pricePerSeat) * userCount : pricePerSeat * userCount;
            var invoice = new Invoice
            {
                OrganizationId = orgId,
                UserCount = userCount,
                IsAnnual = annual,
                AmountDue = total,
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30)
            };
            // Simulate async billing logic (e.g., call payment gateway)
            await Task.Delay(100);
            return invoice;
        }
    }

    public class Invoice
    {
        public string OrganizationId { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public bool IsAnnual { get; set; }
        public decimal AmountDue { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
