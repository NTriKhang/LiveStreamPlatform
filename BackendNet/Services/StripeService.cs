using Amazon.Runtime.Internal.Util;
using BackendNet.Models;
using Stripe.Checkout;
using Stripe;

namespace BackendNet.Services
{
    public interface IStripeService
    {
        Task<string> CreatePaymentInfo(Course course, Users users);
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
            this.httpContextAccessor = httpContextAccessor;
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
        public async Task<string> CreatePaymentInfo(Course course, Users users)
        {

            var userAgent = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? string.Empty;
            string returnUrl = "https://api.hightfive.click/";
            //if (userAgent != string.Empty)
            //{
            //        returnUrl = "http://localhost:8000/";
            //        returnUrl = "http://10.0.2.2/";


            //}


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
                CustomerEmail = users.Email,
                Mode = "payment",
                SuccessUrl = returnUrl + "api/Course/ConfirmCheckout/?userId=" + users.Id + "&courseId=" + course._id,
                CancelUrl = "http://localhost:4200/" + "CancelCheckout"
            };
            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return session.Url;
        }
    }
}