using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Issuing;
using WebApi.DBContext;
using WebApi.Helpers;
using WebApi.Models;
using CardCreateOptions = Stripe.Issuing.CardCreateOptions;
using CardService = Stripe.Issuing.CardService;
using CardUpdateOptions = Stripe.Issuing.CardUpdateOptions;

namespace WebApi.Services
{
    public interface IStripeService
    {
        virtualcardresponse CreateVirtualCard(createcardrequest model);
    }

    public class StripeService : IStripeService
    {
        private readonly AppSettings _appSettings;
        private readonly db_Context _context;

        public StripeService(IOptions<AppSettings> appSettings,
            db_Context context)
        {
            _appSettings = appSettings.Value;
            _context = context;
        }

        public virtualcardresponse CreateVirtualCard(createcardrequest model)
        {
            virtualcardresponse response = new virtualcardresponse();
            try
            {
                StripeConfiguration.ApiKey = _appSettings.StripeApiKey;

                #region Create Card Hodler
                var cardholderoptions = new CardholderCreateOptions
                {
                    Billing = new CardholderBillingOptions
                    {
                        Address = new AddressOptions
                        {
                            Line1 = model.address.line1,
                            Line2 = model.address.line2,
                            City = model.address.city,
                            State = model.address.state,
                            PostalCode = model.address.postalcode,
                            Country = model.address.country,
                        },
                    },
                    Email = "jenny.rosen@example.com",
                    PhoneNumber = "+18008675309",
                    Name = model.fullname,
                    Status = "active",
                    Type = "individual",
                    SpendingControls = new CardholderSpendingControlsOptions
                    {
                        SpendingLimits = new System.Collections.Generic.List<CardholderSpendingControlsSpendingLimitOptions>
                        {
                            new CardholderSpendingControlsSpendingLimitOptions()
                            {
                                Amount = model.amount,
                                Interval = "all_time"
                            }
                        },
                        SpendingLimitsCurrency = "usd"
                    }
                };

                var service = new CardholderService();
                var cardholder = service.Create(cardholderoptions);
                #endregion

                #region Create Virtual Card
                var options = new CardCreateOptions
                {
                    Cardholder = cardholder.Id,
                    Type = "virtual",
                    Currency = "usd",
                    
                };

                var cardservice = new CardService();
                var card = cardservice.Create(options);
                #endregion

                #region Active Virtual Card
                var activecardoptions = new CardUpdateOptions
                {
                    Status = "active",
                };

                var activecardservice = new CardService();
                var activecard = activecardservice.Update(cardholder.Id, activecardoptions);
                #endregion

            }
            catch (Exception ex)
            {
                response.message = ex.Message;
            }

            return response;
        }

    }
}
