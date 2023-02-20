using System.Runtime.Serialization;

namespace Service
{
    [DataContract]
    public class GreeterResponse
    {
        [DataMember(Order = 1)]   
        public string Response { get; set; }
        public override string ToString()
        {
            return Response;
        }
    }
}