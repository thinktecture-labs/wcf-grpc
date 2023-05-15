using Grpc.Core;
using Grpc.Core.Interceptors;

namespace grpc;

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
        _logger.LogDebug("Aufruf der gRPC-Methode {method} mit Argument {argument}", context.Method, request);
        var result = await base.UnaryServerHandler(request, context, continuation);
        _logger.LogDebug("gRPC-Methode {method} mit Ergebnis {result} wurde aufgerufen", context.Method, result);
        return result;
    }
}