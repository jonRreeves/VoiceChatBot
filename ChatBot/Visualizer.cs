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
using ChatBot.Models.Weather;
using ChatBot.Controllers.Weather;
using static ChatBot.Models.Weather.WeatherModel;

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
            RootObject w = await WeatherProxy.GetWeather(location);

            Say($"It is currently {((int)(w.main.temp)-273).ToString()} degrees in {location}. With highs of {((int)(w.main.temp_max)-273).ToString()}" +
                $" degrees and lows of {((int)(w.main.temp_min)-273).ToString()} degrees. It is forecast to be {w.weather[0].description} ");
            
            
        }

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