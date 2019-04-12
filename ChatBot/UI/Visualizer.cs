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
using NAudio.CoreAudioApi;

namespace ChatBot
{
    // TODO - move states to own classes
    // each class needs to initialise its own grammar file + commands + choices

    public partial class Visualizer : Form
    {

        bool listening = true;

        // when "search for" is said, enters searching state
        bool searching = false;

        bool movie = false;

        bool starWars = false;

        bool isSettingUp = false;
        bool isSettingUpName = false;
        bool isSettingUpAge = false;
        bool isSettingUpLocation = false;

        //bool hasExplained = false;

        //Commands and responses
        string[] commandsFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\commands.txt");
        string[] responseFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\responses.txt");

        // file for common search terms
        string[] searchFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\searchterms.txt");

        // favourite recipes file
        string[] recipesFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\recipes.txt");

        // movies file
        string[] moviesFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\movies.txt");

        string[] starWarsCommandsFile = (File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\swcommands.txt"));
        string[] starWarsResponseFile = (File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\swresponses.txt"));

        // setup file
        string[] setupFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\setup.txt");

        string[] detailsFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\name_age_location.txt");

        string FirstName
        {
            get { return detailsFile[0]; }
            set { detailsFile[0] = value; }
        }

        string Age
        {
            get { return detailsFile[1]; }
            set { detailsFile[1] = value; }
        }

        string UserLocation
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

        Choices starWarsCommandsList = new Choices();
        SpeechRecognitionEngine starWarsRecognition = new SpeechRecognitionEngine();

        // same as above
        Choices setupList = new Choices();
        SpeechRecognitionEngine setupRecognition = new SpeechRecognitionEngine();


        public Visualizer()
        {
            //Initialize Grammar (commands)
            commandsList.Add(commandsFile);
            Grammar grammar = new Grammar(new GrammarBuilder(commandsList));
            string[] greetings = { $"hello {FirstName}", $"Good day {FirstName}", $"what now {FirstName}?", $"we meet again {FirstName}", $"i've been expecting you {FirstName}",
            $"greetings {FirstName}", $"bonjour {FirstName}", $"hello {FirstName} this is microsoft tech support, are you aware there is a wirus on your computer?"};

            Random r = new Random();

            string greeting;

            greeting = greetings[r.Next(greetings.Length)];

            try
            {
                speechRecognition.RequestRecognizerUpdate();
                speechRecognition.LoadGrammar(grammar);
                speechRecognition.SpeechRecognized += Menu_SpeechRecognized;
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
                searchRecognition.SpeechRecognized += Search_SpeechRecognized;
                searchRecognition.SetInputToDefaultAudioDevice();
                searchRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }
        }

        // called when "search for" is said, for more accurate search results
        private void Search_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {

            string result = e.Result.Text;


            Random r = new Random();

            string randomRecipe = recipesFile[r.Next(recipesFile.Length)];

            if (searching)
            {
                txtInput.AppendText(char.ToUpper(result[0]) + result.Substring(1) + "\n");
                if (result.Contains("help"))
                {
                    var message = "To Navigate to a Website, Say the name of the website e.g. youtube, reddit. \nTo Retrieve a Random Recipe, Say Random Recipe";
                    MessageBox.Show("Command list: \n" + message);
                }
                if (result == "random recipe")
                {
                    Say("Finding random recipe");
                    Process.Start(randomRecipe);
                    searching = false;
                    searchRecognition.UnloadAllGrammars();
                }
                if (result == "menu")
                {
                    Say("Returning to Main Menu");
                    searching = false;
                    searchRecognition.UnloadAllGrammars();
                }
                else if(result != "help" && result != "random recipe" && result != "menu")
                {
                    Say("Navigating to: " + result);
                    if (result == "amazon")
                    {
                        Process.Start($"https://{result}.co.uk");
                        Say("Do we really need more clutter?");
                    }
                    else
                    {
                        Process.Start($"https://{result}.com/");
                    }
                    searching = false;
                    searchRecognition.UnloadAllGrammars();
                }
            }

            // unload current grammar so it is not available in the main state
            
        }

        public void MovieState()
        {
            movieList.Add(moviesFile);
            Grammar movieGrammar = new Grammar(new GrammarBuilder(movieList));

            try
            {
                movieRecognition.RequestRecognizerUpdate();
                movieRecognition.LoadGrammar(movieGrammar);
                movieRecognition.SpeechRecognized += Movie_SpeechRecognized;
                movieRecognition.SetInputToDefaultAudioDevice();
                movieRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }
        }

        private void Movie_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string result = e.Result.Text;

            Random r = new Random();

            // reformat input string so it can be conforms to api query requirements
            string randomMovie = moviesFile[r.Next(moviesFile.Length)].Replace(' ', '_');
            string underscoredString = result.Replace(' ', '_');

            if (movie)
            {
                txtInput.AppendText(char.ToUpper(result[0]) + result.Substring(1) + "\n");
                if (result.Contains("help"))
                {
                    var message = "Say the name of a IMDb Top 250 Movie, or Say Random";
                    MessageBox.Show("Command list: \n" + message);
                }
                if (result == "random")
                {
                    Say("Finding random movie");
                    GetMovieByTitle(randomMovie);
                    movie = false;
                    movieRecognition.UnloadAllGrammars();

                }
                if (result == "menu")
                {
                    Say("Returning to Main Menu");
                    movie = false;
                    movieRecognition.UnloadAllGrammars();
                }
                else if (result != "help" && result != "random" && result!= "menu")
                {
                    GetMovieByTitle(underscoredString);
                    movie = false;
                    movieRecognition.UnloadAllGrammars();
                }
            }


        }

        public void StarWarsState()
        {
            //Initialize Grammar (commands)
            starWarsCommandsList.Add(starWarsCommandsFile);
            Grammar grammar = new Grammar(new GrammarBuilder(starWarsCommandsList));

            try
            {
                starWarsRecognition.RequestRecognizerUpdate();
                starWarsRecognition.LoadGrammar(grammar);
                starWarsRecognition.SpeechRecognized += StarWars_SpeechRecognized;
                starWarsRecognition.SetInputToDefaultAudioDevice();
                starWarsRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }

        }

        private void StarWars_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (starWars)
            {
                string result = e.Result.Text;
                int response = Array.IndexOf(starWarsCommandsFile, result);
                txtInput.AppendText(char.ToUpper(result[0]) + result.Substring(1) + "\n");

                if (starWarsResponseFile[response].IndexOf('*') == 0)
                {
                    if (result.Contains("help"))
                    {
                        var message = "Say the name of a character, planet or movie title";
                        MessageBox.Show("Command list: \n" + message);
                    }
                    if (result.Contains("luke"))
                    {
                        GetCharacterInfo(1);
                    }
                    if (result.Contains("r"))
                    {
                        GetCharacterInfo(3);
                    }
                    if (result.Contains("tatooine"))
                    {
                        GetPlanetInfo("https://swapi.co/api/planets/1/");
                    }
                    if (result.Contains("naboo"))
                    {
                        GetPlanetInfo("https://swapi.co/api/planets/8/");
                    }
                    if (result.Contains("new"))
                    {
                        GetFilmInfo(1);
                    }
                    if (result.Contains("empire"))
                    {
                        GetFilmInfo(2);
                    }
                    if (result.Contains("jedi"))
                    {
                        GetFilmInfo(3);
                    }
                    if (result.Contains("menace"))
                    {
                        GetFilmInfo(4);
                    }
                    if (result.Contains("clones"))
                    {
                        GetFilmInfo(5);
                    }
                    if (result.Contains("sith"))
                    {
                        GetFilmInfo(6);
                    }
                    if (result.Contains("force"))
                    {
                        GetFilmInfo(7);
                    }
                    if (result.Contains("menu"))
                    {
                        Say("Returning to Main Menu");
                        starWars = false;
                        movieRecognition.UnloadAllGrammars();
                    }
                }

            }
        }

        public async void GetCharacterInfo(int id)
        {
            People people = await PeopleProxy.GetPeople(id);
            Planets planets = await PlanetProxy.GetPlanets(people.homeworld);

            string pronoun = "";
            string hairColour = "";

            if (people.gender == "male")
            {
                pronoun = "He";
            }
            else if (people.gender == "female")
            {
                pronoun = "She";
            }
            else if (people.gender == "n/a")
            {
                pronoun = "This droid";
            }

            if (people.hair_color == "n/a" || people.hair_color == "none")
            {
                hairColour = "no";
            }
            else
            {
                hairColour = people.hair_color;
            }

            Say($"{people.name} has been in {people.films.Length} star wars films." +
                $" {pronoun} has {hairColour} hair and {people.eye_color} eyes. {pronoun} is from the planet {planets.name}");

        }

        public async void GetPlanetInfo(string planetName)
        {
            Planets planets = await PlanetProxy.GetPlanets(planetName);

            string names = "";

            for (int i = 0; i < planets.residents.Length; i++)
            {
                var characters = await PeopleProxy.GetPeople(planets.residents[i]);
                if (i != planets.residents.Length - 1)
                {
                    names += characters.name + ", ";
                }
                else
                {
                    names += " and " + characters.name + ".";
                }
            }

            Say("The planet " + planets.name + " is a " + planets.terrain + " type planet. The climate is " + planets.climate + ".  It has a population of " + planets.population
                + ". It is home to the characters " + names);
        }

        public async void GetFilmInfo(int id)
        {
            Films films = await FilmsProxy.GetFilms(id);

            Say(films.title + ", episode " + films.episode_id + ". " + films.opening_crawl.Replace("\r\n", " "));
        }


        public void SetupState()
        {
            setupList.Add(setupFile);
            Grammar setupGrammar = new Grammar(new GrammarBuilder(setupList));

            try
            {
                setupRecognition.RequestRecognizerUpdate();
                setupRecognition.LoadGrammar(setupGrammar);
                setupRecognition.SpeechRecognized += Setup_SpeechRecognized;
                setupRecognition.SetInputToDefaultAudioDevice();
                setupRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }
        }

        private void Setup_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string result = e.Result.Text;

            if (isSettingUp)
            {
                if (isSettingUpName)
                {
                    Name = result;
                    Say("Name set to: " + Name);
                    detailsFile[0] = Name;

                    isSettingUpName = false;
                }
                if (isSettingUpAge)
                {
                    Age = result;
                    Say("Age set to: " + Age);
                    detailsFile[1] = Age;
                    isSettingUpAge = false;
                }
                if (isSettingUpLocation)
                {
                    UserLocation = result;
                    Say("Location set to: " + UserLocation);
                    detailsFile[2] = UserLocation;
                    isSettingUpLocation = false;
                }

                File.WriteAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\name_age_location.txt", detailsFile);
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
                Say("Notepad is not open");
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
                numbers += r.Next(1, 60) + ", ";
            }

            numbers += r.Next(1, 60) + ", Good Luck";

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

            Say($"It is currently {((int)(weather.main.temp) - 273).ToString()} degrees in {UserLocation}. It is forecast to be {weather.weather[0].description}, with {weather.main.humidity} % humidity." +
                $" Wind speeds of {((int)(weather.wind.speed) * 2.23694).ToString("0.0")} miles per hour. In one hour it will be {((int)(hourly.list[1].main.temp) - 273).ToString()}" +
                $" degrees and {hourly.list[1].weather[0].description}. In two hours it will be {((int)(hourly.list[2].main.temp) - 273).ToString()} degrees and" +
                $" {hourly.list[2].weather[0].description}. Tomorrow at this time it is forecast to be {((int)(hourly.list[24].main.temp) - 273).ToString()} degrees " +
                $"and {hourly.list[24].weather[0].description}.");

        }

        public async void GetJoke()
        {
            JokeModel joke = await JokeProxy.GetJokeConnector();

            Say(joke.setup + " " + joke.punchline);
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

        // Commands
        private void Menu_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string result = e.Result.Text;
            int response = Array.IndexOf(commandsFile, result);
           
            if(result.Contains("wake up"))
            {
                listening = true;
                Say("Ready to comply");
            }

            if (listening && !searching && !movie && !starWars && !isSettingUp)
            {
                lblState.BackColor = Color.Lime;
                lblState.Text = "Listening";
                txtInput.AppendText(char.ToUpper(result[0]) + result.Substring(1) + "\n");

                if (responseFile[response].IndexOf('+') == 0)
                {
                    List<string> responses = responseFile[response].Replace('+', ' ').Replace("name", FirstName).Split('/').Reverse().ToList();
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
                        }
                        if (result.Contains("close note pad"))
                        {
                            Say("Closing Sublime Text");
                            CloseProgram("sublime_text");
                        }
                        if (result.Contains("spotify"))
                        {
                            Say("Opening Spotify");
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

                            Say($"Your name is {FirstName}. You are {Age}. You are from {UserLocation}");
                            //if (!hasExplained)
                            //{
                            //    Say("To change your name, say \"change name\". To change your age, say \"change age\". " +
                            //        "To change your location, say \"change location\"");
                            //    hasExplained = true;
                            //}
                        }
                        if (result.Contains("shut down"))
                        {
                            Environment.Exit(0);
                        }
                        if (result.Contains("star wars"))
                        {
                            Say("What would you like to know about Star Wars?");
                            starWars = true;
                            StarWarsState();
                        }
                        if (result.Contains("weather"))
                        {
                            GetWeather(UserLocation);
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
                        }
                        if (result.Contains("change name"))
                        {
                            Say("What is your name?");
                            isSettingUp = true;
                            isSettingUpName = true;
                            SetupState();
                        }
                        if (result.Contains("change age"))
                        {
                            Say("How old are you?");
                            isSettingUp = true;
                            isSettingUpAge = true;
                            SetupState();
                        }
                        if (result.Contains("change location"))
                        {
                            Say("Where are you from?");
                            isSettingUp = true;
                            isSettingUpLocation = true;
                            SetupState();
                        }
                        if (result.Contains("sleep"))
                        {
                            listening = false;
                            lblState.BackColor = Color.Red;
                            lblState.Text = "Not Listening";
                            Say("Yawn, ok");
                        }
                    }
                    else
                    {
                        Say(responseFile[response]);
                    }
                }
            }
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Say \"help\" for a full list of commands");
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            MMDeviceEnumerator de = new MMDeviceEnumerator();
            MMDevice inputDevice = de.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            MMDevice outputDevice = de.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            float inputVolume = inputDevice.AudioMeterInformation.MasterPeakValue * 100;
            float outputVolume = outputDevice.AudioMeterInformation.MasterPeakValue * 100;
            pbInput.Value = (int)inputVolume;
            pbOutput.Value = (int)outputVolume;

        }
    }
}