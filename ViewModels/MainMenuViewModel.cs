using System.Collections.ObjectModel;
using System.Windows.Input;
using JuegosDeCartas_OpenCampus.Models;
using JuegosDeCartas_OpenCampus.Services;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        private readonly NavigationService _nav;

        public ICommand OpenBlackjackCommand { get; }
        public ICommand OpenTwentyOneCommand { get; }
        public ICommand OpenPitiparCommand { get; }
        public ICommand OpenOnlineCommand { get; }
        public ICommand OpenInstructionsCommand { get; }

        public MainMenuViewModel(NavigationService nav)
        {
            _nav = nav;

            OpenBlackjackCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.GameModeSelection, selectedGame: ViewName.BlackJack));

            OpenTwentyOneCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.GameModeSelection, selectedGame: ViewName.VeintiUno));

            OpenPitiparCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.GameModeSelection, selectedGame: ViewName.Pitipar));

            // ← Nuevo: navega al lobby online
            OpenOnlineCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.OnlineLobby));

            OpenInstructionsCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.Instructions));
        }
    }
}