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
    private readonly IRepository<Order> _repository;
    private readonly IMessagePublisher _messagePublisher;

    public OrdersController(
        IRepository<Order> repos,
        IMessagePublisher publisher)
    {
        _repository = repos;
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
        
        /*var client = new RestClient("http://customerapi/Customers/");
        var customerResponse = client.GetAsync<CustomerDto>(new RestRequest(order.CustomerId.ToString()));
        customerResponse.Wait();
        if (!customerResponse.IsCompletedSuccessfully || customerResponse.Result == null || customerResponse.Result.Id != order.CustomerId)
        {
            return BadRequest("Customer with the given id does not exist!");
        }*/

        try
        {
            // Create a tentative order.
            order.Status = OrderStatus.Pending;
            var newOrder = _repository.Add(order);

            // Publish OrderStatusChangedMessage. 
            _messagePublisher.PublishOrderStatusChangedMessage
            (
                newOrder.CustomerId, newOrder.OrderLine.ToList(), newOrder.Id, "completed"
            );

            // Wait until order status is "completed"
            bool completed = false;
            int count = 0;
            while (!completed || count < 50)
            {
                var pendingOrder = _repository.Get(newOrder.Id);
                if (pendingOrder.Status == OrderStatus.Completed)
                    completed = true;
                Thread.Sleep(100);
                count++;
            }

            if(!completed)
            {
                return StatusCode(500, "An error happened. Try again.");
            }

            return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
        }
        catch(Exception ex)
        {
            return StatusCode(500, "An error happened. Try again. Message: " + ex.Message);
        }
    }

    // PUT orders/5/cancel
    // This action method cancels an order and publishes an OrderStatusChangedMessage
    // with topic set to "cancelled".
    [HttpPut("{id}/cancel")]
    public IActionResult Cancel(int id)
    {
        var order = _repository.Get(id);
        if(order.Status != OrderStatus.Completed && order.Status != OrderStatus.Pending)
        {
            return BadRequest("Order could not be cancelled as the status was not 'pending' nor 'completed'.");
        }
        order.Status = OrderStatus.Cancelled;
        _repository.Edit(order);
        _messagePublisher.PublishOrderStatusChangedMessage
        (
            order.CustomerId, order.OrderLine.ToList(), order.Id, "cancelled"
        );
        return Ok(id);
    }

    // PUT orders/5/ship
    // This action method ships an order and publishes an OrderStatusChangedMessage.
    // with topic set to "shipped".
    [HttpPut("{id}/ship")]
    public IActionResult Ship(int id)
    {
        var order = _repository.Get(id);
        if (order.Status != OrderStatus.Completed)
        {
            return BadRequest("Order could not be set to shipped as the status was not 'completed'.");
        }
        order.Status = OrderStatus.Shipped;
        _repository.Edit(order);
        _messagePublisher.PublishOrderStatusChangedMessage
        (
            order.CustomerId, order.OrderLine.ToList(), order.Id, "shipped"
        );
        return Ok(id);
    }

    // PUT orders/5/pay
    // This action method marks an order as paid and publishes a CreditStandingChangedMessage
    // (which have not yet been implemented), if the credit standing changes.
    [HttpPut("{id}/pay")]
    public IActionResult Pay(int id)
    {
        var order = _repository.Get(id);
        if (order.Status != OrderStatus.Shipped)
        {
            return BadRequest("Order could not be set to paid as the status was not 'shipped'.");
        }
        order.Status = OrderStatus.Paid;
        _repository.Edit(order);
        _messagePublisher.PublishOrderStatusChangedMessage
        (
            order.CustomerId, order.OrderLine.ToList(), order.Id, "paid"
        );
        return Ok(id);
    }
}
