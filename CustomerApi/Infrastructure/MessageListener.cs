using CustomerApi.Data;
using CustomerApi.Models;
using EasyNetQ;
using Newtonsoft.Json.Bson;
using SharedDTOs.Models;

namespace CustomerApi.Infrastructure;

public class MessageListener
{
    private readonly IServiceProvider _provider;
    private readonly string _connectionString;
    private IBus _bus;

    public MessageListener(IServiceProvider provider, string connectionString)
    {
        _provider = provider;
        _connectionString = connectionString;
    }

    public void Start()
    {
        using (_bus = RabbitHutch.CreateBus(_connectionString))
        {
            // Add code to subscribe to other OrderStatusChanged events:
            _bus.PubSub.Subscribe<OrderStatusChangedMessage>("customerApiCompleted", HandleOrderCompleted, x => x.WithTopic("completed"));
            _bus.PubSub.Subscribe<OrderStatusChangedMessage>("customerApiCancelled", HandleOrderCancelled, x => x.WithTopic("cancelled"));
            _bus.PubSub.Subscribe<OrderStatusChangedMessage>("customerApiPaid", HandleOrderPaid, x => x.WithTopic("paid"));


            // Block the thread so that it will not exit and stop subscribing.
            lock (this)
            {
                Monitor.Wait(this);
            }
        }
    }

    private void HandleOrderCompleted(OrderStatusChangedMessage message)
    {
        Console.WriteLine("Handling order completed " + message.OrderId);
        // A service scope is created to get an instance of the customer repository.
        // When the service scope is disposed, the customer repository instance will
        // also be disposed.
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var customerRepos = services.GetService<IRepository<Customer>>();

        // Check if customer exists
        if(customerRepos is null)
        {
            var replyMessage = new OrderRejectedMessage
            {
                OrderId = message.OrderId
            };
            _bus.PubSub.Publish(replyMessage);
            Console.WriteLine("Order rejected; no repo; " + message.OrderId);
            throw new Exception();
        }

        var customer = customerRepos.Get(message.CustomerId!.Value);
        if (customer is null || customer.CreditStanding)
        {
            var replyMessage = new OrderRejectedMessage
            {
                OrderId = message.OrderId
            };
            _bus.PubSub.Publish(replyMessage);
            Console.WriteLine($"Order rejected {message.OrderId} customer: {customer.Id} credits: {customer.CreditStanding}");
            throw new Exception();
        }

        customer.CreditStanding = true;
        var replyAcceptedMessage = new OrderAcceptedMessage
        {
            OrderId = message.OrderId
        };
        _bus.PubSub.Publish(replyAcceptedMessage);
        Console.WriteLine("Order accepts " + message.OrderId);
        customerRepos.Edit(customer);
    }

    private void HandleOrderCancelled(OrderStatusChangedMessage message)
    {
        // A service scope is created to get an instance of the customer repository.
        // When the service scope is disposed, the customer repository instance will
        // also be disposed.
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var customerRepos = services.GetService<IRepository<Customer>>();

        var customer = customerRepos.Get(message.CustomerId!.Value);
        customer.CreditStanding = false;
        customerRepos.Edit(customer);
    }

    private void HandleOrderPaid(OrderStatusChangedMessage message)
    {
        // A service scope is created to get an instance of the customer repository.
        // When the service scope is disposed, the customer repository instance will
        // also be disposed.
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var customerRepos = services.GetService<IRepository<Customer>>();

        var customer = customerRepos.Get(message.CustomerId!.Value);
        customer.CreditStanding = false;
        customerRepos.Edit(customer);

    }

}
