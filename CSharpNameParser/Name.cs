using System.Runtime.Serialization;

namespace CSharpNameParser
{
    [DataContract]
    public class Name
    {
        [DataMember]
        public string Salutation { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string MiddleInitials { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Suffix { get; set; }
    }
}
