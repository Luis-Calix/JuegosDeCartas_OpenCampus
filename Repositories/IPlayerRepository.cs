using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;

namespace JuegosDeCartas_OpenCampus.Repositories
{
    public interface IPlayerRepository
    {
        Players? GetByName(string name);
        IEnumerable<Players> GetAll();
        void Save(Players player);
        void Delete(string name);
    }
}
