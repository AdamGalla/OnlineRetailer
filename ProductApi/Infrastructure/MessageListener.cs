namespace ProductApi.Infrastructure;

public class MessageListener
{
    IServiceProvider provider;
    string connectionString;

    // The service provider is passed as a parameter, because the class needs
    // access to the product repository. With the service provider, we can create
    // a service scope that can provide an instance of the product repository.
    public MessageListener(IServiceProvider provider, string connectionString)
    {
        this.provider = provider;
        this.connectionString = connectionString;
    }

    public void Start()
    {
    }
}
