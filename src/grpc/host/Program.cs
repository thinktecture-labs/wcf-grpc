using ProtoBuf.Grpc.Server;
using Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddCodeFirstGrpc();

var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.Run();