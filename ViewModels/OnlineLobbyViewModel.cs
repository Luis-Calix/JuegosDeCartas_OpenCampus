using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using JuegosDeCartas_OpenCampus.Services;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class OnlineLobbyViewModel : BaseViewModel, IAsyncDisposable
    {
        private readonly NavigationService _nav;
        private readonly OnlineMultiplayerService _online = new();

        private string _playerName = "Jugador";
        public string PlayerName
        {
            get => _playerName;
            set => SetProperty(ref _playerName, value);
        }

        private string _joinCode = string.Empty;
        public string JoinCode
        {
            get => _joinCode;
            set => SetProperty(ref _joinCode, value.ToUpper());
        }

        // ↓ CAMBIO 1: RoomCode ahora notifica también HasRoomCode
        private string _roomCode = string.Empty;
        public string RoomCode
        {
            get => _roomCode;
            private set
            {
                SetProperty(ref _roomCode, value);
                OnPropertyChanged(nameof(HasRoomCode));
            }
        }

        // ↓ CAMBIO 2: nueva propiedad que usa el XAML para mostrar/ocultar el código
        public bool HasRoomCode => !string.IsNullOrEmpty(RoomCode);

        private string _statusMessage = "Introduce tu nombre y elige un juego.";
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        private string _selectedGame = "BlackJack";
        public string SelectedGame
        {
            get => _selectedGame;
            set => SetProperty(ref _selectedGame, value);
        }

        private bool _isHost;
        public bool IsHost
        {
            get => _isHost;
            private set => SetProperty(ref _isHost, value);
        }

        private bool _guestJoined;
        public bool GuestJoined
        {
            get => _guestJoined;
            private set => SetProperty(ref _guestJoined, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        public ICommand CreateRoomCommand { get; }
        public ICommand JoinRoomCommand { get; }
        public ICommand StartGameCommand { get; }
        public ICommand CopyCodeCommand { get; }
        public ICommand BackCommand { get; }

        public OnlineLobbyViewModel(NavigationService nav)
        {
            _nav = nav;

            CreateRoomCommand = new RelayCommand(async () => await CreateRoomAsync(), () => !IsBusy);
            JoinRoomCommand = new RelayCommand(async () => await JoinRoomAsync(), () => !IsBusy && !string.IsNullOrWhiteSpace(JoinCode));
            StartGameCommand = new RelayCommand(async () => await StartGameAsync(), () => IsHost && GuestJoined && !IsBusy);
            CopyCodeCommand = new RelayCommand(() => Clipboard.SetText(RoomCode), () => !string.IsNullOrEmpty(RoomCode));
            BackCommand = new RelayCommand(async () =>
            {
                await _online.DisconnectAsync();
                _nav.NavigateTo(ViewName.MainMenu, addToHistory: false);
            });
        }

        private async Task CreateRoomAsync()
        {
            if (string.IsNullOrWhiteSpace(PlayerName)) return;
            IsBusy = true;
            StatusMessage = "Conectando al servidor…";
            try
            {
                SubscribeToEvents();
                await _online.ConnectAsync(PlayerName);
                await _online.CreateRoomAsync(SelectedGame);
                IsHost = true;
                StatusMessage = "Sala creada. Esperando al oponente…";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task JoinRoomAsync()
        {
            if (string.IsNullOrWhiteSpace(PlayerName) || string.IsNullOrWhiteSpace(JoinCode)) return;
            IsBusy = true;
            StatusMessage = "Uniéndose a la sala…";
            try
            {
                SubscribeToEvents();
                await _online.ConnectAsync(PlayerName);
                await _online.JoinRoomAsync(JoinCode);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private async Task StartGameAsync()
        {
            IsBusy = true;
            try { await _online.StartGameAsync(); }
            finally { IsBusy = false; }
        }

        private void SubscribeToEvents()
        {
            _online.OnRoomCreated += code =>
                RunOnUI(() => RoomCode = code);

            _online.OnPlayerJoined += (host, guest, code) =>
                RunOnUI(() =>
                {
                    GuestJoined = true;
                    StatusMessage = IsHost
                        ? $"{guest} se unió. ¡Puedes iniciar la partida!"
                        : $"Te uniste a la sala de {host}.";
                });

            _online.OnGameStarted += (myCards, dealerCards, opponentName, isMyTurn) =>
                RunOnUI(() =>
                {
                    var gameVm = new OnlineBlackJackViewModel(_nav, _online);
                    gameVm.InitializeFromLobby(myCards, dealerCards, opponentName, isMyTurn);
                    _nav.NavigateToViewModel(gameVm);
                });

            _online.OnError += msg =>
                RunOnUI(() => StatusMessage = $"⚠ {msg}");

            _online.OnConnectionStateChanged += state =>
                RunOnUI(() => StatusMessage = state);

            _online.OnPlayerLeft += name =>
                RunOnUI(() => StatusMessage = $"{name} abandonó la sala.");
        }

        private static void RunOnUI(Action a)
        {
            if (Application.Current?.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(a);
            else
                a();
        }

        public async ValueTask DisposeAsync() => await _online.DisposeAsync();
    }
}