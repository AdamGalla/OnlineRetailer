using System.Collections.Generic;
using System.Linq;
using OrderApi.Models;
using System;

namespace OrderApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(OrderApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Orders.Any())
            {
                return;   // DB has been seeded
            }
            List<OrderLine> orderLines = new List<OrderLine>
            {
                new OrderLine { ProductId = 1, Quantity = 2, OrderId = 1},
                new OrderLine { ProductId = 2, Quantity = 3, OrderId = 1},
                new OrderLine { ProductId = 3, Quantity = 1, OrderId = 1},
            };

            List<Order> orders = new List<Order>
            {
                new Order { CustomerId = 1, Date = DateTime.Today, Status= "Pending", OrderLine = orderLines }
            };

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
