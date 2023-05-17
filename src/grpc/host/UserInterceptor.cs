using Grpc.Core;
using Grpc.Core.Interceptors;

namespace grpc;

public class UserInterceptor: Interceptor
{
    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var httpContext = context.GetHttpContext();
        var user = httpContext.User;
        context.UserState["User"] = user;
        
        return base.UnaryServerHandler(request, context, continuation);
    }
}