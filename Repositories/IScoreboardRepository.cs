using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;

namespace JuegosDeCartas_OpenCampus.Repositories
{
    public interface IScoreboardRepository
    {
        IEnumerable<ScoreBoardPlayer> GetTopPlayers(int count = 10);
        IEnumerable<ScoreBoardPlayer> GetByGame(string gameName);
        ScoreBoardPlayer? GetByPlayerAndGame(string playerName, string gameName);
        void Save(ScoreBoardPlayer entry);
        void Update(ScoreBoardPlayer entry);
    }
}
