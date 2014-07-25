using System;
using System.Runtime.Serialization;

namespace CSharpNameParser
{
    [DataContract]
    public class Name : IComparable<Name>
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

        public int CompareTo(Name other)
        {
            // Compare last names.
            var diff = LastName.ToLower().CompareTo(other.LastName.ToLower());
            if (diff != 0) return diff;

            // Compare first names.
            diff = FirstName.ToLower().CompareTo(other.FirstName.ToLower());
            if (diff != 0) return diff;

            // Compare Middle initials
            diff = MiddleInitials.ToLower().CompareTo(other.MiddleInitials.ToLower());
            if (diff != 0) return diff;

            return 0;
        }
    }
}
