using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ChatBot
{
    [DataContract]
    public class People
    {
            [DataMember]
            public string name { get; set; }
            [DataMember]
            public string height { get; set; }
            [DataMember]
            public string mass { get; set; }
            [DataMember]
            public string hair_color { get; set; }
            [DataMember]
            public string skin_color { get; set; }
            [DataMember]
            public string eye_color { get; set; }
            [DataMember]
            public string birth_year { get; set; }
            [DataMember]
            public string gender { get; set; }
            [DataMember]
            public string homeworld { get; set; }
            [DataMember]
            public string[] films { get; set; }
            [DataMember]
            public string[] species { get; set; }
            [DataMember]
            public string[] vehicles { get; set; }
            [DataMember]
            public string[] starships { get; set; }
            [DataMember]
            public string url { get; set; }

    }
}
