using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Models
{
    [DataContract]
    public class MovieModel
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Year { get; set; }
        [DataMember]
        public string Rated { get; set; }
        [DataMember]
        public string Released { get; set; }
        [DataMember]
        public string Runtime { get; set; }
        [DataMember]
        public string Genre { get; set; }
        [DataMember]
        public string Director { get; set; }
        [DataMember]
        public string Writer { get; set; }
        [DataMember]
        public string Actors { get; set; }
        [DataMember]
        public string Plot { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string Awards { get; set; }
        [DataMember]
        public string Poster { get; set; }
        [DataMember]
        public Rating[] Ratings { get; set; }
        [DataMember]
        public string Metascore { get; set; }
        [DataMember]
        public string imdbRating { get; set; }
        [DataMember]
        public string imdbVotes { get; set; }
        [DataMember]
        public string imdbID { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string DVD { get; set; }
        [DataMember]
        public string BoxOffice { get; set; }
        [DataMember]
        public string Production { get; set; }
        [DataMember]
        public string Website { get; set; }
        [DataMember]
        public string Response { get; set; }

        [DataContract]
        public class Rating
        {
            [DataMember]
            public string Source { get; set; }
            [DataMember]
            public string Value { get; set; }
        }

    }
}
