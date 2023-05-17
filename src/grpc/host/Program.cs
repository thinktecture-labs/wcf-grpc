using grpc;
using ProtoBuf.Grpc.Server;
using Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddCodeFirstGrpc(options =>
    {
        options.Interceptors.Add<LoggingInterceptor>();
        options.Interceptors.Add<UserInterceptor>();
    });

builder.Services
    .AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5001/";
        options.Audience = "api";
    });
builder.Services
    .AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<GreeterService>().RequireAuthorization();
app.Run();