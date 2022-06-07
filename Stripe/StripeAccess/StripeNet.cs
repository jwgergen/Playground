using Stripe;
using System.Diagnostics;

namespace StripeAccess
{
    public static partial class StripeNet
    {
        // this should actually be in a secret file, For simplicity I am putting it here.
        private static readonly string sn_ApiKey = "pk_test_XSda0Pymc5PSgDiceveQ6iSs";
        private static readonly string sn_Token = "sk_test_2tf7T5a1JPvJ6wMKSSWrcCTP";
        private static readonly string sn_ApiBase = "https://api.stripe.com/v1";

        public enum RefundType
        {
            Full,
            Partial
        };

        /// <summary>
        /// CheckConnect:
        /// Simply attempts to fetch a charge to see if the connection is working correctly.
        /// this uses the Stripe client from the string.net sdk
        /// </summary>
        /// <returns></returns>
        public static bool CheckConnect()
        {
            StripeClient client = new(apiKey: sn_Token, apiBase: sn_ApiBase);
            var options = new ChargeListOptions() { Limit = 1 };
            try
            {
                var result = client.RequestAsync<StripeList<Charge>>(HttpMethod.Get, "/charges", options, null);
                if (result.Status == TaskStatus.Faulted)
                {
                    Trace.TraceWarning("Unable to connect to {0}", sn_ApiBase);
                    return false;
                }

                // check to see if we have a list
                StripeList<Charge> list = result.Result;
                // if there are no items in the list we consider that failure
                // however in a real world scenario, there may be no charges...
                if (list.Any())
                {
                    // no charges found
                    Trace.TraceWarning("No charges found in CheckConnect");
                }
                else
                {
                    Trace.TraceInformation("Fount 1 charge({0}", list.First().Id);
                }

                Trace.TraceInformation("Successful connection to {0}", sn_ApiBase);
                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError("Error occurred while trying to connect and query for a charge {0}", e.Message);
                // because the is a "check" method, then we will not throw this excption up to the caller.
                // we will instead, return false.
                // the user or developer will check the log for the problem.
                return false;
            }
        }

        /// <summary>
        /// GetTransfers:
        /// Gets a list of transfers based on data passed.
        /// </summary>
        /// <param name="dFrom"></param>
        /// <returns>
        /// A collection of transfers.
        /// </returns>
        public static StripeList<Transfer> GetTransfers(DateTime dFrom)
        {
            StripeConfiguration.ApiKey = sn_Token;

            var transferOptions = new TransferListOptions()
            {
                Created = dFrom,
            };

            try
            {
                var service = new TransferService();
                StripeList<Transfer> results = service.List(transferOptions);
                if (results.StripeResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // throw an exception in the event that this request if not successful.
                    Exception inEx = new("Status from stripe is: " + results.StripeResponse.StatusCode.ToString());
                    Exception ex = new("Failed to obtain results for dates on or after " + dFrom.ToLongDateString() + ".", inEx);
                    throw ex;
                }

                Trace.TraceInformation("Successfully retrieved payments for the secified date {0}", dFrom.ToLongDateString());


                return results;
            }
            catch (Exception e)
            {
                // do something with the error here.
                // in the real world, here we would log this error using a logging facility like Trace
                // (which would have custom listenter(s) to log to a facility like a file or the event log or both)
                Trace.TraceError(e.Message);
                // result is unreliable so we will return and empty result
                // return new StripeSearchResult<PaymentIntent>();
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
        /// Payments??
        /// </summary>
        /// <param name="dFrom"></param>
        /// <returns></returns>
        public static List<Charge> GetPaidCharges(DateTime dFrom, int limit)
        {
            StripeConfiguration.ApiKey = sn_Token;

            try
            {
                // stripe uses unix time.
                long timetopass = (long)dFrom.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                ChargeSearchOptions options = new()
                {
                    Query = "created>=" + timetopass.ToString() + " status : \"succeeded\"",
                    Limit = limit,
                };

                ChargeService service = new();
                var results = service.Search(options);
                if (results.StripeResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // throw an exception in the event that this request if not successful.
                    Exception inEx = new("Status from stripe is: " + results.StripeResponse.StatusCode.ToString());
                    Exception ex = new("Failed to obtain results for dates on or after " + dFrom.ToLongDateString() + ".", inEx);
                    throw ex;
                }

                Trace.TraceInformation("Successfully retrieved payments for the secified date {0}", dFrom.ToLongDateString());


                return results.ToList();
                //.Where(co => co.Captured && !co.Refunded)
                //.ToList();
            }
            catch (Exception e)
            {
                // do something with the error here.
                // in the real world, here we would log this error using a logging facility like Trace
                // (which would have custom listenter(s) to log to a facility like a file or the event log or both)
                Trace.TraceError(e.Message);
                // result is unreliable so we will return and empty result
                // return new StripeSearchResult<PaymentIntent>();
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
        /// GetPaymentsFromDate:
        /// This method should return the payment intents that have succeeded since the date specified in the parameter
        /// </summary>
        /// <param name="dFrom">
        /// The starting date to search payments
        /// </param>
        /// <returns>
        /// if successful a collection of PaymentIntents
        /// </returns>
        public static StripeSearchResult<PaymentIntent> GetPaymentsFromDate(DateTime dFrom)
        {
            // use the secret key...
            StripeConfiguration.ApiKey = sn_Token;

            // stripe uses unix time.
            DateTime dEpoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan tsFrom = dFrom - dEpoch;
            long timetopass = (long)tsFrom.TotalSeconds;
            var piOptions = new PaymentIntentSearchOptions()
            {
                Query = "status: 'succeeded' AND created >= '" + timetopass.ToString() + "'",
            };

            try
            {
                // regarding the API I am using,
                // I am not sure if this is the correct api to retrieve this 
                // but it is the closest thing I have found to track payments.
                // the documentation doesn't seem to cover just "payments" 
                // except in the form of payouts which from what I can tell
                // is a different scenario than payments.
                var service = new PaymentIntentService();
                StripeSearchResult<PaymentIntent> results = service.Search(piOptions);
                if (results.StripeResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // throw an exception in the event that this request if not successful.
                    Exception inEx = new("Status from stripe is: " + results.StripeResponse.StatusCode.ToString());
                    Exception ex = new("Failed to obtain results for dates on or after " + dFrom.ToLongDateString() + ".", inEx);
                    throw ex;
                }

                Trace.TraceInformation("Successfully retrieved payments for the secified date {0}", dFrom.ToLongDateString());


                return results;
            }
            catch (Exception e)
            {
                // do something with the error here.
                // in the real world, here we would log this error using a logging facility like Trace
                // (which would have custom listenter(s) to log to a facility like a file or the event log or both)
                Trace.TraceError(e.Message);
                // result is unreliable so we will return and empty result
                // return new StripeSearchResult<PaymentIntent>();
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
        /// CreateCharge:
        /// Creates a charge.
        /// Placeholders exist to keep it simple
        /// // this is solely to demonstrate calling the "strip" api from C# using the .NET sdk for stripe.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Charge CreateCharge(string customerId, long amount)
        {
            StripeConfiguration.ApiKey = sn_Token;

            var chargeOptions = new ChargeCreateOptions()
            {
                Amount = amount,
                Currency = "usd",
                Customer = customerId,
                //Capture = false,
                Description = "JG - Create this charge for this assessment.",
            };

            try
            {
                var service = new ChargeService();
                var charge = service.Create(chargeOptions);
                // next we will capture this charge
                // this will only succeed if the charge has not been captures.
                // the only time I believe that you would want to do this is 
                // if your were planning to create many charges in succession
                // then apply a payment afterwards for the entire invoice
                // in which cast the capture would not be done here.
                //try
                //{
                //    charge = service.Capture(charge.Id);
                //}
                //catch(StripeException ce)
                //{
                //    Trace.TraceInformation("Likely the charge has already been captured. ({0})", ce.Message);
                //    // no need to do anything else
                //}
                return charge;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                throw;
            }
        }

        /// <summary>
        /// CreateCharge:
        /// Creates a charge.
        /// Placeholders exist to keep it simple
        /// // this is solely to demonstrate calling the "strip" api from C# using the .NET sdk for stripe.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Charge CreateCharge(Customer customer, long amount)
        {
            return CreateCharge(customer.Id, amount);
        }

        /// <summary>
        /// GetCharge:
        /// fetches a charge based on the ID passed
        /// </summary>
        /// <param name="chargeId">
        /// Id of the charge to query
        /// </param>
        /// <returns>
        /// If found the charge requested.
        /// </returns>
        public static Charge GetCharge(string chargeId)
        {
            StripeConfiguration.ApiKey = sn_Token;

            try
            {
                var service = new ChargeService();
                Charge result = service.Get(chargeId);
                return result;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                throw;
            }
        }
        /// <summary>
        /// GetChargesForCustomer:
        /// Get the charges for the specified customer.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns>
        /// StripeSearchResult<Charge>:
        /// The list of charges for this customer.
        /// </returns>
        public static StripeSearchResult<Charge> GetChargesForCustomer(string customerId)
        {
            StripeConfiguration.ApiKey = sn_Token;

            try
            {
                var chargeSearchOptions = new ChargeSearchOptions()
                {
                    Query = "customer: \"" + customerId + "\"",
                    Limit = 50,
                };

                var service = new ChargeService();
                var results = service.Search(chargeSearchOptions);
                if (results.StripeResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // throw an exception in the event that this request if not successful.
                    Exception inEx = new("Status from stripe is: " + results.StripeResponse.StatusCode.ToString());
                    Exception ex = new("Failed to obtain results ", inEx);
                    throw ex;
                }

                Trace.TraceInformation("Successfully retrieved charges for customer {0}", customerId);
                return results;
            }
            catch (Exception e)
            {
                // do something with the error here.
                // in the real world, here we would log this error using a logging facility like Trace
                // (which would have custom listenter(s) to log to a facility like a file or the event log or both)
                Trace.TraceError(e.Message);
                // result is unreliable so we will return and empty result
                // return new StripeSearchResult<Charge>();
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
        /// RefundCharge:
        /// Creates a refund for a specific charge
        /// </summary>
        /// <param name="refundType">
        /// Types of refunds are:
        /// RefundType.Full or RefundType.Partial
        /// </param>
        /// <param name="chargeid"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Refund? RefundCharge(RefundType refundType, string chargeId, long amount)
        {
            Charge charge = GetCharge(chargeId);
            if (null != charge)
            {
                return RefundCharge(refundType, charge, amount);
            }
            return null;
        }

        /// <summary>
        /// RefundCharge:
        /// Creates a refund for a specific charge
        /// </summary>
        /// <param name="refundType">
        /// Types of refunds are:
        /// RefundType.Full or RefundType.Partial
        /// </param>
        /// <param name="charge"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Refund? RefundCharge(RefundType refundType, Charge charge, long amount)
        {
            StripeConfiguration.ApiKey = sn_Token;

            try
            {
                if (refundType == RefundType.Full)
                {
                    amount = Math.Abs(charge.AmountRefunded - charge.Amount);
                }
                else if (amount > charge.Amount)
                {
                    throw new Exception("Amount cannot be more than the carge amount.");
                }

                var refundOptions = new RefundCreateOptions()
                {
                    Charge = charge.Id,
                    Amount = amount,
                };

                var service = new RefundService();
                Refund refund = service.Create(refundOptions);
                return refund;
            }
            catch (Exception e)
            {
                // do something with the error here.
                // in the real world, here we would log this error using a logging facility like Trace
                // (which would have custom listenter(s) to log to a facility like a file or the event log or both)
                Trace.TraceError(e.Message);
                // result is unreliable so we will return and empty result
                // return null
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
        /// GetFiveAccounts:
        /// retrieves 5 accounts from the service...
        /// </summary>
        /// <returns>
        /// a list of accounts.
        /// </returns>
        public static StripeList<Account> GetFiveAccounts()
        {
            StripeConfiguration.ApiKey = sn_Token;

            var accountOptions = new AccountListOptions() { Limit = 5 };
            try
            {
                var service = new AccountService();
                StripeList<Account> results = service.List(accountOptions);
                if (results.StripeResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // throw an exception in the event that this request if not successful.
                    Exception inEx = new("Status from stripe is: " + results.StripeResponse.StatusCode.ToString());
                    Exception ex = new("Failed to obtain results ", inEx);
                    throw ex;
                }

                Trace.TraceInformation("Successfully retrieved Accounts");
                return results;
            }
            catch (Exception e)
            {
                // do something with the error here.
                // in the real world, here we would log this error using a logging facility like Trace
                // (which would have custom listenter(s) to log to a facility like a file or the event log or both)
                Trace.TraceError(e.Message);
                // result is unreliable so we will return and empty result
                // return new StripeList<Account>();
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
    }
}
