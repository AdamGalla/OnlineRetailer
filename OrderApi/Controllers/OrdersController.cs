using Microsoft.AspNetCore.Mvc;
using SharedDTOs;
using OrderApi.Data;
using OrderApi.Models;
using RestSharp;
using OrderApi.Messaging;

namespace OrderApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repository;
    private readonly IMessagePublisher _messagePublisher;

    public OrdersController(
        IRepository<Order> repos,
        IMessagePublisher publisher)
    {
        _repository = repos as IOrderRepository;
        _messagePublisher = publisher;
    }

    // GET: orders
    [HttpGet]
    public IEnumerable<Order> Get()
    {
        return _repository.GetAll();
    }

    // GET orders/5
    [HttpGet("{id}", Name = "GetOrder")]
    public ActionResult<Order> Get(int id)
    {
        var item = _repository.Get(id);
        if (item == null)
        {
            return NotFound();
        }
        return new OkObjectResult(item);
    }

    [HttpGet("customer/{id}", Name = "GetAllByCustomerId")]
    public ActionResult<IEnumerable<Order>> GetAllByCustomerId(int id)
    {
        var item = _repository.GetAllByCustomerId(id);
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
        
        var client = new RestClient("http://customerapi/Customers/");
        var customerResponse = client.GetAsync<CustomerDto>(new RestRequest(order.CustomerId.ToString()));
        customerResponse.Wait();
        if (!customerResponse.IsCompletedSuccessfully || customerResponse.Result == null || customerResponse.Result.Id != order.CustomerId)
        {
            return BadRequest("Customer with the given id does not exist!");
        }

        try
        {
            // Create a tentative order.
            order.Status = OrderStatus.Pending;
            var newOrder = _repository.Add(order);

            // Publish OrderStatusChangedMessage. 
            _messagePublisher.PublishOrderStatusChangedMessage
            (
                newOrder.CustomerId, newOrder.OrderLine.ToList(), newOrder.Id, "productApiCompleted"
            );


            // Wait until order status is "completed"
            bool completed = false;
            while (!completed)
            {
                var pendingOrder = _repository.Get(newOrder.Id);
                if (pendingOrder.Status == OrderStatus.Completed)
                    completed = true;
                Thread.Sleep(100);
            }

            return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
        }
        catch
        {
            return StatusCode(500, "An error happened. Try again.");
        }
    }

    // PUT orders/5/cancel
    // This action method cancels an order and publishes an OrderStatusChangedMessage
    // with topic set to "cancelled".
    [HttpPut("{id}/cancel")]
    public IActionResult Cancel(int id)
    {
        throw new NotImplementedException();

        // Add code to implement this method.
    }

    // PUT orders/5/ship
    // This action method ships an order and publishes an OrderStatusChangedMessage.
    // with topic set to "shipped".
    [HttpPut("{id}/ship")]
    public IActionResult Ship(int id)
    {
        throw new NotImplementedException();

        // Add code to implement this method.
    }

    // PUT orders/5/pay
    // This action method marks an order as paid and publishes a CreditStandingChangedMessage
    // (which have not yet been implemented), if the credit standing changes.
    [HttpPut("{id}/pay")]
    public IActionResult Pay(int id)
    {
        throw new NotImplementedException();

        // Add code to implement this method.
    }
}
