using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot
{
    public class FilmsProxy
    {
        public async static Task<Films> GetFilms(int id)
        {
            var http = new HttpClient();
            var url = string.Format("https://swapi.co/api/films/{0}/", id);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(Films));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (Films)serializer.ReadObject(ms);

            return data;

        }


    }
}
