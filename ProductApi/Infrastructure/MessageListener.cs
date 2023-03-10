using EasyNetQ;
using ProductApi.Data;
using ProductApi.Models;
using SharedDTOs.Models;

namespace ProductApi.Infrastructure;

public class MessageListener
{
    private readonly IServiceProvider _provider;
    private readonly string _connectionString;
    private IBus _bus;

    // The service _provider is passed as a parameter, because the class needs
    // access to the product repository. With the service _provider, we can create
    // a service scope that can provide an instance of the product repository.
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
            _bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiCompleted",
                HandleOrderCompleted, x => x.WithTopic("completed"));
            _bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiShipped",
                HandleOrderShipped, x => x.WithTopic("shipped"));
            _bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiCancelled",
                HandleOrderCancelled, x => x.WithTopic("cancelled"));
            _bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiPaid",
                HandleOrderPaid, x => x.WithTopic("paid"));
        }

        // Block the thread so that it will not exit and stop subscribing.
        lock (this)
        {
            Monitor.Wait(this);
        }

    }

    // Implement an event handler for each of these events.
    private void HandleOrderCompleted(OrderStatusChangedMessage message)
    {
        // A service scope is created to get an instance of the product repository.
        // When the service scope is disposed, the product repository instance will
        // also be disposed.
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var productRepos = services.GetService<IRepository<Product>>();

        // Reserve items of ordered product (should be a single transaction).
        // Beware that this operation is not idempotent.
        try
        {
            foreach (var orderLine in message.OrderLines)
            {
                var product = productRepos.Get(orderLine.ProductId);
                product.ItemsReserved += orderLine.Quantity;
                productRepos.Edit(product);
            }
            var replyMessage = new OrderAcceptedMessage
            {
                OrderId = message.OrderId
            };

            _bus.PubSub.Publish(replyMessage);

        }
        catch
        {
            var replyMessage = new OrderRejectedMessage
            {
                OrderId = message.OrderId
            };


            _bus.PubSub.Publish(replyMessage);
        }

    }

    private void HandleOrderShipped(OrderStatusChangedMessage message)
    {
        // A service scope is created to get an instance of the product repository.
        // When the service scope is disposed, the product repository instance will
        // also be disposed.
        using (var scope = _provider.CreateScope())
        {
            var services = scope.ServiceProvider;
            var productRepos = services.GetService<IRepository<Product>>();

            // Delete items of ordered product (should be a single transaction).
            // Beware that this operation is not idempotent.
            foreach (var orderLine in message.OrderLines)
            {
                var product = productRepos.Get(orderLine.ProductId);
                product.ItemsReserved -= orderLine.Quantity;
                product.ItemsInStock -= orderLine.Quantity;
                productRepos.Edit(product);

            }
        }
    }

    private void HandleOrderCancelled(OrderStatusChangedMessage message)
    {
        // A service scope is created to get an instance of the product repository.
        // When the service scope is disposed, the product repository instance will
        // also be disposed.
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var productRepos = services.GetService<IRepository<Product>>();

        // Delete items of ordered product (should be a single transaction).
        // Beware that this operation is not idempotent.
        foreach (var orderLine in message.OrderLines)
        {
            var product = productRepos.Get(orderLine.ProductId);
            product.ItemsReserved -= orderLine.Quantity;
            productRepos.Edit(product);

        }
    }
    private void HandleOrderPaid(OrderStatusChangedMessage message)
    {
        // A service scope is created to get an instance of the product repository.
        // When the service scope is disposed, the product repository instance will
        // also be disposed.
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var productRepos = services.GetService<IRepository<Product>>();

        //TODO add logic if order has been paid

    }
}
