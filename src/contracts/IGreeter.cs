using System.ServiceModel;

namespace Service
{
    [ServiceContract]
    public interface IGreeter
    {
        [OperationContract]
        public GreeterResponse Greet(GreetRequest request);
    }
}