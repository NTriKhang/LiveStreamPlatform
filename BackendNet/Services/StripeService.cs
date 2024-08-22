using Amazon.Runtime.Internal.Util;
using BackendNet.Models;
using Stripe.Checkout;
using Stripe;

namespace BackendNet.Services
{
    public interface IStripeService
    {
        Task<string> CreatePaymentInfo(Course course, string buyerId);
        string CreateStripeAccount();
    }
    public class StripeService : IStripeService
    {
        private IConfiguration configuration;
        private IHttpContextAccessor httpContextAccessor;
        public StripeService(IConfiguration configuration
            ,IHttpContextAccessor httpContextAccessor)
        {
            this.configuration = configuration;
        }
        public string CreateStripeAccount()
        {
            StripeConfiguration.ApiKey = configuration.GetSection("Stripe").GetValue<string>("Secretkey");

            var options = new AccountCreateOptions
            {
                Controller = new AccountControllerOptions
                {
                    Losses = new AccountControllerLossesOptions { Payments = "application" },
                    Fees = new AccountControllerFeesOptions { Payer = "application" },
                    StripeDashboard = new AccountControllerStripeDashboardOptions
                    {
                        Type = "express",
                    },
                },
            };
            var service = new AccountService();
            service.Create(options);
            return service.BaseUrl;
        }
        public async Task<string> CreatePaymentInfo(Course course, string buyerId)
        {

            var userAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? string.Empty;
            string returnUrl = string.Empty;
            if (userAgent != string.Empty)
            {
                if (userAgent.Contains("Windows NT") || userAgent.Contains("Macintosh") || userAgent.Contains("Mac OS X"))
                    returnUrl = "http://localhost:8000/";
                else if (userAgent.Contains("Mobi") || userAgent.Contains("Android") || userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                    returnUrl = "http://10.0.2.2/";
            }

            

            StripeConfiguration.ApiKey = configuration.GetSection("Stripe").GetValue<string>("Secretkey");
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
                                Name = $"Buy course {course.Title}",
                                Description = $"Desc: {course.Desc}",
                                Images = new List<string>{ course.CourseImage }
                            },
                        },
                        Quantity = 1,
                    },
                },
                
                Mode = "payment",
                SuccessUrl = returnUrl + "/ConfirmCheckout/?userId=" + buyerId,
                CancelUrl = returnUrl + "/Index"
            };
            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return session.Url;
        }
    }
}
