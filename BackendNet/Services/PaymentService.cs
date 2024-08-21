using Amazon.Runtime.Internal.Util;
using BackendNet.Dtos.Payment;
using BackendNet.Models;
using Stripe.Checkout;
using Stripe;

namespace BackendNet.Services
{
    public interface IPaymenService
    {
        Task<string> CreatePaymentInfo(Course course, string buyerId);
    }
    public class PaymentService : IPaymenService
    {
        private IConfiguration configuration;
        private IHttpContextAccessor httpContextAccessor;
        public PaymentService(IConfiguration configuration
            ,IHttpContextAccessor httpContextAccessor)
        {
            this.configuration = configuration;
        }

        public async Task<string> CreatePaymentInfo(Course course, string buyerId)
        {
            StripeConfiguration.ApiKey = configuration.GetSection("Stripe").GetValue<string>("Secretkey");

            var url = "https://localhost:44363/RoleManage";
            decimal price = course.Price;
            if(course.Discount > 0)
            {
                price *= course.Discount;
            }
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmountDecimal = price,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Buy course {course.Title}"
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = url + "/ConfirmCheckout/?userId=" + buyerId,
                CancelUrl = url + "/Index"
            };
            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return session.Url;
        }
    }
}
