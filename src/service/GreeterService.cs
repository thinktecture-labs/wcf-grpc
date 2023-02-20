namespace Service
{
    public class GreeterService : IGreeter
    { 
        public GreeterResponse Greet(GreetRequest request)
        {
            return new GreeterResponse { Response = $"Hello {request.FirstName} {request.LastName}"};
        }
    }
}
