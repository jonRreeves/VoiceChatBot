using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatBot
{
    public partial class StarWarsVisualizer : Form
    {

        string[] commandsFile = (File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\swcommands.txt"));
        string[] responseFile = (File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\swresponses.txt"));

        SpeechSynthesizer speechSynth = new SpeechSynthesizer();

        Choices commandsList = new Choices();
        SpeechRecognitionEngine speechRecognition = new SpeechRecognitionEngine();

        public async void GetCharacterInfo(int id)
        {
            People people = await PeopleProxy.GetPeople(id);
            Planets planets = await PlanetProxy.GetPlanets(people.homeworld);

            string pronoun = "";
            string hairColour = "";

            if(people.gender == "male")
            {
                pronoun = "He";
            }
            else if (people.gender == "n/a")
            {
                pronoun = "This droid";
            }

            if(people.hair_color == "n/a" || people.hair_color == "none")
            {
                hairColour = "no";
            }
            else
            {
                hairColour = people.hair_color;
            }

            Say($"{people.name} has been in {people.films.Length} star wars films." +
                $" {pronoun} has {hairColour} hair and {people.eye_color} eyes. They are from the planet {planets.name}");

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

        public void Say(string text)
        {
            speechSynth.SpeakAsync(text);
            txtOutput.AppendText(text + "\n");
        }

        public StarWarsVisualizer()
        {
            //Initialize Grammar (commands)
            commandsList.Add(commandsFile);
            Grammar grammar = new Grammar(new GrammarBuilder(commandsList));

            try
            {
                speechRecognition.RequestRecognizerUpdate();
                speechRecognition.LoadGrammar(grammar);
                speechRecognition.SpeechRecognized += starWars_SpeechRecognized;
                speechRecognition.SetInputToDefaultAudioDevice();
                speechRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }

            //Custom voice settings
            speechSynth.SelectVoiceByHints(VoiceGender.Female);
            speechSynth.Speak("Hello Jon, star wars menu");

            InitializeComponent();
            lblState.Text = "State: Star Wars Menu";
        }

        public bool listening = true;

        private void starWars_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
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
                        if (result.Contains("m"))
                        {
                            Hide();
                            Visualizer v = new Visualizer();
                            v.Show();
                            v.listening = true;
                            listening = false;
                        }

                    }
                    else
                    {
                        Say(responseFile[response]);
                    }
                }
            }
        }
    }
}

