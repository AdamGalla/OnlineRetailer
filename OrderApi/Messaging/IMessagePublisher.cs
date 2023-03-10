using OrderApi.Models;

namespace OrderApi.Messaging;

public interface IMessagePublisher
{
    void PublishOrderStatusChangedMessage(int? customerId, IList<OrderLine> orderLines, int orderId, string topic);
}
