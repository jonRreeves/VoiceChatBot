using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models
{ 
    [DataContract]
    public class JokeModel
    {
            [DataMember]
            public int id { get; set; }
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public string setup { get; set; }
            [DataMember]
            public string punchline { get; set; }
    }
}
