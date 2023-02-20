using ProtoBuf.Grpc.Server;
using Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCodeFirstGrpc();
builder.Services.AddCodeFirstGrpcReflection();

var app = builder.Build();

app.MapGrpcService<GreeterService>();
app.MapCodeFirstGrpcReflectionService();

app.Run();
