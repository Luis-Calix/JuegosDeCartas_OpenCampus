using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegosDeCartas_OpenCampus.Models
{
    public enum ResultType
    {
        PlayerWin,
        DealerWin,
        Tie,
        Blackjack,
        Bust
    }

    public class GameResult
    {
        public ResultType Result { get; set; }

        public string WinnerName { get; set; } = string.Empty;

        public int PlayerScore { get; set; }

        public int DealerScore { get; set; }

        public string Message { get; set; } = string.Empty;

        public int PointsEarned { get; set; }

        public string GameName { get; set; } = string.Empty;

        public DateTime PlayedAt { get; set; } = DateTime.Now;

        public GameResult() { }

        public GameResult(ResultType result, string winnerName,
                          int playerScore, int dealerScore,
                          string gameName, int pointsEarned = 0)
        {
            Result       = result;
            WinnerName   = winnerName;
            PlayerScore  = playerScore;
            DealerScore  = dealerScore;
            GameName     = gameName;
            PointsEarned = pointsEarned;
            Message      = BuildMessage();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string BuildMessage() => Result switch
        {
            ResultType.Blackjack  => $"🃏 ¡BLACKJACK! {WinnerName} gana con 21 natural.",
            ResultType.PlayerWin  => $"🏆 ¡{WinnerName} gana! ({PlayerScore} vs {DealerScore})",
            ResultType.DealerWin  => $"💀 El dealer gana. ({DealerScore} vs {PlayerScore})",
            ResultType.Tie        => $"🤝 Empate. Ambos con {PlayerScore} puntos.",
            ResultType.Bust       => $"💥 ¡{WinnerName} se pasa de 21! Pierde la ronda.",
            _                     => "Fin de ronda."
        };

        public override string ToString() => Message;
    }
}
