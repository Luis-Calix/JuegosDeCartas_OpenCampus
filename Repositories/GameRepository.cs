using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;

namespace JuegosDeCartas_OpenCampus.Repositories
{

    public class GameRepository : IGameRepository
    {
        private readonly List<GameResult> _results = new();

        public void SaveResult(GameResult result)
            => _results.Add(result);

        public IEnumerable<GameResult> GetResultsByPlayer(string playerName)
            => _results.Where(r => r.WinnerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                       .OrderByDescending(r => r.PlayedAt);

        public IEnumerable<GameResult> GetResultsByGame(string gameName)
            => _results.Where(r => r.GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase))
                       .OrderByDescending(r => r.PlayedAt);
    }
}
