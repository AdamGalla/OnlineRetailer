using OrderApi.Models;
using EasyNetQ;
using SharedDTOs;
using SharedDTOs.DtoConverters;
using SharedDTOs.Models;

namespace OrderApi.Messaging;

public class MessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IBus _bus;

    public MessagePublisher(string connectionString)
    {
        _bus = RabbitHutch.CreateBus(connectionString);
    }

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
        _bus.Dispose();
    }

    public void PublishOrderStatusChangedMessage(int? customerId, IList<OrderLine> orderLines, int orderId, string topic)
    {

        var message = new OrderStatusChangedMessage
        {
            CustomerId = customerId,
            OrderId = orderId,
            OrderLines = DTOConverter<OrderLine, OrderLineDto>.FromList(orderLines).ToList()
        };

        _bus.PubSub.Publish(message, topic);
    }
}
