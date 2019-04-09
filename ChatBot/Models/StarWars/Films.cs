using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot
{
    [DataContract]
    public class Films
    {
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public int episode_id { get; set; }
        [DataMember]
        public string opening_crawl { get; set; }
        [DataMember]
        public string director { get; set; }
        [DataMember]
        public string producer { get; set; }
        [DataMember]
        public string release_date { get; set; }
        [DataMember]
        public string[] characters { get; set; }
        [DataMember]
        public string[] planets { get; set; }
        [DataMember]
        public string[] starships { get; set; }
        [DataMember]
        public string[] vehicles { get; set; }
        [DataMember]
        public string[] species { get; set; }
        [DataMember]
        public string url { get; set; }

    }
}
