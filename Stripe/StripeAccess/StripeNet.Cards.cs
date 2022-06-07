using Stripe;
using System.Diagnostics;

namespace StripeAccess
{
    public static partial class StripeNet
    {
        public static StripeList<Card> GetCards(string customerid)
        {
            StripeConfiguration.ApiKey = sn_ApiKey;

            try
            {
                var service = new CardService();
                var results = service.List(customerid);
                return results;
            }
            catch(Exception e)
            {
                Trace.TraceError(e.Message);
                throw;
            }
        }
    }
}