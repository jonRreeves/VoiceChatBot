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
using System.Drawing;

namespace ChatBot
{
    // TODO - move states to own classes
    // each class needs to initialise its own grammar file + commands + choices

    public partial class Visualizer : Form
    {

        // used to switch between forms, so that dictionaries (command files) do not overlap
        bool listening = true;

        // when "search for" is said, enters searching state
        bool searching = false;

        bool movie = false;

        bool isSettingUp = false;
        bool isSettingUpName = false;
        bool isSettingUpAge = false;
        bool isSettingUpLocation = false;

        bool hasExplained = false;

        //Commands and responses
        string[] commandsFile = File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\commands.txt");
        string[] responseFile = File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\responses.txt");
        
        // file for common search terms
        string[] searchFile = File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\searchterms.txt");

        // favourite recipes file
        string[] recipesFile = File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\recipes.txt");

        // movies file
        string[] moviesFile = File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\movies.txt");

        // setup file
        string[] setupFile = File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\setup.txt");

        string[] detailsFile = File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\name_age_location.txt");

        string name
        {
            get { return detailsFile[0]; }
            set { detailsFile[0] = value; }
        }

        string age
        {
            get { return detailsFile[1]; }
            set { detailsFile[1] = value; }
        }
        
        string location
        {
            get { return detailsFile[2]; }
            set { detailsFile[2] = value; }
        }

        //Speech Synthesizer
        SpeechSynthesizer speechSynth = new SpeechSynthesizer();

        // initialize basic commands and engine
        Choices commandsList = new Choices();
        SpeechRecognitionEngine speechRecognition = new SpeechRecognitionEngine();

        // different engine for search state, so the grammar files do not get confused
        Choices searchList = new Choices();
        SpeechRecognitionEngine searchRecognition = new SpeechRecognitionEngine();

        // same as above
        Choices movieList = new Choices();
        SpeechRecognitionEngine movieRecognition = new SpeechRecognitionEngine();

        // same as above
        Choices setupList = new Choices();
        SpeechRecognitionEngine setupRecognition = new SpeechRecognitionEngine();


        public Visualizer()
        {

            //Initialize Grammar (commands)
            commandsList.Add(commandsFile);
            Grammar grammar = new Grammar(new GrammarBuilder(commandsList));
            string[] greetings = { $"hello {name}", $"Good day {name}", $"what now {name}?", $"we meet again {name}", $"i've been expecting you {name}",
            $"greetings {name}", $"bonjour {name}", $"hello {name} this is microsoft tech support, are you aware there is a wirus on your computer?"};

            Random r = new Random();

            string greeting;

            greeting = greetings[r.Next(greetings.Length)];

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
            speechSynth.GetInstalledVoices();
            speechSynth.SelectVoice("Microsoft Zira Desktop");
            speechSynth.Speak(greeting);

            InitializeComponent();
        }

        // instantiate search state, new Grammar is needed so terms don't overlap
        public void SearchState()
        {
            searchList.Add(searchFile);
            Grammar searchGrammar = new Grammar(new GrammarBuilder(searchList));

            try
            {
                searchRecognition.RequestRecognizerUpdate();
                searchRecognition.LoadGrammar(searchGrammar);
                searchRecognition.SpeechRecognized += search_SpeechRecognized;
                searchRecognition.SetInputToDefaultAudioDevice();
                searchRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }
        }

        // called when "search for" is said, for more accurate search results
        private void search_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {

            string result = e.Result.Text;
            

            Random r = new Random();

            string randomRecipe = recipesFile[r.Next(recipesFile.Length)];

            if (searching)
            {
                txtInput.AppendText(result + "\n");
                if (result == "random recipe")
                {
                    Say("Finding random recipe");
                    Process.Start(randomRecipe);
                    searching = false;
                }
                else
                {
                    Say("Navigating to: " + result);
                    if(result == "amazon")
                    {
                        Process.Start($"https://{result}.co.uk");
                        Say("Do we really need more clutter?");
                    }
                    else
                    {
                        Process.Start($"https://{result}.com/");
                    }                   
                    searching = false;
                }
            }

            // unload current grammar so it is not available in the main state
            searchRecognition.UnloadAllGrammars();
        }

        public void MovieState()
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
        }

        private void movie_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string result = e.Result.Text;
            
            Random r = new Random();

            // reformat input string so it can be conforms to api query requirements
            string randomMovie = moviesFile[r.Next(moviesFile.Length)].Replace(' ', '_');
            string underscoredString = result.Replace(' ', '_');

            if (movie)
            {
                txtInput.AppendText(result + "\n");
                if (result == "random")
                {
                    Say("Finding random movie");
                    GetMovieByTitle(randomMovie);
                    movie = false;
                    
                }
                else
                {
                    GetMovieByTitle(underscoredString);
                    movie = false;
                }
            }

            movieRecognition.UnloadAllGrammars();
        }

        public void SetupState()
        {
            setupList.Add(setupFile);
            Grammar setupGrammar = new Grammar(new GrammarBuilder(setupList));

            try
            {
                setupRecognition.RequestRecognizerUpdate();
                setupRecognition.LoadGrammar(setupGrammar);
                setupRecognition.SpeechRecognized += setup_SpeechRecognized;
                setupRecognition.SetInputToDefaultAudioDevice();
                setupRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }
        }

        private void setup_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string result = e.Result.Text;

            if (isSettingUp)
            {
                if (isSettingUpName)
                {
                    name = result;                    
                    Say("Name set to: " + name);
                    detailsFile[0] = name;

                    isSettingUpName = false;
                }
                if (isSettingUpAge)
                {
                    age = result;
                    Say("Age set to: " + age);
                    detailsFile[1] = age;
                    isSettingUpAge = false;
                }
                if (isSettingUpLocation)
                {
                    location = result;
                    Say("Location set to: " + location);
                    detailsFile[2] = location;
                    isSettingUpLocation = false;
                }

                File.WriteAllLines(@"C:\Users\Jon\Documents\jonvis commands\name_age_location.txt", detailsFile);
            }

            isSettingUp = false;
       
            setupRecognition.UnloadAllGrammars();
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

        public string Lotto()
        {
            Random r = new Random();
            string numbers = "";

            for (int i = 0; i <= 5; i++)
            {
                numbers += r.Next(1,60) + ", ";
            }

            numbers += r.Next(1, 60) + ". Good Luck";

            return numbers;

        }

        public async void GetWeather(string location)
        {
            // unix time | +1 hour = + 3600 | +1 day = +86400
            //var epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            RootObject weather = await WeatherProxy.GetWeatherConnector(location);
            WeatherHourlyModel.Rootobject hourly = await WeatherProxy.GetHourlyWeatherConnector(location);

            // With highs of {((int)(w.main.temp_max)-273).ToString()}" +
            // $" degrees and lows of {((int)(w.main.temp_min) - 273).ToString()} degrees.

            Say($"It is currently {((int)(weather.main.temp) - 273).ToString()} degrees in {location}. It is forecast to be {weather.weather[0].description}, with {weather.main.humidity} % humidity." +
                $" Wind speeds of {((int)(weather.wind.speed) * 2.23694).ToString("0.0")} miles per hour. In one hour it will be {((int)(hourly.list[1].main.temp) - 273).ToString()}" +
                $" degrees and {hourly.list[1].weather[0].description}. In two hours it will be {((int)(hourly.list[2].main.temp) - 273).ToString()} degrees and" +
                $" {hourly.list[2].weather[0].description}. Tomorrow at this time it is forecast to be {((int)(hourly.list[24].main.temp)-273).ToString()} degrees " +
                $"and {hourly.list[24].weather[0].description}.");

        }

        public async void GetJoke()
        {
            JokeModel joke = await JokeProxy.GetJokeConnector();

            Say(joke.setup + " " + joke.punchline);
        }

        //public async void GetMovie(string filmId)
        //{
        //    MovieModel movie = await MovieProxy.GetMovieConnector(filmId);

        //    Say($"{movie.Title} {movie.Released} directed by {movie.Director}, produced by {movie.Production}. {movie.Plot}. {movie.Awards} {movie.BoxOffice} IMDb Rating: {movie.imdbRating}");
        //}

        public async void GetMovieByTitle(string filmId)
        {
            MovieModel movie = await MovieProxy.GetMovieByTitle(filmId);

            string boxOffice;

            if(movie.BoxOffice == "N/A" || movie.BoxOffice == "n/a")
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

        // Commands
        private void menu_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (listening)
            {              

                if (!searching && !movie && !isSettingUp)
                {
                    string result = e.Result.Text;
                    int response = Array.IndexOf(commandsFile, result);
                    txtInput.AppendText(result + "\n");

                    if (responseFile[response].IndexOf('+') == 0)
                    {
                        List<string> responses = responseFile[response].Replace('+', ' ').Replace("name",name).Split('/').Reverse().ToList();
                        Random r = new Random();
                        Say(responses[r.Next(responses.Count)]);
                    }
                    else
                    {
                        if (responseFile[response].IndexOf('*') == 0)
                        {
                            if (result.Contains("go to"))
                            {
                                Say("Where do you want to go?");
                                searching = true;
                                SearchState();
                            }
                            if (result.Contains("movie"))
                            {
                                Say("What movie would you like me to search for?");
                                movie = true;
                                MovieState();
                            }
                            if (result.Contains("what time is it"))
                            {
                                Say(DateTime.Now.ToString("h:mm tt"));
                            }
                            if (result.Contains("whats the date"))
                            {
                                Say(DateTime.Now.ToString("dddd, dd MMMM yyyy"));
                            }
                            if (result.Contains("open google"))
                            {
                                Say("Opening google");
                                Process.Start("https://google.com");
                            }
                            if (result.Contains("recent"))
                            {
                                SendKeys.Send("^+{T}");
                            }
                            if (result.Contains("tab"))
                            {
                                SendKeys.Send("^w");
                            }
                            if (result.Contains("open note pad"))
                            {
                                Say("Opening Sublime Text");
                                Process.Start(@"C:\Program Files\Sublime Text 3\sublime_text.exe");
                                //isNotepadOpen = true;
                                //Say("Would you like to begin writing?");
                            }
                            if (result.Contains("close note pad"))
                            {
                                Say("Closing Sublime Text");
                                CloseProgram("sublime_text");
                                //isNotepadOpen = false;
                            }
                            if (result.Contains("spotify"))
                            {
                                Say("Opening Spot if eye");
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
                            if (result.Contains("who am i"))
                            {

                                Say($"Your name is {name}. You are {age}. You are from {location}");

                                if (!hasExplained)
                                {
                                    Say("To change your name, say \"change name\". To change your age, say \"change age\". " +
                                        "To change your location, say \"change location\"");
                                    hasExplained = true;
                                }
                            }
                            if (result.Contains("shut down"))
                            {
                                Environment.Exit(0);
                            }
                            if (result.Contains("star wars"))
                            {
                                Hide();
                                StarWarsVisualizer swv = new StarWarsVisualizer();
                                swv.Show();
                                swv.listening = true;
                                listening = false;
                            }
                            if (result.Contains("weather"))
                            {
                                GetWeather(location);
                            }
                            if (result.Contains("tell me a joke"))
                            {
                                GetJoke();
                            }
                            if (result.Contains("lottery"))
                            {
                                Say("Your Numbers Today: " + Lotto());
                            }
                            if (result.Contains("help"))
                            {
                                var message = string.Join("\n", commandsFile);
                                MessageBox.Show("Command list: \n" + message);
                                //txtOutput.AppendText("Command list: \n");
                                //foreach (var line in commandsFile)
                                //{
                                //    txtOutput.AppendText(line + "\n");
                                //}
                            }
                            if (result.Contains("change name"))
                            {
                                Say("What is your name?");
                                isSettingUp = true;
                                isSettingUpName = true;
                                SetupState();
                            }
                            if(result.Contains("change age"))
                            {
                                Say("How old are you?");
                                isSettingUp = true;
                                isSettingUpAge = true;
                                SetupState();
                            }
                            if(result.Contains("change location"))
                            {
                                Say("Where are you from?");
                                isSettingUp = true;
                                isSettingUpLocation = true;
                                SetupState();
                            }

                            // TODO - speech to text / write to notepad
                            //if (isNotepadOpen && result.Contains("yes"))
                            //{

                            //}
                            //if (isNotepadOpen && result.Contains("no"))
                            //{

                            //}
                        }
                        else
                        {
                            Say(responseFile[response]);
                        }
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