using Grpc.Core;
using Grpc.Core.Interceptors;
using ProtoBuf.Grpc.Server;
using Service;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddCodeFirstGrpc(_ => { })
    .AddServiceOptions<GreeterService>(options => options.Interceptors.Add<LoggingInterceptor>());

var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.Run();


public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }
    
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogDebug("Calling gRPC Method {method} with argument {argument}", context.Method, request);
        var result = await base.UnaryServerHandler(request, context, continuation);
        _logger.LogDebug("Called gRPC Method {method} with result {result}", context.Method, result);
        return result;
    }
}