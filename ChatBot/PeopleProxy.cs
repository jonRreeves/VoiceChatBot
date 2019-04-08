using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;

namespace ChatBot
{
    public class PeopleProxy
    {
        public async static Task<People> GetPeople(int id)
        {
            var http = new HttpClient();
            var url = string.Format("https://swapi.co/api/people/{0}/",id);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(People));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (People)serializer.ReadObject(ms);

            return data;

        }

        public async static Task<People> GetPeople(string planetName)
        {
            var http = new HttpClient();
            var url = string.Format(planetName);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(People));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (People)serializer.ReadObject(ms);

            return data;

        }


    }
}
