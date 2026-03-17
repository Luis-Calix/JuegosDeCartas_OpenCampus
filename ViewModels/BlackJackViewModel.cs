using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;
using JuegosDeCartas_OpenCampus.Repositories;
using JuegosDeCartas_OpenCampus.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class BlackJackViewModel : BaseViewModel
    {
        private readonly NavigationService  _nav;
        private readonly GameEngineService  _engine  = new();
        private readonly MultiplayerService _session = new();
        private readonly IScoreboardRepository _scoreboard = new ScoreboardRepository();
        private readonly IGameRepository       _gameRepo   = new GameRepository();

        private Players Player => _session.LocalPlayer!;
        private Players Dealer => _session.Dealer;


        public string GameTitle => "♠  B L A C K J A C K";

        public ObservableCollection<Card> PlayerCards { get; } = new();
        public ObservableCollection<Card> DealerCards { get; } = new();

        private int _playerScore;
        public int PlayerScore { get => _playerScore; private set => SetProperty(ref _playerScore, value); }

        private int _dealerScore;
        public int DealerScore { get => _dealerScore; private set => SetProperty(ref _dealerScore, value); }

        private string _statusMessage = string.Empty;
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

        private bool _isRoundOver;
        public bool IsRoundOver { get => _isRoundOver; private set => SetProperty(ref _isRoundOver, value); }

        public ICommand HitCommand      { get; }
        public ICommand StandCommand    { get; }
        public ICommand NewRoundCommand { get; }
        public ICommand ExitCommand     { get; }

        public BlackJackViewModel(NavigationService nav)
        {
            _nav = nav;

            HitCommand      = new RelayCommand(Hit,      () => !IsRoundOver);
            StandCommand    = new RelayCommand(Stand,    () => !IsRoundOver);
            NewRoundCommand = new RelayCommand(StartNewRound);
            ExitCommand     = new RelayCommand(() => _nav.NavigateTo(ViewName.MainMenu, addToHistory: false));

            _session.SetupSinglePlayer("Jugador", "BlackJack");
            StartNewRound();
        }


        private void StartNewRound()
        {
            _engine.ResetDeck();
            _session.ResetAllHands();
            PlayerCards.Clear();
            DealerCards.Clear();
            IsRoundOver    = false;
            StatusMessage  = "Pide carta o plántate.";

            _engine.DealInitialCards(Player, 2);
            _engine.DealInitialCards(Dealer, 2, lastFaceDown: true);

            SyncCards();
            UpdateScores();

            if (Player.HasBlackjack) EndRound();
        }

        private void Hit()
        {
            _engine.DealCard(Player);
            SyncCards();
            UpdateScores();

            if (Player.IsBusted) EndRound();
        }

        private void Stand()
        {
            Player.HasStood = true;
            _engine.PlayDealerTurn(Dealer);
            SyncCards();
            UpdateScores();
            EndRound();
        }

        private void EndRound()
        {

            foreach (var c in Dealer.Hand) c.IsFaceDown = false;
            SyncCards();

            var result = _engine.EvaluateBlackjack(Player, Dealer, "BlackJack");
            _gameRepo.SaveResult(result);


            var entry = _scoreboard.GetByPlayerAndGame(Player.Name, "BlackJack")
                        ?? new ScoreBoardPlayer(Player.Name, "BlackJack");
            entry.RecordResult(result);
            _scoreboard.Update(entry);

            StatusMessage = result.Message;
            IsRoundOver   = true;
            UpdateScores();
        }

        private void SyncCards()
        {
            Sync(Player.Hand, PlayerCards);
            Sync(Dealer.Hand, DealerCards);
        }

        private static void Sync(List<Card> source, ObservableCollection<Card> target)
        {
            target.Clear();
            foreach (var c in source) target.Add(c);
        }

        private void UpdateScores()
        {
            PlayerScore = Player.Score;
            DealerScore = Dealer.Hand.Any(c => !c.IsFaceDown)
                          ? Dealer.Hand.Where(c => !c.IsFaceDown).Sum(c => c.Value)
                          : 0;
        }
    }
}


