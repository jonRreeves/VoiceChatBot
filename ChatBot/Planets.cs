using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot
{
    [DataContract]
    public class Planets
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string rotation_period { get; set; }
        [DataMember]
        public string orbital_period { get; set; }
        [DataMember]
        public string diameter { get; set; }
        [DataMember]
        public string climate { get; set; }
        [DataMember]
        public string gravity { get; set; }
        [DataMember]
        public string terrain { get; set; }
        [DataMember]
        public string surface_water { get; set; }
        [DataMember]
        public string population { get; set; }
        [DataMember]
        public string[] residents { get; set; }
        [DataMember]
        public string[] films { get; set; }
        [DataMember]
        public string url { get; set; }

    }
}
