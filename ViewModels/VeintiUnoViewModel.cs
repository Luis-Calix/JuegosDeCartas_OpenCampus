using JuegosDeCartas_OpenCampus.Models;
using JuegosDeCartas_OpenCampus.Repositories;
using JuegosDeCartas_OpenCampus.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    /// <summary>
    /// Juego de Guerra.
    /// 
    /// Flujo normal:
    ///   Robar carta → ambos revelan una carta → mayor gana y se lleva todas las cartas de la mesa.
    /// 
    /// Empate → GUERRA:
    ///   Cada jugador pone 1 carta boca abajo (al fondo de la pila de guerra)
    ///   y luego 1 carta boca arriba que se compara.
    ///   Si vuelve a empatar, se repite la guerra.
    ///   El ganador de la guerra se lleva TODAS las cartas acumuladas en la mesa.
    /// 
    /// Fin de partida: un jugador se queda sin cartas.
    /// </summary>
    public class VeintiUnoViewModel : BaseViewModel
    {
        private readonly NavigationService _nav;
        private readonly IScoreboardRepository _scoreboard = new ScoreboardRepository();
        private readonly IGameRepository _gameRepo = new GameRepository();

        // Mazos individuales de cada jugador (se llenan con 26 cartas cada uno al inicio)
        private Queue<Card> _playerDeck = new();
        private Queue<Card> _dealerDeck = new();

        // Cartas acumuladas en la mesa durante una guerra
        private readonly List<Card> _warPot = new();

        // Estado de la ronda actual
        private Card? _playerCard;
        private Card? _dealerCard;
        private bool _isInWar;

        // ── Propiedades ───────────────────────────────────────────────────────

        public string GameTitle => "⚔  G U E R R A";

        // Carta boca arriba del jugador
        private string _playerCardDisplay = "?";
        public string PlayerCardDisplay { get => _playerCardDisplay; private set => SetProperty(ref _playerCardDisplay, value); }

        private string _playerCardSuit = string.Empty;
        public string PlayerCardSuit { get => _playerCardSuit; private set => SetProperty(ref _playerCardSuit, value); }

        private string _playerCardColor = "#1A0F2E";
        public string PlayerCardColor { get => _playerCardColor; private set => SetProperty(ref _playerCardColor, value); }

        // Carta boca arriba del dealer
        private string _dealerCardDisplay = "?";
        public string DealerCardDisplay { get => _dealerCardDisplay; private set => SetProperty(ref _dealerCardDisplay, value); }

        private string _dealerCardSuit = string.Empty;
        public string DealerCardSuit { get => _dealerCardSuit; private set => SetProperty(ref _dealerCardSuit, value); }

        private string _dealerCardColor = "#1A0F2E";
        public string DealerCardColor { get => _dealerCardColor; private set => SetProperty(ref _dealerCardColor, value); }

        // Cartas boca abajo durante la guerra
        public ObservableCollection<string> PlayerFaceDownCards { get; } = new();
        public ObservableCollection<string> DealerFaceDownCards { get; } = new();

        // Tamaños de mazo
        private int _playerDeckCount;
        public int PlayerDeckCount { get => _playerDeckCount; private set => SetProperty(ref _playerDeckCount, value); }

        private int _dealerDeckCount;
        public int DealerDeckCount { get => _dealerDeckCount; private set => SetProperty(ref _dealerDeckCount, value); }

        // Cartas en el bote de guerra
        private int _warPotCount;
        public int WarPotCount { get => _warPotCount; private set => SetProperty(ref _warPotCount, value); }

        private bool _isWarActive;
        public bool IsWarActive { get => _isWarActive; private set => SetProperty(ref _isWarActive, value); }

        private bool _isDealerHidden = true;
        public bool IsDealerHidden { get => _isDealerHidden; private set => SetProperty(ref _isDealerHidden, value); }

        private bool _isRoundOver;
        public bool IsRoundOver { get => _isRoundOver; private set => SetProperty(ref _isRoundOver, value); }

        private string _statusMessage = "Presiona 'Robar carta' para comenzar.";
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

        // Marcador de rondas ganadas
        private int _playerWins;
        public int PlayerWins { get => _playerWins; private set => SetProperty(ref _playerWins, value); }

        private int _dealerWins;
        public int DealerWins { get => _dealerWins; private set => SetProperty(ref _dealerWins, value); }

        // ── Comandos ──────────────────────────────────────────────────────────
        public ICommand DrawCommand { get; }
        public ICommand NextRoundCommand { get; }
        public ICommand NewGameCommand { get; }
        public ICommand ExitCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────
        public VeintiUnoViewModel(NavigationService nav)
        {
            _nav = nav;

            DrawCommand = new RelayCommand(DrawCards, () => !IsRoundOver);
            NextRoundCommand = new RelayCommand(NextRound, () => IsRoundOver);
            NewGameCommand = new RelayCommand(NewGame);
            ExitCommand = new RelayCommand(() =>
                _nav.NavigateTo(ViewName.MainMenu, addToHistory: false));

            NewGame();
        }

        // ── Lógica principal ──────────────────────────────────────────────────

        private void DrawCards()
        {
            // Verificar que ambos tienen cartas
            if (_playerDeck.Count == 0 || _dealerDeck.Count == 0)
            {
                CheckGameOver();
                return;
            }

            // Robar carta boca arriba para cada jugador
            _playerCard = _playerDeck.Dequeue();
            _dealerCard = _dealerDeck.Dequeue();

            // Añadir al bote de la mesa
            _warPot.Add(_playerCard);
            _warPot.Add(_dealerCard);

            // Mostrar cartas
            ShowCard(_playerCard, isPlayer: true);
            IsDealerHidden = false;
            ShowCard(_dealerCard, isPlayer: false);
            UpdateCounts();

            // Evaluar resultado
            if (_playerCard.Value > _dealerCard.Value)
            {
                // Jugador gana — se lleva el bote
                GiveCardsTo(_playerDeck, _warPot);
                PlayerWins++;
                IsWarActive = false;
                IsRoundOver = true;
                StatusMessage = $"🏆 ¡Ganaste! Tu {_playerCard.Rank} supera al {_dealerCard.Rank}. Te llevas {_warPot.Count} cartas. ▶ Siguiente";
                _warPot.Clear();
                SaveResult(ResultType.PlayerWin);
            }
            else if (_dealerCard.Value > _playerCard.Value)
            {
                // Dealer gana — se lleva el bote
                GiveCardsTo(_dealerDeck, _warPot);
                DealerWins++;
                IsWarActive = false;
                IsRoundOver = true;
                StatusMessage = $"💀 El dealer gana. Su {_dealerCard.Rank} supera a tu {_playerCard.Rank}. Pierde {_warPot.Count} cartas. ▶ Siguiente";
                _warPot.Clear();
                SaveResult(ResultType.DealerWin);
            }
            else
            {
                // EMPATE → GUERRA
                StartWar();
            }

            UpdateCounts();
            CheckGameOver();
        }

        private void StartWar()
        {
            // Verificar que ambos tienen al menos 2 cartas para la guerra (1 boca abajo + 1 boca arriba)
            if (_playerDeck.Count < 2 || _dealerDeck.Count < 2)
            {
                // Si no hay suficientes cartas, el que tiene más gana
                if (_playerDeck.Count >= _dealerDeck.Count)
                {
                    GiveCardsTo(_playerDeck, _warPot);
                    PlayerWins++;
                    StatusMessage = "⚔ ¡Empate pero el dealer no tiene cartas suficientes para la guerra! Tú ganas esta batalla.";
                }
                else
                {
                    GiveCardsTo(_dealerDeck, _warPot);
                    DealerWins++;
                    StatusMessage = "⚔ ¡Empate pero no tienes cartas suficientes para la guerra! El dealer gana esta batalla.";
                }
                _warPot.Clear();
                IsRoundOver = true;
                return;
            }

            _isInWar = true;
            IsWarActive = true;

            // Cada jugador pone 1 carta boca abajo
            var playerFaceDown = _playerDeck.Dequeue();
            var dealerFaceDown = _dealerDeck.Dequeue();
            _warPot.Add(playerFaceDown);
            _warPot.Add(dealerFaceDown);

            // Mostrar representación de cartas boca abajo
            PlayerFaceDownCards.Add("🂠");
            DealerFaceDownCards.Add("🂠");

            // Ocultar las cartas boca arriba anteriores
            PlayerCardDisplay = "?";
            PlayerCardSuit = string.Empty;
            DealerCardDisplay = "?";
            DealerCardSuit = string.Empty;
            IsDealerHidden = true;

            WarPotCount = _warPot.Count;
            StatusMessage = $"⚔ ¡GUERRA! Ambos tienen {_playerCard!.Rank}. Se pusieron {PlayerFaceDownCards.Count} carta(s) boca abajo. ¡Roba de nuevo para revelar la carta de combate!";
        }

        private void NextRound()
        {
            IsRoundOver = false;
            _isInWar = false;
            IsWarActive = false;
            IsDealerHidden = true;
            PlayerFaceDownCards.Clear();
            DealerFaceDownCards.Clear();
            ResetCardDisplay();
            StatusMessage = "Presiona 'Robar carta' para la siguiente batalla.";
            UpdateCounts();
        }

        private void NewGame()
        {
            // Crear mazo y dividirlo en 2
            var deck = new Deck();
            _playerDeck = new Queue<Card>();
            _dealerDeck = new Queue<Card>();
            _warPot.Clear();

            int i = 0;
            while (deck.RemainingCards > 0)
            {
                var card = deck.Deal();
                if (i % 2 == 0) _playerDeck.Enqueue(card);
                else _dealerDeck.Enqueue(card);
                i++;
            }

            PlayerWins = 0;
            DealerWins = 0;
            IsRoundOver = false;
            IsWarActive = false;
            _isInWar = false;
            PlayerFaceDownCards.Clear();
            DealerFaceDownCards.Clear();
            IsDealerHidden = true;
            ResetCardDisplay();
            UpdateCounts();
            StatusMessage = "¡Nueva partida! Cada jugador tiene 26 cartas. Presiona 'Robar carta'.";
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ShowCard(Card card, bool isPlayer)
        {
            string rank = card.Rank;
            string suit = SuitSymbol(card.Suit);
            string color = IsRed(card.Suit) ? "#C0392B" : "#1A0F2E";

            if (isPlayer)
            {
                PlayerCardDisplay = rank;
                PlayerCardSuit = suit;
                PlayerCardColor = color;
            }
            else
            {
                DealerCardDisplay = rank;
                DealerCardSuit = suit;
                DealerCardColor = color;
            }
        }

        private void ResetCardDisplay()
        {
            PlayerCardDisplay = "?";
            PlayerCardSuit = string.Empty;
            PlayerCardColor = "#1A0F2E";
            DealerCardDisplay = "?";
            DealerCardSuit = string.Empty;
            DealerCardColor = "#1A0F2E";
        }

        private void UpdateCounts()
        {
            PlayerDeckCount = _playerDeck.Count;
            DealerDeckCount = _dealerDeck.Count;
            WarPotCount = _warPot.Count;
        }

        private void CheckGameOver()
        {
            if (_playerDeck.Count == 0)
            {
                StatusMessage = "💀 ¡Sin cartas! El dealer gana la partida. Presiona 'Nueva partida'.";
                IsRoundOver = true;
            }
            else if (_dealerDeck.Count == 0)
            {
                StatusMessage = "🏆 ¡El dealer no tiene cartas! Ganas la partida. Presiona 'Nueva partida'.";
                IsRoundOver = true;
            }
        }

        private static void GiveCardsTo(Queue<Card> deck, List<Card> cards)
        {
            // Barajar las cartas ganadas antes de añadirlas al fondo del mazo
            var shuffled = cards.OrderBy(_ => Guid.NewGuid()).ToList();
            foreach (var c in shuffled)
                deck.Enqueue(c);
        }

        private void SaveResult(ResultType type)
        {
            var result = new GameResult(type,
                type == ResultType.PlayerWin ? "Jugador" : "Dealer",
                _playerCard?.Value ?? 0, _dealerCard?.Value ?? 0,
                "Guerra", type == ResultType.PlayerWin ? 10 : 0);

            _gameRepo.SaveResult(result);

            var entry = _scoreboard.GetByPlayerAndGame("Jugador", "Guerra")
                        ?? new ScoreBoardPlayer("Jugador", "Guerra");
            entry.RecordResult(result);
            _scoreboard.Update(entry);
        }

        private static string SuitSymbol(string suit) => suit switch
        {
            "Corazones" => "♥",
            "Diamantes" => "♦",
            "Tréboles" => "♣",
            "Picas" => "♠",
            _ => suit
        };

        private static bool IsRed(string suit) =>
            suit == "Corazones" || suit == "Diamantes";
    }
}