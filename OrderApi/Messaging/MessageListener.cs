using EasyNetQ;
using OrderApi.Data;
using OrderApi.Models;
using SharedDTOs.Models;

namespace OrderApi.Messaging;

public class MessageListener
{
    private readonly IServiceProvider _provider;
    private readonly string _connectionString;
    private IBus _bus;

    // The service _provider is passed as a parameter, because the class needs
    // access to the product repository. With the service _provider, we can create
    // a service scope that can provide an instance of the order repository.
    public MessageListener(IServiceProvider provider, string connectionString)
    {
        _provider = provider;
        _connectionString = connectionString;
    }

    public void Start()
    {
        using (_bus = RabbitHutch.CreateBus(_connectionString))
        {
            _bus.PubSub.Subscribe<OrderAcceptedMessage>("orderApiAccepted", HandleOrderAccepted);

            _bus.PubSub.Subscribe<OrderRejectedMessage>("orderApiRejected", HandleOrderRejected);

            // Block the thread so that it will not exit and stop subscribing.
            lock (this)
            {
                Monitor.Wait(this);
            }
        }

    }

    private void HandleOrderAccepted(OrderAcceptedMessage message)
    {
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var orderRepos = services.GetService<IRepository<Order>>();

        // Mark order as completed
        var order = orderRepos.Get(message.OrderId);
        order.Status = OrderStatus.Completed;
        orderRepos.Edit(order);
    }

    private void HandleOrderRejected(OrderRejectedMessage message)
    {
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var orderRepos = services.GetService<IRepository<Order>>();

        // Delete tentative order.
        orderRepos.Remove(message.OrderId);
    }
}
