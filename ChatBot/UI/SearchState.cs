using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.UI
{
    public class SearchState
    {
        Connector connector = new Connector();
        MainState ms = new MainState();

        string[] searchFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\searchterms.txt");
        string[] recipesFile = File.ReadAllLines(@"C:\Users\Jon\source\GitHub\ChatBot\ChatBot\commands and responses\recipes.txt");

        SpeechSynthesizer speechSynth = new SpeechSynthesizer();

        Choices searchList = new Choices();
        SpeechRecognitionEngine searchRecognition = new SpeechRecognitionEngine();

        public void SearchStateIntitializer()
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

            speechSynth.SelectVoice("Microsoft Zira Desktop");
        }

        private void search_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {

            string result = e.Result.Text;
            connector.InputText = result;

            Random r = new Random();

            string randomRecipe = recipesFile[r.Next(recipesFile.Length)];

            if (result == "random recipe")
            {
                Say("Finding random recipe");
                Process.Start(randomRecipe);
            }
            else
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
            }

            ms.isCurrentState = true;
            ms.MainStateInitializer();
            searchRecognition.UnloadAllGrammars();
        }

        public void Say(string text)
        {
            speechSynth.SpeakAsync(text);
            if (text != connector.OutputText)
            {
                connector.OutputText = text;
            }
            
            //txtOutput.AppendText(text + "\n");
        }
    }
}
