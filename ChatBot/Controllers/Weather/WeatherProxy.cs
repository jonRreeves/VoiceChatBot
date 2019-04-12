using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using static ChatBot.Models.Weather.WeatherModel;

namespace ChatBot.Controllers.Weather
{
    public class WeatherProxy
    {

        public async static Task<RootObject> GetWeatherConnector(string location)
        {
            var apiKey = "de284c8e5c7ce48b07500756e9d1bd22";
            var http = new HttpClient();
            var url = string.Format("https://api.openweathermap.org/data/2.5/weather?q={0},uk&appid={1}", location, apiKey);

            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(RootObject));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (RootObject)serializer.ReadObject(ms);

            return data;

        }

        public async static Task<WeatherHourlyModel.Rootobject> GetHourlyWeatherConnector(string location)
        {
            var apiKey = "de284c8e5c7ce48b07500756e9d1bd22";
            var http = new HttpClient();
            var url = string.Format("https://api.openweathermap.org/data/2.5/forecast/hourly/?q={0},uk&appid={1}", location, apiKey);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(WeatherHourlyModel.Rootobject));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (WeatherHourlyModel.Rootobject)serializer.ReadObject(ms);

            return data;

        }
    }
}
