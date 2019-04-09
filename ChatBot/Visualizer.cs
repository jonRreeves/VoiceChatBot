using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Diagnostics;
using System.IO;
using ChatBot.Controllers.Weather;
using static ChatBot.Models.Weather.WeatherModel;
using ChatBot.Models;
using ChatBot.Controllers;

namespace ChatBot
{
    // TODO - states (menus) i.e certain actions can only be performed from certain states (custom dictionaries), prevents overlapping of keywords and confusion in one state
    // setupState | keyword: setup | file: profile.txt (names, ages, location) 
    // mainState | keyword: menu | file: commands.txt time/date functionality, open close programs, jokes, recall (name,age), weather report etc. anything that is not specific done
    // starWarsState | keyword: star wars | file: starwars.txt done
    // writingState | keyword: write | file dictionary.txt
    // searchingState | keyword: search | file searchdictionary.txt

    // TODO - move states to own classes
    // each class needs to initialise its own grammar file + commands + choices

    /* TODO - pass input from voice to query movie database
       big dictionary 
       try catch incase of 404
       replace " " in strings with "_"
    */

    // TODO - figure out storing voice input in variables and using them in api queries | maybe use seperate form and listener for search features, so can use input as search term

    public partial class Visualizer : Form
    {

        //Commands and responses
        string[] commandsFile = File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\commands.txt");
        string[] responseFile = File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\responses.txt");

        //Speech Synthesizer
        SpeechSynthesizer speechSynth = new SpeechSynthesizer();

        //Speech Recognition
        Choices commandsList = new Choices();
        SpeechRecognitionEngine speechRecognition = new SpeechRecognitionEngine();

        public Visualizer()
        {

            //Initialize Grammar (commands)
            commandsList.Add(commandsFile);
            Grammar grammar = new Grammar(new GrammarBuilder(commandsList));

            try
            {
                speechRecognition.RequestRecognizerUpdate();
                speechRecognition.LoadGrammar(grammar);
                speechRecognition.SpeechRecognized += menu_SpeechRecognized;
                speechRecognition.SetInputToDefaultAudioDevice();
                speechRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }

            //Custom voice settings
            speechSynth.SelectVoiceByHints(VoiceGender.Female);
            speechSynth.Speak("Hello Jon, main menu");

            InitializeComponent();
            lblState.Text = "State: Main Menu";
        }

        
        // voice output, shows output on gui for debugging
        public void Say(string text)
        {
            speechSynth.SpeakAsync(text);
            txtOutput.AppendText(text + "\n");
        }

        public void CloseProgram(string s)
        {

            Process[] proc = null;

            try
            {
                proc = Process.GetProcessesByName(s);
                Process prog = proc[0];

                if (!prog.HasExited)
                {
                    prog.Kill();
                }
            }
            catch
            {
                Say("notepad is not open");
            }
            finally
            {
                if (proc != null)
                {
                    foreach (Process p in proc)
                    {
                        p.Dispose();
                    }
                }
            }

            proc = null;
        }

        public async void GetWeather(string location)
        {
            // unix time | +1 hour = + 3600 | +1 day = +86400
            var epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            RootObject weather = await WeatherProxy.GetWeatherConnector(location);
            WeatherHourlyModel.Rootobject hourly = await WeatherProxy.GetHourlyWeatherConnector(location);

            // With highs of {((int)(w.main.temp_max)-273).ToString()}" +
            // $" degrees and lows of {((int)(w.main.temp_min) - 273).ToString()} degrees.

            Say($"It is currently {((int)(weather.main.temp) - 273).ToString()} degrees in {location}. It is forecast to be {weather.weather[0].description}, with {weather.main.humidity} % humidity." +
                $" Wind speeds of {((int)(weather.wind.speed) * 2.23694).ToString("0.0")} miles per hour. In one hour it will be {((int)(hourly.list[1].main.temp) - 273).ToString()}" +
                $" degrees and {hourly.list[1].weather[0].description}. In two hours it will be {((int)(hourly.list[2].main.temp) - 273).ToString()} degrees and" +
                $" {hourly.list[2].weather[0].description}. Tomorrow is forecast to be {hourly.list[24].weather[0].description}.");

        }

        public async void GetJoke()
        {
            JokeModel joke = await JokeProxy.GetJokeConnector();

            Say(joke.setup + joke.punchline);
        }

        public async void GetMovie(string filmId)
        {
            MovieModel movie = await MovieProxy.GetMovieConnector(filmId);

            Say($"{movie.Title} {movie.Released} directed by {movie.Director}, produced by {movie.Production}. {movie.Plot}. {movie.Awards}. {movie.imdbRating}");
        }

        public async void GetMovieByTitle(string filmId)
        {
            MovieModel movie = await MovieProxy.GetMovieByTitle(filmId);

            Say($"{movie.Title} {movie.Released} directed by {movie.Director}, produced by {movie.Production}. {movie.Plot}. {movie.Awards}. {movie.imdbRating}");
        }

        // used to switch between forms, so that dictionaries (command files) do not overlap
        public bool listening = true;

        // Commands
        private void menu_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (listening)
            {
                string result = e.Result.Text;
                int response = Array.IndexOf(commandsFile, result);
                txtInput.AppendText(result + "\n");

                if (responseFile[response].IndexOf('+') == 0)
                {
                    List<string> responses = responseFile[response].Replace('+', ' ').Split('/').Reverse().ToList();
                    Random r = new Random();
                    Say(responses[r.Next(responses.Count)]);
                }
                else
                {
                    if (responseFile[response].IndexOf('*') == 0)
                    {
                        if (result.Contains("time"))
                        {
                            Say(DateTime.Now.ToString("h:mm tt"));
                        }
                        if (result.Contains("today"))
                        {
                            Say(DateTime.Now.ToString("dd/MM/yyy"));
                        }
                        if (result.Contains("google"))
                        {
                            Process.Start("https://google.com");
                        }
                        if (result.Contains("open"))
                        {
                            Process.Start(@"C:\Program Files\Sublime Text 3\sublime_text.exe");
                        }
                        if (result.Contains("close"))
                        {
                            CloseProgram("sublime_text");
                        }
                        if (result.Contains("spotify"))
                        {
                            Process.Start(@"C:\Users\Jon\AppData\Roaming\Spotify\Spotify.exe");
                        }
                        if (result.Contains("play") || result.Contains("pause"))
                        {
                            SendKeys.Send(" ");
                        }
                        if (result.Contains("skip"))
                        {
                            SendKeys.Send("^{RIGHT}");
                        }
                        if (result.Contains("back"))
                        {
                            SendKeys.Send("^{LEFT}");
                        }
                        if (result.Contains("name"))
                        {
                            // TODO - update this with name variable, maybe store it to file when you open the program
                            Say("Your name is Jon");
                        }
                        if (result.Contains("shut"))
                        {
                            Environment.Exit(0);
                        }
                        if (result.Contains("star"))
                        {
                            Hide();
                            StarWarsVisualizer swv = new StarWarsVisualizer();
                            swv.Show();
                            swv.listening = true;
                            listening = false;
                        }
                        if (result.Contains("weather"))
                        {
                            GetWeather("bournemouth");
                        }
                        if (result.Contains("joke"))
                        {
                            GetJoke();
                        }
                        if (result.Contains("film"))
                        {
                            GetMovie("tt3896198");                           
                        }
                        if (result.Contains("1"))
                        {
                            GetMovieByTitle("the_matrix");
                        }

                    }
                    else
                    {
                        Say(responseFile[response]);
                    }
                }

            }
        }

        // TODO - Sleep and wake functions, maybe activate only on call like "Alexa" "ok google" style
        //if (r == "wake up")
        //{
        //    Say("hello Jon, i am awake");
        //    wake = true;
        //    lblState.Text = "State: Awake";
        //}
        //if (r == "go to sleep")
        //{
        //    Say("goodnight Jon");
        //    wake = false;
        //    lblState.Text = "State: Asleep";
        //}


    }
}