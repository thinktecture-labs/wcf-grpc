using System;
using System.ServiceModel;
using Service;

namespace wcf_host
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var address = new Uri("http://localhost:8080/");

            var host = new ServiceHost(typeof(GreeterService), address);
            host.AddServiceEndpoint(typeof(IGreeter), new BasicHttpBinding (), "GreeterService");

            host.Open();
            Console.WriteLine($"Listing on {address}");
            Console.WriteLine("Press <Enter> to terminate the service.");
            Console.ReadLine();
            host.Close();
        }
    }
}