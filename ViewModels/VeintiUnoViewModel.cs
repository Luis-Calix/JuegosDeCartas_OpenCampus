using JuegosDeCartas_OpenCampus.Models;
using JuegosDeCartas_OpenCampus.Repositories;
using JuegosDeCartas_OpenCampus.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JuegosDeCartas_OpenCampus.ViewModels
{

    public class VeintiUnoViewModel : BaseViewModel
    {
        private readonly NavigationService     _nav;
        private readonly GameEngineService     _engine     = new();
        private readonly IScoreboardRepository _scoreboard = new ScoreboardRepository();
        private readonly IGameRepository       _gameRepo   = new GameRepository();

        private Deck _deck = new();

        private Card? _playerCard;
        private Card? _dealerCard;

        public string GameTitle => "⚔  G U E R R A";

        private string _playerCardDisplay = string.Empty;
        public string PlayerCardDisplay { get => _playerCardDisplay; private set => SetProperty(ref _playerCardDisplay, value); }

        private string _playerCardSuit = string.Empty;
        public string PlayerCardSuit { get => _playerCardSuit; private set => SetProperty(ref _playerCardSuit, value); }

        private string _playerCardColor = "#1A0F2E";
        public string PlayerCardColor { get => _playerCardColor; private set => SetProperty(ref _playerCardColor, value); }

        private string _dealerCardDisplay = "?";
        public string DealerCardDisplay { get => _dealerCardDisplay; private set => SetProperty(ref _dealerCardDisplay, value); }

        private string _dealerCardSuit = string.Empty;
        public string DealerCardSuit { get => _dealerCardSuit; private set => SetProperty(ref _dealerCardSuit, value); }

        private string _dealerCardColor = "#1A0F2E";
        public string DealerCardColor { get => _dealerCardColor; private set => SetProperty(ref _dealerCardColor, value); }


        private int _playerWins;
        public int PlayerWins { get => _playerWins; private set => SetProperty(ref _playerWins, value); }

        private int _dealerWins;
        public int DealerWins { get => _dealerWins; private set => SetProperty(ref _dealerWins, value); }

        private string _statusMessage = "Presiona 'Robar carta' para comenzar la batalla.";
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

        private bool _isRevealed;
        public bool IsRevealed { get => _isRevealed; private set => SetProperty(ref _isRevealed, value); }

        private bool _isDealerHidden = true;
        public bool IsDealerHidden { get => _isDealerHidden; private set => SetProperty(ref _isDealerHidden, value); }

        public ICommand DrawCommand     { get; }
        public ICommand NextRoundCommand{ get; }
        public ICommand NewGameCommand  { get; }
        public ICommand ExitCommand     { get; }

        public VeintiUnoViewModel(NavigationService nav)
        {
            _nav = nav;

            DrawCommand      = new RelayCommand(Draw,      () => !IsRevealed);
            NextRoundCommand = new RelayCommand(NextRound, () => IsRevealed);
            NewGameCommand   = new RelayCommand(NewGame);
            ExitCommand      = new RelayCommand(() => _nav.NavigateTo(ViewName.MainMenu, addToHistory: false));

            ResetDisplay();
        }


        private void Draw()
        {

            _playerCard = _deck.Deal();
            _dealerCard = _deck.Deal();


            PlayerCardDisplay = _playerCard.Rank;
            PlayerCardSuit    = SuitSymbol(_playerCard.Suit);
            PlayerCardColor   = IsRed(_playerCard.Suit) ? "#C0392B" : "#1A0F2E";

            DealerCardDisplay = _dealerCard.Rank;
            DealerCardSuit    = SuitSymbol(_dealerCard.Suit);
            DealerCardColor   = IsRed(_dealerCard.Suit) ? "#C0392B" : "#1A0F2E";
            IsDealerHidden    = false;


            EvaluateRound();
            IsRevealed = true;
        }

        private void EvaluateRound()
        {
            int pv = _playerCard!.Value;
            int dv = _dealerCard!.Value;

            if (pv > dv)
            {
                PlayerWins++;
                StatusMessage = $"🏆 ¡Ganaste! Tu {_playerCard.Rank} supera al {_dealerCard.Rank} del dealer.";
            }
            else if (dv > pv)
            {
                DealerWins++;
                StatusMessage = $"💀 Perdiste. El dealer tiene {_dealerCard.Rank}, tú tienes {_playerCard.Rank}.";
            }
            else
            {
                StatusMessage = $"⚔ ¡GUERRA! Ambos tienen {_playerCard.Rank}. ¡Roba otra carta!";
            }

            var resultType = pv > dv ? ResultType.PlayerWin : pv < dv ? ResultType.DealerWin : ResultType.Tie;
            var result = new GameResult(resultType, pv > dv ? "Jugador" : "Dealer",
                                        pv, dv, "Guerra", pv > dv ? 10 : 0);
            _gameRepo.SaveResult(result);

            var entry = _scoreboard.GetByPlayerAndGame("Jugador", "Guerra")
                        ?? new ScoreBoardPlayer("Jugador", "Guerra");
            entry.RecordResult(result);
            _scoreboard.Update(entry);
        }

        private void NextRound()
        {
            if (_deck.RemainingCards < 4)
            {
                _deck = new Deck();
                StatusMessage = "🔀 Mazo repuesto. ¡Siguiente ronda!";
            }
            else
            {
                StatusMessage = "Presiona 'Robar carta' para la siguiente batalla.";
            }

            ResetDisplay();
            IsRevealed = false;
        }

        private void NewGame()
        {
            _deck       = new Deck();
            PlayerWins  = 0;
            DealerWins  = 0;
            IsRevealed  = false;
            ResetDisplay();
            StatusMessage = "¡Nueva partida! Presiona 'Robar carta' para comenzar.";
        }

        private void ResetDisplay()
        {
            PlayerCardDisplay = "?";
            PlayerCardSuit    = string.Empty;
            PlayerCardColor   = "#1A0F2E";
            DealerCardDisplay = "?";
            DealerCardSuit    = string.Empty;
            DealerCardColor   = "#1A0F2E";
            IsDealerHidden    = true;
        }

        private static string SuitSymbol(string suit) => suit switch
        {
            "Corazones" => "♥",
            "Diamantes" => "♦",
            "Tréboles"  => "♣",
            "Picas"     => "♠",
            _           => suit
        };

        private static bool IsRed(string suit) =>
            suit == "Corazones" || suit == "Diamantes";
    }
}
