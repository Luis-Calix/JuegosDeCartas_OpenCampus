using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JuegosDeCartas_OpenCampus.Services
{
    /// <summary>
    /// DTO que mapea las cartas que llegan desde el servidor.
    /// Debe coincidir con CardDto del servidor.
    /// </summary>
    public record OnlineCard(string Suit, string Rank, int Value, string ImagePath);

    /// <summary>
    /// Servicio que encapsula toda la comunicación SignalR con el servidor.
    /// Úsalo inyectado en tu ViewModel de juego online.
    ///
    /// FLUJO TÍPICO:
    ///   1. await ConnectAsync()
    ///   2. Suscríbete a los eventos (OnGameStarted, OnCardReceived, etc.)
    ///   3. await CreateRoomAsync() o await JoinRoomAsync()
    ///   4. El host llama await StartGameAsync() cuando el guest se une
    ///   5. Durante el juego: HitAsync() / StandAsync()
    ///   6. await DisconnectAsync() al salir
    /// </summary>
    public class OnlineMultiplayerService : IAsyncDisposable
    {
        // ─── Configuración ────────────────────────────────────────────────────
        // Cambia esta URL a la IP/puerto donde corra tu servidor
        private const string ServerUrl = "http://localhost:5000/gamehub";

        private HubConnection? _hub;

        // ─── Estado local ─────────────────────────────────────────────────────
        public string RoomCode    { get; private set; } = string.Empty;
        public string MyName      { get; private set; } = string.Empty;
        public string OpponentName{ get; private set; } = string.Empty;
        public bool   IsHost      { get; private set; }
        public bool   IsMyTurn    { get; private set; }
        public bool   IsConnected => _hub?.State == HubConnectionState.Connected;

        // ─── Eventos — el ViewModel se suscribe a estos ───────────────────────

        /// <summary>Sala creada. Parámetro: código de sala.</summary>
        public event Action<string>? OnRoomCreated;

        /// <summary>Otro jugador se unió. Parámetros: hostName, guestName, roomCode.</summary>
        public event Action<string, string, string>? OnPlayerJoined;

        /// <summary>
        /// Partida iniciada.
        /// Parámetros: myCards, dealerCards, opponentName, isMyTurn
        /// </summary>
        public event Action<List<OnlineCard>, List<OnlineCard>, string, bool>? OnGameStarted;

        /// <summary>Carta recibida. Parámetros: card, newScore.</summary>
        public event Action<OnlineCard, int>? OnCardReceived;

        /// <summary>El oponente pidió carta. Parámetro: opponentName.</summary>
        public event Action<string>? OnOpponentHit;

        /// <summary>Un jugador se plantó. Parámetro: playerName.</summary>
        public event Action<string>? OnPlayerStood;

        /// <summary>Un jugador se pasó. Parámetros: playerName, score.</summary>
        public event Action<string, int>? OnBust;

        /// <summary>
        /// Resultado final de la ronda.
        /// Parámetros: dealerHand, dealerScore,
        ///             hostName, hostScore, hostResult,
        ///             guestName, guestScore, guestResult
        /// </summary>
        public event Action<List<OnlineCard>, int, string, int, string, string, int, string>? OnGameResult;

        /// <summary>Ronda nueva. El ViewModel limpia la pantalla.</summary>
        public event Action? OnNewRound;

        /// <summary>Turno cambiado (Pitipar). Parámetro: nombre del jugador activo.</summary>
        public event Action<string>? OnTurnChanged;

        /// <summary>
        /// Resultado de predicción Pitipar.
        /// Parámetros: playerName, prediction, nextCard, isCorrect, points
        /// </summary>
        public event Action<string, string, OnlineCard, bool, int>? OnPitiparResult;

        /// <summary>Inicio de partida Pitipar. Parámetro: primera carta.</summary>
        public event Action<OnlineCard>? OnPitiparStart;

        /// <summary>Un jugador se desconectó. Parámetro: nombre.</summary>
        public event Action<string>? OnPlayerLeft;

        /// <summary>Error del servidor. Parámetro: mensaje.</summary>
        public event Action<string>? OnError;

        /// <summary>Cambio en el estado de conexión.</summary>
        public event Action<string>? OnConnectionStateChanged;

        // ─── Conexión ─────────────────────────────────────────────────────────

        public async Task ConnectAsync(string playerName)
        {
            MyName = playerName;

            _hub = new HubConnectionBuilder()
                .WithUrl(ServerUrl)
                .WithAutomaticReconnect()
                .Build();

            RegisterHandlers();

            _hub.Reconnecting  += _ => { OnConnectionStateChanged?.Invoke("Reconectando..."); return Task.CompletedTask; };
            _hub.Reconnected   += _ => { OnConnectionStateChanged?.Invoke("Conectado");       return Task.CompletedTask; };
            _hub.Closed        += _ => { OnConnectionStateChanged?.Invoke("Desconectado");    return Task.CompletedTask; };

            await _hub.StartAsync();
            OnConnectionStateChanged?.Invoke("Conectado");
        }

        public async Task DisconnectAsync()
        {
            if (_hub != null)
                await _hub.StopAsync();
        }

        // ─── Acciones del jugador ─────────────────────────────────────────────

        public async Task CreateRoomAsync(string gameName)
        {
            IsHost = true;
            await InvokeAsync("CreateRoom", MyName, gameName);
        }

        public async Task JoinRoomAsync(string code)
        {
            IsHost = false;
            await InvokeAsync("JoinRoom", code, MyName);
        }

        public async Task StartGameAsync()
            => await InvokeAsync("StartGame");

        public async Task HitAsync()
            => await InvokeAsync("Hit");

        public async Task StandAsync()
            => await InvokeAsync("Stand");

        public async Task RequestNewRoundAsync()
            => await InvokeAsync("RequestNewRound");

        // ── Pitipar ───────────────────────────────────────────────────────────

        public async Task StartPitiparAsync()
            => await InvokeAsync("StartPitipar");

        public async Task MakePitiparPredictionAsync(
            string suit, string rank, int value, string prediction)
            => await InvokeAsync("MakePitiparPrediction", suit, rank, value, prediction);

        // ─── Handlers del servidor ────────────────────────────────────────────

        private void RegisterHandlers()
        {
            if (_hub == null) return;

            _hub.On<string, string>("ReceiveRoomCreated", (code, host) =>
            {
                RoomCode = code;
                OnRoomCreated?.Invoke(code);
            });

            _hub.On<string, string, string>("ReceivePlayerJoined", (host, guest, code) =>
            {
                RoomCode      = code;
                OpponentName  = IsHost ? guest : host;
                OnPlayerJoined?.Invoke(host, guest, code);
            });

            _hub.On<List<OnlineCard>, List<OnlineCard>, string, bool>(
                "ReceiveGameStarted",
                (myCards, dealerCards, opponentName, isMyTurn) =>
                {
                    OpponentName = opponentName;
                    IsMyTurn     = isMyTurn;
                    OnGameStarted?.Invoke(myCards, dealerCards, opponentName, isMyTurn);
                });

            _hub.On<OnlineCard, int>("ReceiveCard", (card, score) =>
            {
                OnCardReceived?.Invoke(card, score);
            });

            _hub.On<string>("ReceiveOpponentHit", name => OnOpponentHit?.Invoke(name));

            _hub.On<string>("ReceivePlayerStood", name => OnPlayerStood?.Invoke(name));

            _hub.On<string, int>("ReceiveBust", (name, score) => OnBust?.Invoke(name, score));

            _hub.On<List<OnlineCard>, int, string, int, string, string, int, string>(
                "ReceiveGameResult",
                (dealerHand, ds, hn, hs, hr, gn, gs, gr) =>
                    OnGameResult?.Invoke(dealerHand, ds, hn, hs, hr, gn, gs, gr));

            _hub.On("ReceiveNewRound", () => OnNewRound?.Invoke());

            _hub.On<string>("ReceiveTurnChanged", name =>
            {
                IsMyTurn = name == MyName;
                OnTurnChanged?.Invoke(name);
            });

            _hub.On<string, string, OnlineCard, bool, int>(
                "ReceivePitiparResult",
                (player, pred, card, correct, pts) =>
                    OnPitiparResult?.Invoke(player, pred, card, correct, pts));

            _hub.On<OnlineCard>("ReceivePitiparStart", card => OnPitiparStart?.Invoke(card));

            _hub.On<string>("ReceivePlayerLeft", name => OnPlayerLeft?.Invoke(name));

            _hub.On<string>("ReceiveError", msg => OnError?.Invoke(msg));
        }

        // ─── Utilidad ─────────────────────────────────────────────────────────

        private async Task InvokeAsync(string method, params object[] args)
        {
            if (_hub?.State != HubConnectionState.Connected)
            {
                OnError?.Invoke("No estás conectado al servidor.");
                return;
            }
            try
            {
                await _hub.InvokeCoreAsync(method, args);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error de red: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hub != null)
                await _hub.DisposeAsync();
        }
    }
}
