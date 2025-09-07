using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics; //allows open apps
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
namespace SpeechRecognitionApp
{
    public partial class FormTyping : Form
    {
        // Use environment variable for API key
        private string openAIapiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        private string currentUsername;
        // private bool firstTimeTyping = true; //detects if user is typing for first time
        private bool side;
        private int numArgs;
        private string topic;
        private bool userFirst;

        private int currRound = 0; //counts #rounds (arguments made)
        private bool waiting4User = true; //true if waiting for user to type
        // false if debatemate is formulating response

        public FormTyping()
        {
            InitializeComponent();
            currentUsername = "guest";
        }

        public FormTyping(string username, string topic, bool side, int numArgs, bool userFirst)
        {
            InitializeComponent();
            currentUsername = username;
            this.topic = topic;
            this.side = side;
            this.numArgs = numArgs;
            this.userFirst = userFirst;
            //styling 
        }

        private void FormTyping_Load(object sender, EventArgs e) //like the main
        {
            // sets full screen
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;

            string path = Path.Combine(Application.StartupPath, $"chat-{currentUsername}.txt");
            //guna2Button1.BorderRadius = 25;

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    richTypeAnswerTextBox.Text+=(line);
                }
            }
            else
            {
                richTypeAnswerTextBox.Text+=("No previous chat to show yet. Start typing!");
            }


            //styling 
            guna2SubmitButton.FillColor = Color.FromArgb(88, 101, 242);
            guna2SubmitButton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            guna2SubmitButton.ForeColor = Color.White;
            guna2SubmitButton.BorderRadius = 10;

            guna2ClearButton.FillColor = Color.Gray;
            guna2ClearButton.BorderRadius = 10;
            guna2ClearButton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            guna2ClearButton.ForeColor = Color.White;
            guna2ExitButton.FillColor = Color.Firebrick;
            guna2ExitButton.BorderRadius = 10;
            guna2ExitButton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            guna2ExitButton.ForeColor = Color.White;

            richTypeAnswerTextBox.WordWrap = true;
            richTypeAnswerTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTypeAnswerTextBox.ReadOnly = true; // Prevent editing
            richTypeAnswerTextBox.Font = new Font("Segoe UI", 11); // Clean UI font

            guna2TextBox.Multiline = true;
            guna2TextBox.ScrollBars = ScrollBars.Vertical; // Only vertical scroll
            guna2TextBox.WordWrap = true;
            guna2TextBox.AcceptsReturn = true; // Allows Enter key to insert new lines
            guna2TextBox.AcceptsTab = true;    // Optional: allows tabbing within the box


            // displays topic 
            richTypeAnswerTextBox.Text += Environment.NewLine;
            richTypeAnswerTextBox.Text += Environment.NewLine;
            string intro = $"Welcome to DebateMate! You are debating the topic: {topic}. You are arguing on the {(side ? "FOR" : "AGAINST")} side. You have {numArgs} arguments to make. Good luck!";
            richTypeAnswerTextBox.Text += "----------------------------------------" + Environment.NewLine;
            richTypeAnswerTextBox.Text += ("DebateMate: " + intro);
            richTypeAnswerTextBox.Text += "----------------------------------------" + Environment.NewLine;


            waiting4User = userFirst; //depends if user wants to go firt
            if (!userFirst)
            {
                ShowBotArgument(); //only if user is not going first
            }
        }


        private void TypeAnswerListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void guna2TextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2ExitButton_Click(object sender, EventArgs e)
        {
            this.Close(); //exits the form
            Application.Exit();
        }

        private void guna2ClearButton_Click(object sender, EventArgs e)
        {
            if (currRound < numArgs)
            {
                MessageBox.Show("You cannot clear the chat until the debate is over.");
                return;
            }
            richTypeAnswerTextBox.Clear();
            string path = Path.Combine(Application.StartupPath, $"chat-{currentUsername}.txt");
            //creates new file if it does not exist, appends if it does
            //stored inside: Visual Studio project’s /bin/Debug/ 
            File.WriteAllText(path, string.Empty);

        }

        //only used for taking in users input and displaying it
        private async void guna2SubmitButton_Click(object sender, EventArgs e)
        {
            if (currRound >= numArgs)
            {
                MessageBox.Show("The debate has finished");
                return;
            }
            string userInput = guna2TextBox.Text.Trim();
            //if user enters nothing
            if (string.IsNullOrEmpty(userInput))
            {
                MessageBox.Show("Please enter an argument before submitting.");
                return;
            }

            //adds user input to listbox
            richTypeAnswerTextBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
            richTypeAnswerTextBox.SelectionColor = Color.SteelBlue;
            richTypeAnswerTextBox.AppendText($"You: {userInput}\n\n");
            guna2TextBox.Clear();
            waiting4User = false; //user has submitted their argument

            //calling api
            //string botResponse =await getOpenAIResponse(userInput);
            //TypeAnswerListBox.Items.Add("DebateMate: " + botResponse);
            ShowBotArgument();
            guna2TextBox.Enabled = waiting4User;
        }

        private async void ShowBotArgument()
        {
            if (currRound >= numArgs)
            {
                MessageBox.Show("You have reached the maximum number of arguments.");
                return;
            }

            await Task.Delay(100);// simualte delay in thinking


            // Get user's last argument from the ListBox
            string userLastInput = "";
            string[] lines = richTypeAnswerTextBox.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].StartsWith("You: "))
                {
                    userLastInput = lines[i].Substring(5); // Remove "You: "
                    break;
                }
            }


            string prompt =
                $"You are DebateMate, a skilled debater arguing on the {(side ? "FOR" : "AGAINST")} side of the topic: \"{topic}\".\n" +
                $"The user's most recent argument was:\n\"{userLastInput}\"\n\n" +
                $"First, provide a direct rebuttal to that argument.\n" +
                $"Then, continue by providing your own argument for round #{currRound + 1}.\n" +
                $"Keep your tone respectful and logical.";


            string botResponse = await getOpenAIResponse(prompt);
            richTypeAnswerTextBox.SelectionFont = new Font("Segoe UI", 10, FontStyle.Italic);
            richTypeAnswerTextBox.SelectionColor = Color.DarkGreen;
            richTypeAnswerTextBox.AppendText($"DebateMate: {botResponse}\n\n");
            richTypeAnswerTextBox.Text += "----------------------------------------" + Environment.NewLine;

            //logs the conversation to a file
            string path = Path.Combine(Application.StartupPath, $"chat-{currentUsername}.txt");
            File.AppendAllText(path, $"DebateMate: {botResponse}{Environment.NewLine}");

            currRound++;
            waiting4User = true;
            guna2TextBox.Enabled = waiting4User;
        }

        private void guna2InstructionsTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void guna2EvaluateButton_Click(object sender, EventArgs e)
        {
            if (currRound < numArgs)
            {
                MessageBox.Show("You cannot evaluate the debate until it is over.");
                return;
            }
        }

        private async Task<string> getOpenAIResponse(string userPrompt)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAIapiKey);

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                new { role = "system", content = "You are DebateMate, a helpful debate assistant." },
                new { role = "user", content = userPrompt }
            },
                    max_tokens = 512,
                    temperature = 0.7
                };

                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Error: " + response.StatusCode + "\n" + responseString);
                    return "[API call failed]";
                }

                JObject json = JObject.Parse(responseString);
                return json["choices"]?[0]?["message"]?["content"]?.ToString().Trim() ?? "[No response]";
            }
        }

        private void richTypeAnswerTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }

}
