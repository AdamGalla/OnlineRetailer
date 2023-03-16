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

    private Dictionary<int, int> _orderAcceptedCount;

    // The service _provider is passed as a parameter, because the class needs
    // access to the product repository. With the service _provider, we can create
    // a service scope that can provide an instance of the order repository.
    public MessageListener(IServiceProvider provider, string connectionString)
    {
        _provider = provider;
        _connectionString = connectionString;
        _orderAcceptedCount = new Dictionary<int, int>();
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
        Console.WriteLine("Accepted message for " + message.OrderId);
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var orderRepos = services.GetService<IRepository<Order>>();
        try
        {
            if (_orderAcceptedCount.TryGetValue(message.OrderId, out int currentValue))
            {
                currentValue++;
                _orderAcceptedCount[message.OrderId] = currentValue;
                Console.WriteLine($"{currentValue} Accept for {message.OrderId}");
                if (currentValue == 2)
                {
                    _orderAcceptedCount.Remove(message.OrderId);
                    // Mark order as completed
                    var order = orderRepos.Get(message.OrderId);
                    order.Status = OrderStatus.Completed;
                    orderRepos.Edit(order);
                }
            }
            else
            {
                Console.WriteLine("First accept for " + message.OrderId);
                _orderAcceptedCount.Add(message.OrderId, 1);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Encountered Exception: {ex.Message}");
        }
    }

    private void HandleOrderRejected(OrderRejectedMessage message)
    {
        using var scope = _provider.CreateScope();
        var services = scope.ServiceProvider;
        var orderRepos = services.GetService<IRepository<Order>>();

        Console.WriteLine("Reject for " + message.OrderId);

        if (_orderAcceptedCount.ContainsKey(message.OrderId))
        {
            _orderAcceptedCount.Remove(message.OrderId);
        }

        // Delete tentative order.
        orderRepos.Remove(message.OrderId);
    }
}
