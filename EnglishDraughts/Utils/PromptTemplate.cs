using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishDraughts.Utils
{
    public class PromptTemplate
    {
        public string SystemMessage { get; set; }
        public string UserPrompt { get; set; }

        public string RenderPrompt(string boardText, int currentPlayer)
        {
            string player = currentPlayer == 1 ? "White (1)" : "Black (2)";
            return UserPrompt
                .Replace("{{board}}", boardText)
                .Replace("{{player}}", player);
        }
    }
}

