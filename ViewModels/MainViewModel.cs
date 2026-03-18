using System;
using JuegosDeCartas_OpenCampus.Services;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public NavigationService Navigation { get; }

        public object? CurrentView => Navigation.CurrentView;

        public MainViewModel()
        {
            Navigation = new NavigationService(CreateView);

            Navigation.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Navigation.CurrentView))
                    OnPropertyChanged(nameof(CurrentView));
            };

            Navigation.NavigateTo(ViewName.MainMenu, addToHistory: false);
        }

        private object CreateView(ViewName view, ViewName selectedGame) => view switch
        {
            ViewName.MainMenu => new MainMenuViewModel(Navigation),
            ViewName.GameModeSelection => new GameModeSelectionViewModel(Navigation, selectedGame),
            ViewName.Instructions => new InstructionsViewModel(Navigation, selectedGame),
            ViewName.BlackJack => new BlackJackViewModel(Navigation),
            ViewName.VeintiUno => new VeintiUnoViewModel(Navigation),
            ViewName.Pitipar => new PitiparViewModel(Navigation),
            ViewName.OnlineLobby => new OnlineLobbyViewModel(Navigation),  // ← LÍNEA AGREGADA
            _ => throw new ArgumentOutOfRangeException(nameof(view))
        };
    }
}