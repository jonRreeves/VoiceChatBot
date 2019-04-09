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
    public class MovieProxy
    {
        public async static Task<MovieModel> GetMovieConnector(string filmId)
        {
            var apiKey = "4c3b7edc";
            var http = new HttpClient();
            var url = string.Format("http://www.omdbapi.com/?i={0}&apikey=4c3b7edc",filmId);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(MovieModel));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (MovieModel)serializer.ReadObject(ms);

            return data;
        }

        public async static Task<MovieModel> GetMovieByTitle(string filmId)
        {
            var apiKey = "4c3b7edc";
            var http = new HttpClient();
            var url = string.Format("http://www.omdbapi.com/?t={0}&apikey=4c3b7edc", filmId);
            var response = await http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(MovieModel));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (MovieModel)serializer.ReadObject(ms);

            return data;
        }

    }
}
