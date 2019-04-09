using ChatBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Controllers
{
    public class JokeProxy
    {

        public async static Task<JokeModel> GetJokeConnector()
        {
            var http = new HttpClient();
            var url = string.Format("https://official-joke-api.appspot.com/random_joke");
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(JokeModel));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (JokeModel)serializer.ReadObject(ms);

            return data;
        }

    }
}
