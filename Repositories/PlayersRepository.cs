using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;

namespace JuegosDeCartas_OpenCampus.Repositories
{
    public class PlayersRepository : IPlayerRepository
    {
        private readonly List<Players> _players = new();

        public Players? GetByName(string name)
            => _players.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<Players> GetAll()
            => _players.AsReadOnly();

        public void Save(Players player)
        {
            if (GetByName(player.Name) is null)
                _players.Add(player);
        }

        public void Delete(string name)
            => _players.RemoveAll(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
