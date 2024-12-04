using System.Drawing.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace final2
{
    public partial class Form1 : Form
    {
        private PrivateFontCollection customFont;
        private static readonly HttpClient client = new HttpClient();
        private List<string> wordList = new List<string>();
        private string final;

        private Button[,] buttons;
        private int curRow = 0;
        private int curCol = 0;

        public Form1()
        {
            InitializeComponent();
            FetchWords();

            customFont = new PrivateFontCollection();
            customFont.AddFontFile("C:/Users/ASUS/Documents/Lessons/3.1/Visual Programming/final2/final2/final2/ARLRDBD.TTF"); // Update to the correct font path

            int rows = 6;
            int cols = 5;
            int buttonSize = 50;
            int margin = 5;

            int totalWidth = cols * buttonSize + (cols - 1) * margin;
            int startX = (this.ClientSize.Width - totalWidth) / 2;

            buttons = new Button[rows, cols];

            Font font = new Font(customFont.Families[0], 16, FontStyle.Bold);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Button button = new Button
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(startX + j * (buttonSize + margin) + margin,
                                             30 + i * (buttonSize + margin) + margin),
                        Text = "",
                        Font = font,
                        TabStop = false
                    };
                    buttons[i, j] = button;
                    this.Controls.Add(button);
                }
            }

            this.KeyPreview = true;
            this.KeyPress += HandleKeyPress;
            this.KeyDown += HandleKeyDown;
        }

        private void HandleKeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsLetter(e.KeyChar))
            {
                if (curCol < 5)
                {
                    buttons[curRow, curCol].Text = e.KeyChar.ToString().ToUpper();
                    curCol++;
                }
            }
            else if (e.KeyChar == (char)Keys.Back)
            {
                if (curCol > 0)
                {
                    curCol--;
                    buttons[curRow, curCol].Text = "";
                }
            }
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string guess = GetGuess();
                if (guess.Length != 5)
                {
                    MessageBox.Show("Hoàn thành từ trước khi submit!");
                    return;
                }

                //if (!IsValid(guess))
                //{
                //    MessageBox.Show("Từ không tồn tại.");
                //    return;
                //}

                if (IsCorrect())
                {
                    ColorButtons(true);
                    MessageBox.Show("Bạn đã trả lời chính xác!");
                }
                else
                {
                    ColorButtons(false);
                    curRow++;
                    curCol = 0;

                    if (curRow >= 6)
                    {
                        MessageBox.Show($"Trò chơi kết thúc! Từ đúng là: {final}.");
                        curRow = 5;
                    }
                }
            }
        }

        //bool IsValid(string word)
        //{
        //    return wordList.Contains(word);
        //}

        private string GetGuess()
        {
            string guess = "";
            for (int i = 0; i < 5; i++)
                guess += buttons[curRow, i].Text.ToUpper();
            return guess;
        }

        private bool IsCorrect()
        {
            string guess = GetGuess();
            if (guess.Length != 5)
            {
                Console.WriteLine("Từ chưa đủ 5 ký tự.");
                return false;
            }
            return guess == final;
        }

        private void ColorButtons(bool correct)
        {
            string guess = GetGuess();
            Console.WriteLine($"G: {guess}, F: {final}");

            if (guess.Length != 5 || final.Length != 5)
            {
                MessageBox.Show("Không đúng độ dài từ.");
                return;
            }

            bool[] guessedCorrectly = new bool[5];
            bool[] letterUsed = new bool[5];

            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == final[i])
                {
                    buttons[curRow, i].BackColor = ColorTranslator.FromHtml("#79b851");
                    buttons[curRow, i].ForeColor = Color.White;
                    guessedCorrectly[i] = true;
                    letterUsed[i] = true;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (!guessedCorrectly[i])
                {
                    bool isYellow = false;
                    for (int j = 0; j < 5; j++)
                    {
                        if (!letterUsed[j] && guess[i] == final[j])
                        {
                            buttons[curRow, i].BackColor = ColorTranslator.FromHtml("#f3c237");
                            buttons[curRow, i].ForeColor = Color.White;
                            letterUsed[j] = true;
                            isYellow = true;
                            break;
                        }
                    }
                    if (!isYellow)
                    { 
                        buttons[curRow, i].BackColor = ColorTranslator.FromHtml("#a4aec4");
                        buttons[curRow, i].ForeColor = Color.White;
                    }
                }
            }
        }

        private async Task FetchWords()
        {
            try
            {
                string apiurl = "https://api.datamuse.com/words?sp=?????";
                HttpResponseMessage response = await client.GetAsync(apiurl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var words = JsonConvert.DeserializeObject<DatamuseResponse[]>(responseBody);

                    foreach (var word in words)
                    {
                        if (word.Word.Length == 5)
                            wordList.Add(word.Word.ToUpper());
                    }

                    if (wordList.Count > 0)
                    {
                        Random rand = new Random();
                        final = wordList[rand.Next(wordList.Count)];
                        Console.WriteLine($"Final: {final}");
                    }
                }
                else
                    Console.WriteLine("Fetch that bai.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public class DatamuseResponse
        {
            [JsonProperty("word")]
            public string Word { get; set; }
        }
    }
}