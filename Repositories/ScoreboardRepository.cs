using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;

namespace JuegosDeCartas_OpenCampus.Repositories
{

    public class ScoreboardRepository : IScoreboardRepository
    {
        private readonly List<ScoreBoardPlayer> _entries = new();

        public ScoreboardRepository()
        {
            _entries.Add(new ScoreBoardPlayer { PlayerName = "Carlos",  Game = "BlackJack", Points = 250, Wins = 8, Losses = 3 });
            _entries.Add(new ScoreBoardPlayer { PlayerName = "María",   Game = "Veintiuno", Points = 190, Wins = 6, Losses = 4 });
            _entries.Add(new ScoreBoardPlayer { PlayerName = "José",    Game = "Pitipar",   Points = 160, Wins = 5, Losses = 2 });
            _entries.Add(new ScoreBoardPlayer { PlayerName = "Lucía",   Game = "BlackJack", Points = 140, Wins = 4, Losses = 5 });
            _entries.Add(new ScoreBoardPlayer { PlayerName = "Pedro",   Game = "Veintiuno", Points = 100, Wins = 3, Losses = 6 });
        }

        public IEnumerable<ScoreBoardPlayer> GetTopPlayers(int count = 10)
            => _entries.OrderByDescending(e => e.Points).Take(count);

        public IEnumerable<ScoreBoardPlayer> GetByGame(string gameName)
            => _entries.Where(e => e.Game.Equals(gameName, StringComparison.OrdinalIgnoreCase))
                       .OrderByDescending(e => e.Points);

        public ScoreBoardPlayer? GetByPlayerAndGame(string playerName, string gameName)
            => _entries.FirstOrDefault(e =>
                e.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase) &&
                e.Game.Equals(gameName, StringComparison.OrdinalIgnoreCase));

        public void Save(ScoreBoardPlayer entry)
        {
            if (GetByPlayerAndGame(entry.PlayerName, entry.Game) is null)
                _entries.Add(entry);
        }

        public void Update(ScoreBoardPlayer entry)
        {
            var existing = GetByPlayerAndGame(entry.PlayerName, entry.Game);
            if (existing is not null)
            {
                existing.Points  = entry.Points;
                existing.Wins    = entry.Wins;
                existing.Losses  = entry.Losses;
                existing.Ties    = entry.Ties;
            }
            else
            {
                _entries.Add(entry);
            }
        }
    }
}
