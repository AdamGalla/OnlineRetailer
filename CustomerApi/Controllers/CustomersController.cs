using CustomerApi.Models;
using Microsoft.AspNetCore.Mvc;
using CustomerApi.Data;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomerApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IRepository<Customer> repository;

        public CustomersController(IRepository<Customer> repos)
        {
            repository = repos;
        }

        // GET customers
        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return repository.GetAll();
        }

        // GET customers/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST customers
        [HttpPost]
        public IActionResult Post([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }

            var newCustomer = repository.Add(customer);

            return CreatedAtRoute("GetCustomer", new { id = newCustomer.Id }, newCustomer);
        }

        // PUT customers/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Customer customer)
        {
            if (customer == null || customer.Id != id)
            {
                return BadRequest();
            }

            var modifiedCustomer = repository.Get(id);

            if (modifiedCustomer == null)
            {
                return NotFound();
            }

            modifiedCustomer.Name = customer.Name;
            modifiedCustomer.ShippingAddress = customer.ShippingAddress;
            modifiedCustomer.Phone = customer.Phone;
            modifiedCustomer.Email = customer.Email;
            modifiedCustomer.BillingAddress = customer.BillingAddress;
            modifiedCustomer.CreditStanding = customer.CreditStanding;

            repository.Edit(modifiedCustomer);
            return new NoContentResult();
        }
        // DELETE customers/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return NotFound();
            }

            repository.Remove(id);
            return new NoContentResult();
        }
    }
}
