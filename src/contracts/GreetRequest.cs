using System.Runtime.Serialization;

namespace Service
{
    [DataContract]
    public class GreetRequest
    {
        [DataMember(Order = 1)]
        public string FirstName { get; set; }

        [DataMember(Order = 2)]
        public string LastName { get; set; }
    }
}