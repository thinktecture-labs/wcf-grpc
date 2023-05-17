using System.ServiceModel;
using ProtoBuf.Grpc;

namespace Service;

[ServiceContract]
public interface IGreeter
{
    [OperationContract]
    public GreeterResponse Greet(GreetRequest request, CallContext context = default);
}