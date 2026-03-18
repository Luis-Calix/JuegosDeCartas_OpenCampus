using JuegosDeCartas_OpenCampus.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JuegosDeCartas_OpenCampus.Services
{
    public enum ViewName
    {
        MainMenu,
        GameModeSelection,
        Instructions,
        BlackJack,
        VeintiUno,
        Pitipar,
        OnlineLobby,
    }

    public class NavigationService : INotifyPropertyChanged
    {
        private readonly Func<ViewName, ViewName, object> _viewFactory;

        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            private set { _currentView = value; OnPropertyChanged(); }
        }

        private readonly Stack<object> _history = new();

        public NavigationService(Func<ViewName, ViewName, object> viewFactory)
        {
            _viewFactory = viewFactory;
        }

        /// <summary>
        /// Navega a un ViewModel ya construido (usado por el lobby online
        /// para pasar al juego conservando la conexión SignalR).
        /// </summary>
        public void NavigateToViewModel(BaseViewModel viewModel)
        {
            if (_currentView is not null)
                _history.Push(_currentView);

            CurrentView = viewModel;
        }

        public void NavigateTo(ViewName view, bool addToHistory = true, ViewName selectedGame = ViewName.BlackJack)
        {
            if (_currentView is not null && addToHistory)
                _history.Push(_currentView);

            CurrentView = _viewFactory(view, selectedGame);
        }

        public void GoBack()
        {
            if (_history.Count > 0)
                CurrentView = _history.Pop();
        }

        public bool CanGoBack => _history.Count > 0;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}