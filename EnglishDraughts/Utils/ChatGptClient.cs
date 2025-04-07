using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnglishDraughts.Utils
{
    public class ChatGptClient
    {
        private const int MaxAttempt = 10;

        private string ChatGptUser = "chatGptUser";
        private readonly HttpClient httpClient;

        private readonly PromptTemplate promptTemplate = new PromptTemplate
        {
            SystemMessage = "You are a professional English Draughts assistant. Help the user choose a legal and optimal move based on the board state. " +
                            "Moves are only allowed on dark squares. Jumps are mandatory. Kings are labeled with 'D1' or 'D2' depended of player. Respond only with one move.",
            UserPrompt =
                "This is the current 8x8 board state (0 = empty, 1 = white, 2 = black):\n{{board}}\n" +
                "The current player is {{player}}.\n" +
                "Please give the best legal move in the format:\n" +
                "\"go from (row1, col1) to (row2, col2)\"\n" +
                "Use numbers 1-8 for rows and A-H for columns."
        };

        public ChatGptClient()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChatGptUser);
        }

        public async Task<string> AskForHintAsync(int[,] board, int currentPlayer, Button[,] buttons)
        {
            string boardText = ConvertBoardToText(board, buttons);
            string userPrompt = promptTemplate.RenderPrompt(boardText, currentPlayer);
            string systemPrompt = promptTemplate.SystemMessage;

            for (int attempt = 1; attempt <= MaxAttempt; attempt++)
            {
                var response = await CallChatGpt(systemPrompt, userPrompt);
                if (IsValidResponse(response, board, currentPlayer, buttons))
                    return response.Trim();
            }

            return $"No valid move found after {MaxAttempt} attempts.";
        }

        private async Task<string> CallChatGpt(string systemPrompt, string userPrompt)
        {
            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 150,
                temperature = 0.7
            };

            string json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            string result = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(result);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString();
        }
        private bool IsValidResponse(string response, int[,] board, int currentPlayer, Button[,] buttons)
        {
            // Regex pattern: go from (2, B) to (3, C)
            var match = Regex.Match(response, @"go from \((\d),\s*([A-Ha-h])\) to \((\d),\s*([A-Ha-h])\)");
            if (!match.Success)
                return false;

            try
            {
                // Parse positions
                int fromRow = int.Parse(match.Groups[1].Value) - 1; // 0-indexed
                int fromCol = match.Groups[2].Value.ToUpper()[0] - 'A';
                int toRow = int.Parse(match.Groups[3].Value) - 1;
                int toCol = match.Groups[4].Value.ToUpper()[0] - 'A';

                // Provera granica
                if (!IsInside(fromRow, fromCol) || !IsInside(toRow, toCol))
                    return false;

                // On start there must be current player
                if (board[fromRow, fromCol] != currentPlayer)
                    return false;

                // On the end it should be empty field
                if (board[toRow, toCol] != 0)
                    return false;

                // Must be diagonal move
                int dRow = Math.Abs(toRow - fromRow);
                int dCol = Math.Abs(toCol - fromCol);

                if (dRow != dCol)
                    return false;

                if (dRow == 1 || dRow == 2)
                {
                    // If it is jump, there must be opponent figure
                    if (dRow == 2)
                    {
                        int midRow = (fromRow + toRow) / 2;
                        int midCol = (fromCol + toCol) / 2;
                        int opponent = currentPlayer == 1 ? 2 : 1;
                        if (board[midRow, midCol] != opponent)
                            return false;
                    }

                    // Check for direction
                    bool isDraught = buttons[fromRow, fromCol].Text == "D";

                    if (!isDraught)
                    {
                        if (currentPlayer == 1 && toRow < fromRow) return false;
                        if (currentPlayer == 2 && toRow > fromRow) return false;
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private string ConvertBoardToText(int[,] board, Button[,] buttons)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    int val = board[i, j];

                    if (buttons[i, j].Text == "D")
                    {
                        sb.Append($"D{val}");
                    }
                    else
                    {
                        sb.Append(val);
                    }

                    if (j < board.GetLength(1) - 1)
                        sb.Append(" ");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }


        private bool IsInside(int i, int j)
        {
            return i >= 0 && i < 8 && j >= 0 && j < 8;
        }

    }
}

