using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Service;

namespace wcf_client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var client = new GreeterClient(new BasicHttpBinding(), new EndpointAddress("http://localhost:8080/GreeterService"));
            var result = client.Greet(new GreetRequest() { FirstName = "John", LastName = "Doe" });
            Console.WriteLine(result);
        }
    }

    public class GreeterClient : ClientBase<IGreeter>, IGreeter
    {
        public GreeterClient(Binding binding, EndpointAddress address): base(binding, address)
        {
            
        }
        
        public GreeterResponse Greet(GreetRequest request)
        {
            return Channel.Greet(request);
        }
    }
}