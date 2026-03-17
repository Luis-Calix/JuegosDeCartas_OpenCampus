using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegosDeCartas_OpenCampus.Models
{

    public class ScoreBoardPlayer
    {

        public string PlayerName { get; set; } = string.Empty;

        public string Game { get; set; } = string.Empty;

        public int Points { get; set; }

        public int Wins { get; set; }

        public int Losses { get; set; }

        public int Ties { get; set; }

        public double WinRate =>
            (Wins + Losses + Ties) == 0
                ? 0
                : Math.Round((double)Wins / (Wins + Losses + Ties) * 100, 1);


        public ScoreBoardPlayer() { }

        public ScoreBoardPlayer(string playerName, string game)
        {
            PlayerName = playerName;
            Game       = game;
        }

        public void RecordResult(GameResult result)
        {
            switch (result.Result)
            {
                case ResultType.PlayerWin:
                case ResultType.Blackjack:
                    Wins++;
                    Points += result.PointsEarned > 0 ? result.PointsEarned : 10;
                    break;
                case ResultType.DealerWin:
                case ResultType.Bust:
                    Losses++;
                    break;
                case ResultType.Tie:
                    Ties++;
                    Points += 2;
                    break;
            }
        }

        public override string ToString() =>
            $"{PlayerName} | {Game} | {Points} pts | W:{Wins} L:{Losses} T:{Ties}";
    }
}
