// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using Service;

using var channel = GrpcChannel.ForAddress("https://localhost:7199");
var client = channel.CreateGrpcService<IGreeter>();

var reply = client.Greet(new GreetRequest() { FirstName = "John", LastName = "Doe"});

Console.WriteLine($"Greeting: {reply.Response}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();