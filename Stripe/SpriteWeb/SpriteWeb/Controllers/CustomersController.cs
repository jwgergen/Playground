using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using StripeAccess;

namespace StripeWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        // GET: api/<CustomersController>
        [HttpGet]
        [ResponseCache(NoStore = true)]
        public IEnumerable<Customer> Get()
        {
            return StripeNet.GetCustomers();
        }

        // GET api/<CustomersController>/5
        [HttpGet("{id}")]
        public object Get(string id)
        {
            if (long.TryParse(id, out long limit))
            {
                return StripeNet.GetCustomers(limit);
            }
            return StripeNet.GetCustomer(id);
        }

        // POST api/<CustomersController>
        [HttpPost]
        public Customer Post([FromForm] Models.CustomerModel model)
        {
            return StripeNet.CreateCustomer(model.Name!, model.Email!);
        }

        // PUT api/<CustomersController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<CustomersController>/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            StripeNet.DeleteCustomer(id);
        }
    }
}
