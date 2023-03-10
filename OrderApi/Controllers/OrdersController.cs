using Microsoft.AspNetCore.Mvc;
using SharedDTOs;
using OrderApi.Data;
using OrderApi.Models;
using RestSharp;
using OrderApi.Messaging.Gateways;
using OrderApi.Messaging;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repository;
        private readonly IServiceGateway<ProductDto> _productServiceGateway;
        private readonly IMessagePublisher _messagePublisher;

        public OrdersController(
            IRepository<Order> repos,
            IServiceGateway<ProductDto> gateway,
            IMessagePublisher publisher)
        {
            repository = repos;
            _productServiceGateway = gateway;
            _messagePublisher = publisher;
        }

        // GET: orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public ActionResult<Order> Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new OkObjectResult(item);
        }

        [HttpGet("customer/{id}", Name = "GetAllByCustomerId")]
        public ActionResult<IEnumerable<Order>> GetAllByCustomerId(int id)
        {
            var item = repository.GetAllByCustomerId(id);
            if (item == null)
            {
                return NotFound();
            }
            return new OkObjectResult(item);
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody]Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }
            bool isSucess = true;
            
            var client = new RestClient("http://customerapi/Customers/");
            var customerResponse = client.GetAsync<CustomerDto>(new RestRequest(order.CustomerId.ToString()));
            customerResponse.Wait();
            if (!customerResponse.IsCompletedSuccessfully || customerResponse.Result == null || customerResponse.Result.Id != order.CustomerId)
            {
                return BadRequest("Customer with the given id does not exist!");
            }

            // Call ProductApi to get the product ordered
            // You may need to change the port number in the BaseUrl below
            // before you can run the request.
            client = new RestClient("http://productapi/Products/");
            foreach (var orderLine in order.OrderLine) 
            {
                try 
                {
                    // Check if product has enough items in stock
                    var request = new RestRequest(orderLine.ProductId.ToString());
                    var response = client.GetAsync<ProductDto>(request);
                    response.Wait();
                    var orderedProduct = response.Result;

                    if (orderLine.Quantity <= orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                    {
                        // reduce the number of items in stock for the ordered product,
                        // and create a new order.
                        orderedProduct.ItemsReserved += orderLine.Quantity;
                        var updateRequest = new RestRequest(orderedProduct.Id.ToString());
                        updateRequest.AddJsonBody(orderedProduct);
                        var updateResponse = client.PutAsync(updateRequest);
                        updateResponse.Wait();
                    }
                    else
                    {
                        return BadRequest("Order product quantity exceeded the available product quantity");
                    }
                }
                catch(Exception ex) 
                {
                    isSucess = false;
                    return BadRequest($"Something went wrong {ex.GetType()}");
                }
            }

            if (isSucess)
            {
                var newOrder = repository.Add(order);
                return CreatedAtRoute("GetOrder",
                    new { id = newOrder.Id }, newOrder);
            }

            return BadRequest("Order was not created try again later");
        }
    }
}
