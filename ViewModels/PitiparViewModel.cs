using JuegosDeCartas_OpenCampus.Models;
using JuegosDeCartas_OpenCampus.Repositories;
using JuegosDeCartas_OpenCampus.Services;
using System.Windows.Input;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class PitiparViewModel : BaseViewModel
    {
        private readonly NavigationService     _nav;
        private readonly IScoreboardRepository _scoreboard = new ScoreboardRepository();
        private readonly IGameRepository       _gameRepo   = new GameRepository();
        private Deck _deck = new();

        private const string PlayerName = "Jugador";

        private Card? _currentCard;
        private Card? _revealedCard;

        public string GameTitle => "♦  P I T I P A R";

        private string _currentRank = "?";
        public string CurrentRank { get => _currentRank; private set => SetProperty(ref _currentRank, value); }

        private string _currentSuit = string.Empty;
        public string CurrentSuit { get => _currentSuit; private set => SetProperty(ref _currentSuit, value); }

        private string _currentCardColor = "#1A0F2E";
        public string CurrentCardColor { get => _currentCardColor; private set => SetProperty(ref _currentCardColor, value); }

        private string _revealedRank = "?";
        public string RevealedRank { get => _revealedRank; private set => SetProperty(ref _revealedRank, value); }

        private string _revealedSuit = string.Empty;
        public string RevealedSuit { get => _revealedSuit; private set => SetProperty(ref _revealedSuit, value); }

        private string _revealedCardColor = "#1A0F2E";
        public string RevealedCardColor { get => _revealedCardColor; private set => SetProperty(ref _revealedCardColor, value); }

        private bool _isRevealed;
        public bool IsRevealed { get => _isRevealed; private set => SetProperty(ref _isRevealed, value); }

        private string _statusMessage = "¿Pi (mayor), Ti (igual) o Par (menor)?";
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

        private int _score;
        public int Score { get => _score; private set => SetProperty(ref _score, value); }

        public ICommand PiCommand        { get; }
        public ICommand TiCommand        { get; }
        public ICommand ParCommand       { get; }
        public ICommand NextRoundCommand { get; }
        public ICommand ExitCommand      { get; }

        public PitiparViewModel(NavigationService nav)
        {
            _nav = nav;

            PiCommand        = new RelayCommand(() => Predict("Pi"),  () => !IsRevealed);
            TiCommand        = new RelayCommand(() => Predict("Ti"),  () => !IsRevealed);
            ParCommand       = new RelayCommand(() => Predict("Par"), () => !IsRevealed);
            NextRoundCommand = new RelayCommand(NextRound, () => IsRevealed);
            ExitCommand      = new RelayCommand(() => _nav.NavigateTo(ViewName.MainMenu, addToHistory: false));

            DrawCurrentCard();
        }


        private void DrawCurrentCard()
        {
            if (_deck.RemainingCards < 2) _deck = new Deck();

            _currentCard  = _deck.Deal();
            _revealedCard = null;

            CurrentRank      = _currentCard.Rank;
            CurrentSuit      = SuitSymbol(_currentCard.Suit);
            CurrentCardColor = IsRed(_currentCard.Suit) ? "#C0392B" : "#1A0F2E";

            RevealedRank      = "?";
            RevealedSuit      = string.Empty;
            RevealedCardColor = "#1A0F2E";

            StatusMessage = "¿Pi (mayor), Ti (igual) o Par (menor)?";
            IsRevealed    = false;
        }

        private void Predict(string prediction)
        {
            if (_deck.RemainingCards < 1) _deck = new Deck();

            _revealedCard     = _deck.Deal();
            RevealedRank      = _revealedCard.Rank;
            RevealedSuit      = SuitSymbol(_revealedCard.Suit);
            RevealedCardColor = IsRed(_revealedCard.Suit) ? "#C0392B" : "#1A0F2E";
            IsRevealed        = true;

            bool correct = prediction switch
            {
                "Pi"  => _revealedCard.Value > _currentCard!.Value,
                "Ti"  => _revealedCard.Value == _currentCard!.Value,
                "Par" => _revealedCard.Value < _currentCard!.Value,
                _     => false
            };

            var resultType = correct ? ResultType.PlayerWin : ResultType.DealerWin;
            var result = new GameResult(resultType, correct ? PlayerName : "Casa",
                                        _revealedCard.Value, _currentCard!.Value,
                                        "Pitipar", correct ? 10 : 0);
            _gameRepo.SaveResult(result);

            var entry = _scoreboard.GetByPlayerAndGame(PlayerName, "Pitipar")
                        ?? new ScoreBoardPlayer(PlayerName, "Pitipar");
            entry.RecordResult(result);
            _scoreboard.Update(entry);

            Score += result.PointsEarned;

            string predLabel = prediction switch { "Pi" => "mayor", "Ti" => "igual", _ => "menor" };
            StatusMessage = correct
                ? $"✅ ¡Correcto! Dijiste {predLabel} — {_currentCard.Rank} → {_revealedCard.Rank}. +10 pts"
                : $"❌ Fallaste. Dijiste {predLabel} — {_currentCard.Rank} → {_revealedCard.Rank}.";
        }

        private void NextRound()
        {

            _currentCard      = _revealedCard;
            CurrentRank       = _currentCard!.Rank;
            CurrentSuit       = SuitSymbol(_currentCard.Suit);
            CurrentCardColor  = IsRed(_currentCard.Suit) ? "#C0392B" : "#1A0F2E";
            RevealedRank      = "?";
            RevealedSuit      = string.Empty;
            RevealedCardColor = "#1A0F2E";
            StatusMessage     = "¿Pi (mayor), Ti (igual) o Par (menor)?";
            IsRevealed        = false;
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
