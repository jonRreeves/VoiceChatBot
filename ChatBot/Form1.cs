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

namespace ChatBot
{
    public partial class Form1 : Form
    {
        SpeechSynthesizer s = new SpeechSynthesizer();
        Choices list = new Choices();
        bool wake = true;
        string name = "Jon";
        bool doesLike = true;

        public Form1()
        {
            SpeechRecognitionEngine rec = new SpeechRecognitionEngine();

            list.Add(File.ReadAllLines(@"C:\Users\Jon\Documents\jonvis commands\commands.txt"));

            Grammar gr = new Grammar(new GrammarBuilder(list));

            try
            {
                rec.RequestRecognizerUpdate();
                rec.LoadGrammar(gr);
                rec.SpeechRecognized += rec_SpeechRecognized;
                rec.SetInputToDefaultAudioDevice();
                rec.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }

            s.SelectVoiceByHints(VoiceGender.Female);
            s.Speak("Hello Jon");

            InitializeComponent();
        }

        public void Restart()
        {
            Process.Start(@"C:\Users\Jon\jonvis\ChatBot.exe");
            Environment.Exit(0);
        }

        public void Say(string h)
        {
            s.Speak(h);
            txtOutput.AppendText(h + "\n");
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

        string[] greetings = new string[] { "Hi Jon", "Hello Jon!", "Hey Jon, How are you?" };
        string[] jokes = new string[] { "what do you call 100 niggers in the ground? a fucking miracle",
            "have you ever seen Stevie Wonders wife? No, neither has he" };

        public string Greetings_Action()
        {
            Random r = new Random();

            return greetings[r.Next(3)];
        }

        public string Joke_Action()
        {
            Random r = new Random();
            return jokes[r.Next(2)];
        }

        // commands
        private void rec_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string r = e.Result.Text;

            if (r == "restart" || r == "update")
            {
                Restart();
            }

            if (r == "wake up")
            {
                Say("hello Jon, i am awake");
                wake = true;
                lblState.Text = "State: Awake";
            }
            if (r == "go to sleep")
            {
                Say("goodnight Jon");
                wake = false;
                lblState.Text = "State: Asleep";
            }

            if (wake == true)
            {
                if (r == "hello")
                {
                    Say(Greetings_Action());
                }
                if (r == "how are you")
                {
                    Say("Yeah, you?");
                }
                if (r == "what time is it")
                {
                    Say(DateTime.Now.ToString("h:mm tt"));
                }
                if (r == "what is today")
                {
                    Say(DateTime.Now.ToString("dd/M/yyy"));
                }
                if (r == "open google")
                {
                    Process.Start("https://google.com");
                }
                if (r == "open note pad")
                {
                    Process.Start(@"C:\Program Files\Sublime Text 3\sublime_text.exe");
                }
                if (r == "close note pad")
                {
                    CloseProgram("sublime_text");
                }
                //if(r == "weather report")
                //{
                // Say("The weather is " + GetWeather("cond") + " today");
                //}
                if (r == "spotify")
                {
                    Process.Start(@"C:\Users\Jon\AppData\Roaming\Spotify\Spotify.exe");
                }
                if (r == "play" || r == "pause")
                {
                    SendKeys.Send(" ");
                }
                if (r == "skip")
                {
                    SendKeys.Send("^{RIGHT}");
                }
                if (r == "back")
                {
                    SendKeys.Send("^{LEFT}");
                }
                if (r == "good job")
                {
                    Say("Thats what I'm here for");
                }
                if (r == "tell me a joke")
                {
                    Say(Joke_Action());
                }
                if (r == "whats my name")
                {
                    Say("your name is " + name);
                }
                if (r == "do i like chicken")
                {
                    if (!doesLike)
                    {
                        Say("no, " + name + " you don't like chicken");
                    }
                    if (doesLike)
                    {
                        Say("yes, " + name + " you love chicken");
                    }
                }
                if (r == "shut down")
                {
                    Environment.Exit(0);
                }
                txtInput.AppendText(r + "\n");

            }
        }
    }
}
