using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;
using JuegosDeCartas_OpenCampus.Repositories;
using JuegosDeCartas_OpenCampus.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class ScoreBoardViewModel : BaseViewModel
    {
        private readonly IScoreboardRepository _repo = new ScoreboardRepository();
        private readonly NavigationService     _nav;

        public ObservableCollection<ScoreBoardPlayer> Players { get; } = new();

        private string _selectedGame = "Todos";
        public string SelectedGame
        {
            get => _selectedGame;
            set { SetProperty(ref _selectedGame, value); LoadPlayers(); }
        }

        public List<string> GameFilters { get; } = new() { "Todos", "BlackJack", "Veintiuno", "Pitipar" };

        public ICommand BackCommand { get; }

        public ScoreBoardViewModel(NavigationService nav)
        {
            _nav        = nav;
            BackCommand = new RelayCommand(() => _nav.GoBack());
            LoadPlayers();
        }

        private void LoadPlayers()
        {
            Players.Clear();
            var entries = SelectedGame == "Todos"
                ? _repo.GetTopPlayers(20)
                : _repo.GetByGame(SelectedGame);

            foreach (var p in entries) Players.Add(p);
        }
    }
}

