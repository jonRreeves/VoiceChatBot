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
    public class StarWarsState
    {
        string[] starWarsCommandsFile = (File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\swcommands.txt"));
        string[] starWarsResponseFile = (File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\swresponses.txt"));

        SpeechSynthesizer speechSynth = new SpeechSynthesizer();

        Choices starWarsList = new Choices();
        SpeechRecognitionEngine starWarsRecognition = new SpeechRecognitionEngine();

        public void StarWarsStateInitializer()
        {
            starWarsList.Add(starWarsCommandsFile);
            Grammar starWarsGrammar = new Grammar(new GrammarBuilder(starWarsList));

            try
            {
                starWarsRecognition.RequestRecognizerUpdate();
                starWarsRecognition.LoadGrammar(starWarsGrammar);
                starWarsRecognition.SpeechRecognized += starWars_SpeechRecognized;
                starWarsRecognition.SetInputToDefaultAudioDevice();
                starWarsRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }

            speechSynth.SelectVoice("Microsoft Zira Desktop");
            Say("Star wars menu");
        }

        private void starWars_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            MainState ms = new MainState();

                string result = e.Result.Text;

                //txtInput.AppendText(result + "\n");

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
                    Say("Leaving star wars menu");
                }

            ms.isCurrentState = true;
            ms.MainStateInitializer();
            starWarsRecognition.UnloadAllGrammars();
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

        public void Say(string text)
        {
            speechSynth.SpeakAsync(text);
            //txtOutput.AppendText(text + "\n");
        }
    }
}
