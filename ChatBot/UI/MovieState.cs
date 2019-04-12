using ChatBot.Controllers;
using ChatBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.UI
{
    public class MovieState
    {
        string[] moviesFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\movies.txt");

        Choices movieList = new Choices();
        SpeechRecognitionEngine movieRecognition = new SpeechRecognitionEngine();

        SpeechSynthesizer speechSynth = new SpeechSynthesizer();

        public void MovieStateInitializer()
        {
            movieList.Add(moviesFile);
            Grammar movieGrammar = new Grammar(new GrammarBuilder(movieList));

            try
            {
                movieRecognition.RequestRecognizerUpdate();
                movieRecognition.LoadGrammar(movieGrammar);
                movieRecognition.SpeechRecognized += movie_SpeechRecognized;
                movieRecognition.SetInputToDefaultAudioDevice();
                movieRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }

            speechSynth.SelectVoice("Microsoft Zira Desktop");
        }

        private void movie_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            MainState ms = new MainState();

            string result = e.Result.Text;

            Random r = new Random();

            // reformat input string so it can be conforms to api query requirements
            string randomMovie = moviesFile[r.Next(moviesFile.Length)].Replace(' ', '_');
            string underscoredString = result.Replace(' ', '_');

                //txtInput.AppendText(result + "\n");
                if (result == "random")
                {
                    Say("Finding random movie");
                    GetMovieByTitle(randomMovie);     
                }
                else
                {
                    GetMovieByTitle(underscoredString);
                }

            ms.isCurrentState = true;
            ms.MainStateInitializer();
            movieRecognition.UnloadAllGrammars();
        }

        public async void GetMovieByTitle(string filmId)
        {
            MovieModel movie = await MovieProxy.GetMovieByTitle(filmId);

            string boxOffice;

            if (movie.BoxOffice == "N/A" || movie.BoxOffice == "n/a")
            {
                boxOffice = "";

            }
            else
            {
                boxOffice = movie.BoxOffice + " total box office revenue.";
            }

            Say($"{movie.Title} {movie.Released} directed by {movie.Director}, produced by {movie.Production}. {movie.Plot} {movie.Awards}" +
                $" {boxOffice} IMDb Rating: {movie.imdbRating}");
        }

        public void Say(string text)
        {
            speechSynth.SpeakAsync(text);
            //txtOutput.AppendText(text + "\n");
        }
    }
}
