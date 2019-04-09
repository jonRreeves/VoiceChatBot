using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot
{
    public class PlanetProxy
    {
        public async static Task<Planets> GetPlanets (string planetId)
        {
            var http = new HttpClient();
            var url = string.Format(planetId);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(Planets));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (Planets)serializer.ReadObject(ms);

            return data;

        }
    }
}
