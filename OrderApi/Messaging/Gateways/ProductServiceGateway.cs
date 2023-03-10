using RestSharp;
using SharedDTOs;

namespace OrderApi.Messaging.Gateways;

public class ProductServiceGateway
{
    string _productServiceBaseUrl;

    public ProductServiceGateway(string baseUrl)
    {
        _productServiceBaseUrl = baseUrl;
    }

    public ProductDto Get(int id)
    {
        var client = new RestClient(_productServiceBaseUrl);

        var request = new RestRequest(id.ToString());
        var response = client.GetAsync<ProductDto>(request);
        response.Wait();
        return response.Result;
    }
}
