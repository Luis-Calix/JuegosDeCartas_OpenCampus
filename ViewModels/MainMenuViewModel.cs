using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;
using JuegosDeCartas_OpenCampus.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        private readonly NavigationService _nav;

        public ObservableCollection<ScoreBoardPlayer> TopPlayers { get; } = new();

        public ICommand OpenBlackjackCommand { get; }
        public ICommand OpenTwentyOneCommand { get; }
        public ICommand OpenPitiparCommand   { get; }

        public MainMenuViewModel(NavigationService nav)
        {
            _nav = nav;

            OpenBlackjackCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.GameModeSelection, selectedGame: ViewName.BlackJack));

            OpenTwentyOneCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.GameModeSelection, selectedGame: ViewName.VeintiUno));

            OpenPitiparCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.GameModeSelection, selectedGame: ViewName.Pitipar));

            LoadTopPlayers();
        }

        private void LoadTopPlayers()
        {
            TopPlayers.Add(new ScoreBoardPlayer { PlayerName = "Jugador 1", Game = "BlackJack", Points = 150 });
            TopPlayers.Add(new ScoreBoardPlayer { PlayerName = "Jugador 2", Game = "Veintiuno", Points = 120 });
            TopPlayers.Add(new ScoreBoardPlayer { PlayerName = "Jugador 3", Game = "Pitipar",   Points = 90  });
        }
    }
}

