using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using APILibrary;

namespace APILibrary
{
    public class WeatherProcessor
    {

        public static async Task<WeatherModel> LoadWeather()
        {
            string url = "https://api.openweathermap.org/data/2.5/weather?q=London,uk";

            using (HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(url))
            {
                WeatherResultModel result = await response.Content.ReadAsAsync<WeatherResultModel>();
                return result.Result;
            }
        }


    }
}
