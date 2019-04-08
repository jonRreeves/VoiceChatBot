using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace ChatBot
{

    public partial class Visualizer : Form
    {

        //Commands and responses
        string[] commandsFile = (File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\commands.txt"));
        string[] responseFile = (File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\responses.txt"));

        //Speech Synthesizer
        SpeechSynthesizer speechSynth = new SpeechSynthesizer();

        //Speech Recognition
        Choices commandsList = new Choices();
        SpeechRecognitionEngine speechRecognition = new SpeechRecognitionEngine();

        //SwAPI commands
        public async void GetCharacterInfo(int id)
        {
            People people = await PeopleProxy.GetPeople(id);
            Planets planets = await PlanetProxy.GetPlanets(people.homeworld);
            Say(people.name + " has been in " + people.films.Length + " star wars films." + " They have " + people.hair_color + " hair and " + people.eye_color + " eyes. "
                + "They are from the planet " + planets.name);
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



        public Visualizer()
        {
            
            //Initialize Grammar (commands)
            commandsList.Add(commandsFile);
            Grammar grammar = new Grammar(new GrammarBuilder(commandsList));

            try
            {
                speechRecognition.RequestRecognizerUpdate();
                speechRecognition.LoadGrammar(grammar);
                speechRecognition.SpeechRecognized += rec_SpeechRecognized;
                speechRecognition.SetInputToDefaultAudioDevice();
                speechRecognition.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }

            //Custom voice settings
            speechSynth.SelectVoiceByHints(VoiceGender.Female);
            speechSynth.Speak("Hello Jon");

            InitializeComponent();
        }

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



        // Commands
        private void rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
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
                        Say(DateTime.Now.ToString("dd/M/yyy"));
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

                }
                else
                {
                    Say(responseFile[response]);
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
