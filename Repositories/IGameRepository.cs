using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;

namespace JuegosDeCartas_OpenCampus.Repositories
{
    public interface IGameRepository
    {
        void SaveResult(GameResult result);
        IEnumerable<GameResult> GetResultsByPlayer(string playerName);
        IEnumerable<GameResult> GetResultsByGame(string gameName);
    }
}
