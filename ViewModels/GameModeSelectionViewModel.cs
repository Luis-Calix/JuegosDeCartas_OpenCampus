using JuegosDeCartas_OpenCampus.Services;
using System.Windows.Input;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class GameModeSelectionViewModel : BaseViewModel
    {
        private readonly NavigationService _nav;
        private readonly ViewName _selectedGame;

        public ICommand OnlineModeCommand { get; }
        public ICommand LocalModeCommand  { get; }
        public ICommand CpuModeCommand    { get; }
        public ICommand BackCommand       { get; }

        public GameModeSelectionViewModel(NavigationService nav, ViewName selectedGame)
        {
            _nav          = nav;
            _selectedGame = selectedGame;

            OnlineModeCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.Instructions, selectedGame: _selectedGame));

            LocalModeCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.Instructions, selectedGame: _selectedGame));

            CpuModeCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.Instructions, selectedGame: _selectedGame));

            BackCommand = new RelayCommand(() => _nav.GoBack());
        }
    }
}
