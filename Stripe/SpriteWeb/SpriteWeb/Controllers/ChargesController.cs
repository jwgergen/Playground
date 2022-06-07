using Microsoft.AspNetCore.Mvc;
using Stripe;
using StripeAccess;

namespace StripeWeb.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ChargesController : ControllerBase
    {
        //GET: api/<ChargesController>
        [HttpGet] //("{fromdate}/{limit}")]
        [ActionName("GetPayments")]
        public IEnumerable<Charge> GetPayments([FromQuery] Models.PaymentModel payment)
        {
            DateTime dFrom = DateTime.Parse(payment.FromDate!); // throws an exception if erroneous
            return StripeNet.GetPaidCharges(dFrom, int.Parse(payment.Limit!));
        }

        // GET api/<ChargesController>/5
        [HttpGet] //("{id}")]
        [ActionName("GetById")]
        public Charge GetById(string id)
        {
            return StripeNet.GetCharge(id);
        }

        [HttpGet] //("{id}")]
        [ActionName("GetByCustomer")]
        public IEnumerable<Charge> GetByCustomer(string id)
        {
            return StripeNet.GetChargesForCustomer(id);
        }
        // POST api/<ChargesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        [HttpPost]
        public Charge Create([FromForm] Models.ChargeModel charge)
        {
            return StripeNet.CreateCharge(charge.Id!, long.Parse(charge.Amount!));
        }

        // PUT api/<ChargesController>/5
        [HttpPut("{id}")]
        [ActionName("Put")]
        public void Put(string id, [FromBody] string value)
        {
        }

        [HttpPut]
        public void Refund([FromForm] Models.ChargeModel refund)
        {
            StripeNet.RefundCharge(StripeNet.RefundType.Partial,
                refund.Id!, long.Parse(refund.Amount!));
        }

        // DELETE api/<ChargesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
