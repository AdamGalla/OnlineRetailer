using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Models;
using RestSharp;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repository;

        public OrdersController(IRepository<Order> repos)
        {
            repository = repos;
        }

        // GET: orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        [HttpGet("customer/{id}", Name = "GetAllByCustomerId")]
        public IActionResult GetAllByCustomerId(int id)
        {
            var item = repository.GetAllByCustomerId(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
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
            

            // Call ProductApi to get the product ordered
            // You may need to change the port number in the BaseUrl below
            // before you can run the request.
            foreach (var orderLine in order.OrderLine) 
            {
                try 
                {
                    RestClient c = new RestClient("http://productapi/Products/");
                    var request = new RestRequest(orderLine.ProductId.ToString());
                    var response = c.GetAsync<Product>(request);
                    response.Wait();
                    var orderedProduct = response.Result;

                    if (orderLine.Quantity <= orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                    {
                        // reduce the number of items in stock for the ordered product,
                        // and create a new order.
                        orderedProduct.ItemsReserved += orderLine.Quantity;
                        var updateRequest = new RestRequest(orderedProduct.Id.ToString());
                        updateRequest.AddJsonBody(orderedProduct);
                        var updateResponse = c.PutAsync(updateRequest);
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
                    return BadRequest("Something went wrong");
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
