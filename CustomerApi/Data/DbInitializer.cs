using System.Collections.Generic;
using System.Linq;
using CustomerApi.Models;
using System;

namespace CustomerApi.Data;

public class DbInitializer : IDbInitializer
{
    // This method will create and seed the database.
    public void Initialize(CustomerApiContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Look for any Products
        if (context.Customers.Any())
        {
            return;   // DB has been seeded
        }

        List<Customer> customers = new List<Customer>
        {
            new Customer { Name = "Adam", Email = "asdfg@gmail.com", Phone = "123456789", BillingAddress = "test", ShippingAddress = "test", CreditStanding = true }
        };

        context.Customers.AddRange(customers);
        context.SaveChanges();
    }
}
