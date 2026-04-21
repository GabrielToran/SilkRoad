using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace Multiplatoform_Project.Services
{

    public  class StripeService
    {
        private const string StripeSecretKey = "add your own stripe secret key";

        private static StripeService? _instance;
        public static StripeService Instance => _instance ??= new StripeService();


        private StripeService()
        {
            StripeConfiguration.ApiKey = StripeSecretKey;
        }

        public async Task<PaymentIntent> ChargeCardAsync(
            double amount,
            string currency,
             string paymentMethodToken,
            string description)
        {

            var pm = await new PaymentMethodService().CreateAsync(
       new PaymentMethodCreateOptions
       {
           Type = "card",
           Card = new PaymentMethodCardOptions
           {
               Token = paymentMethodToken
           }
       });

            
            var intent = await new PaymentIntentService().CreateAsync(
                new PaymentIntentCreateOptions
                {
                    Amount = (long)Math.Round(amount * 100), 
                    Currency = currency,
                    PaymentMethod = pm.Id,
                    Confirm = true,
                    Description = description,
                    AutomaticPaymentMethods = new()
                    {
                        Enabled = true,
                        AllowRedirects = "never"
                    }
                });

            if (intent.Status != "succeeded")
                throw new Exception($"Payment not completed. Status: {intent.Status}");

            return intent;
        }

        
        public async Task<Refund> RefundAsync(string paymentIntentId)
        {
            return await new RefundService().CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId
            });
        }


    }
}
