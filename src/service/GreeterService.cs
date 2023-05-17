using System.Security.Claims;
using ProtoBuf.Grpc;

namespace Service
{
    public class GreeterService : IGreeter
    {
        public GreeterResponse Greet(GreetRequest request, CallContext context)
        {
            var message = $"Hello {request.FirstName} {request.LastName}";
            if (context.ServerCallContext?.UserState.TryGetValue("User", out var user) == true 
                && user is ClaimsPrincipal principal)
            {
                message = $"{message} calling from Client {principal.FindFirst("client_id")?.Value}";
            }
            
            return new GreeterResponse { Response = message};
        }
    }
}
