using Grpc.Core;
using Grpc.Net.Client;
using IdentityModel.Client;
using ProtoBuf.Grpc.Client;
using Service;

Console.WriteLine("Press enter to start");
Console.ReadLine();

# region "Authentication"
var tokenClient = new HttpClient();
var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = "https://localhost:5001/connect/token",
    ClientId = "m2m",
    ClientSecret = "secret",
    Scope = "api"
});

var credentials = CallCredentials.FromInterceptor((_, metadata) =>
{
    metadata.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
    return Task.CompletedTask;
});

# endregion

using var channel = GrpcChannel.ForAddress("https://localhost:7199", new GrpcChannelOptions()
{
    Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
});
var client = channel.CreateGrpcService<IGreeter>();

var reply = client.Greet(new GreetRequest() { FirstName = "John", LastName = "Doe"});

Console.WriteLine($"Greeting: {reply.Response}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();