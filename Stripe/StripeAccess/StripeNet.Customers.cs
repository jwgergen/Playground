using Stripe;
using System.Diagnostics;

namespace StripeAccess
{
    public static partial class StripeNet
    {

        /// <summary>
        /// CreaetCustomer:
        /// Creates a new customer.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static Customer CreateCustomer(string name, string email, string cardNumber = null!)
        {

            StripeConfiguration.ApiKey = sn_Token;

            try
            {
                CustomerCreateOptions options = new()
                {
                    Name = name,
                    Email = email,
                    Description = "Jim's test Customer",
                };
                var service = new CustomerService();
                var customer = service.Create(options);
                // create a fake card
                CardCreateOptions ccOptions = new()
                {
                    Source = "tok_visa",
                };
                var cardService = new CardService();
                var card = cardService.Create(customer.Id, ccOptions);

                return customer;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                throw;
            }
        }

        public static void DeleteCustomer(string customerId)
        {
            StripeConfiguration.ApiKey = sn_Token;
            try
            {
                var service = new CustomerService();
                service.Delete(customerId);
            }
            catch(Exception e)
            {
                Trace.TraceError(e.Message);
                throw;
            }
        }




        /// <summary>
        /// GetCustomers:
        /// Get a list of all customers.
        /// </summary>
        /// <returns>
        /// list of customers.
        /// </returns>
        public static StripeList<Customer> GetCustomers(long limit = 20)
        {
            StripeConfiguration.ApiKey = sn_Token;

            try
            {
                CustomerListOptions options = new()
                {
                    Limit = limit,
                };
                var service = new CustomerService();
                StripeList<Customer> results = service.List(options);
                if (results.StripeResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // throw an exception in the event that this request if not successful.
                    Exception inEx = new("Status from stripe is: " + results.StripeResponse.StatusCode.ToString());
                    Exception ex = new("Failed to obtain results ", inEx);
                    throw ex;
                }

                Trace.TraceInformation("Successfully retrieved Customers");
                return results;
            }
            catch (Exception e)
            {
                // do something with the error here.
                // in the real world, here we would log this error using a logging facility like Trace
                // (which would have custom listenter(s) to log to a facility like a file or the event log or both)
                Trace.TraceError(e.Message);
                // result is unreliable so we will return and empty result
                // return new StripeList<Customer>();
                // -- or --
                // we can rethrow the exception so the caller will be notified that there has been an error.
                // here we use just throw to maintain the stack information
                throw;
            }
            finally
            {
                // this block is entered no matter what the condition in the try block
                // in this case it is irrevelent.  Showing here for completeness...
            }

        }

        /// <summary>
        /// GetCustomer:
        /// Retrieves a customer from stripe
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>
        /// If successful returns a customer to the caller.
        /// </returns>
        public static Customer GetCustomer(string Id)
        {
            StripeConfiguration.ApiKey = sn_Token;

            try
            {
                var service = new CustomerService();
                Customer customer = service.Get(Id);
                return customer;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                throw;
            }
        }
    }
}