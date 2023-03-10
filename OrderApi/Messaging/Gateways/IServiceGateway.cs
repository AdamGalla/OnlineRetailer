namespace OrderApi.Messaging.Gateways;

public interface IServiceGateway<T>
{
    T Get(int id);
}
