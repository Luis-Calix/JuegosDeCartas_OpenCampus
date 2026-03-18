using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using JuegosDeCartas_OpenCampus.Models;
using JuegosDeCartas_OpenCampus.Services;
using JuegosDeCartas_OpenCampus.Repositories;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class OnlineBlackJackViewModel : BaseViewModel
    {
        private readonly NavigationService _nav;
        private readonly OnlineMultiplayerService _online;
        private readonly IScoreboardRepository _scoreboard = new ScoreboardRepository();
        private readonly IGameRepository _gameRepo = new GameRepository();

        public string GameTitle => "♠  B L A C K J A C K  —  Online";

        public ObservableCollection<Card> PlayerCards { get; } = new();
        public ObservableCollection<Card> DealerCards { get; } = new();
        public ObservableCollection<Card> OpponentCards { get; } = new();

        private int _playerScore;
        public int PlayerScore { get => _playerScore; private set => SetProperty(ref _playerScore, value); }

        private int _dealerScore;
        public int DealerScore { get => _dealerScore; private set => SetProperty(ref _dealerScore, value); }

        private int _opponentScore;
        public int OpponentScore { get => _opponentScore; private set => SetProperty(ref _opponentScore, value); }

        private string _statusMessage = string.Empty;
        public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

        private bool _isRoundOver;
        public bool IsRoundOver { get => _isRoundOver; private set => SetProperty(ref _isRoundOver, value); }

        private bool _isMyTurn;
        public bool IsMyTurn { get => _isMyTurn; private set => SetProperty(ref _isMyTurn, value); }

        private string _opponentName = "Oponente";
        public string OpponentName { get => _opponentName; private set => SetProperty(ref _opponentName, value); }

        private int _myWins;
        public int MyWins { get => _myWins; private set => SetProperty(ref _myWins, value); }

        private int _opponentWins;
        public int OpponentWins { get => _opponentWins; private set => SetProperty(ref _opponentWins, value); }

        public ICommand HitCommand { get; }
        public ICommand StandCommand { get; }
        public ICommand NewRoundCommand { get; }
        public ICommand ExitCommand { get; }

        public OnlineBlackJackViewModel(NavigationService nav, OnlineMultiplayerService online)
        {
            _nav = nav;
            _online = online;

            HitCommand = new RelayCommand(async () => await HitAsync(), () => IsMyTurn && !IsRoundOver);
            StandCommand = new RelayCommand(async () => await StandAsync(), () => IsMyTurn && !IsRoundOver);
            NewRoundCommand = new RelayCommand(async () => await NewRoundAsync(), () => IsRoundOver && _online.IsHost);
            ExitCommand = new RelayCommand(async () =>
            {
                await _online.DisconnectAsync();
                _nav.NavigateTo(ViewName.MainMenu, addToHistory: false);
            });

            SubscribeToGameEvents();
        }

        public void InitializeFromLobby(
            List<OnlineCard> myCards,
            List<OnlineCard> dealerCards,
            string opponentName,
            bool isMyTurn)
        {
            OpponentName = opponentName;
            IsMyTurn = isMyTurn;
            IsRoundOver = false;

            LoadCards(myCards, PlayerCards);
            LoadDealerCards(dealerCards);
            RecalcScores();

            StatusMessage = isMyTurn
                ? "Tu turno. Pide carta o plántate."
                : $"Turno de {opponentName}.";
        }

        private async Task HitAsync()
        {
            IsMyTurn = false;
            await _online.HitAsync();
        }

        private async Task StandAsync()
        {
            IsMyTurn = false;
            await _online.StandAsync();
        }

        private async Task NewRoundAsync()
        {
            if (!_online.IsHost) return;
            await _online.RequestNewRoundAsync();
        }

        private void SubscribeToGameEvents()
        {
            _online.OnCardReceived += (card, score) =>
                RunOnUI(() =>
                {
                    PlayerCards.Add(ToCard(card));
                    PlayerScore = score;
                    IsMyTurn = true;
                    StatusMessage = "Tu turno.";
                });

            _online.OnOpponentHit += name =>
                RunOnUI(() => StatusMessage = $"{name} pidió carta…");

            _online.OnPlayerStood += name =>
                RunOnUI(() => StatusMessage = $"{name} se plantó.");

            _online.OnBust += (name, score) =>
                RunOnUI(() =>
                {
                    StatusMessage = $"{name} se pasó con {score}!";
                    IsRoundOver = true;
                });

            _online.OnGameResult += (dealerHand, ds, hn, hs, hr, gn, gs, gr) =>
                RunOnUI(() =>
                {
                    LoadDealerCardsFromList(dealerHand);
                    DealerScore = ds;

                    string myResult = _online.IsHost ? hr : gr;
                    string oppResult = _online.IsHost ? gr : hr;

                    if (myResult.Contains("gana") && !myResult.Contains("Dealer")) MyWins++;
                    else if (oppResult.Contains("gana") && !oppResult.Contains("Dealer")) OpponentWins++;

                    StatusMessage = $"{hr} | {gr}";
                    IsRoundOver = true;
                    SaveLocalHistory(hn, hs, gn, gs, ds);
                });

            _online.OnNewRound += () =>
                RunOnUI(() =>
                {
                    PlayerCards.Clear();
                    DealerCards.Clear();
                    OpponentCards.Clear();
                    PlayerScore = 0;
                    DealerScore = 0;
                    OpponentScore = 0;
                    IsRoundOver = false;
                    StatusMessage = "Nueva ronda. Esperando cartas…";
                });

            _online.OnGameStarted += (myCards, dealerCards, oppName, isMyTurn) =>
                RunOnUI(() => InitializeFromLobby(myCards, dealerCards, oppName, isMyTurn));

            _online.OnPlayerLeft += name =>
                RunOnUI(() =>
                {
                    StatusMessage = $"{name} abandonó la partida.";
                    IsRoundOver = true;
                });

            _online.OnError += msg =>
                RunOnUI(() => StatusMessage = $"⚠ {msg}");
        }

        private void LoadCards(List<OnlineCard> source, ObservableCollection<Card> target)
        {
            target.Clear();
            foreach (var c in source) target.Add(ToCard(c));
        }

        private void LoadDealerCards(List<OnlineCard> source)
        {
            DealerCards.Clear();
            for (int i = 0; i < source.Count; i++)
            {
                var c = ToCard(source[i]);
                c.IsFaceDown = (i == source.Count - 1);
                DealerCards.Add(c);
            }
        }

        private void LoadDealerCardsFromList(List<OnlineCard> source)
        {
            DealerCards.Clear();
            foreach (var c in source)
            {
                var card = ToCard(c);
                card.IsFaceDown = false;
                DealerCards.Add(card);
            }
        }

        private void RecalcScores()
        {
            PlayerScore = PlayerCards.Sum(c => c.Value);
            DealerScore = DealerCards.Where(c => !c.IsFaceDown).Sum(c => c.Value);
        }

        // OnlineCard ya NO es OnlineMultiplayerService.OnlineCard
        // es el record independiente definido en OnlineMultiplayerService.cs
        private static Card ToCard(OnlineCard oc) => new Card(oc.Suit, oc.Rank, oc.Value);

        private void SaveLocalHistory(string hn, int hs, string gn, int gs, int ds)
        {
            if (!_online.IsHost) return;
            var result = new GameResult(
                hs > ds && hs <= 21 ? ResultType.PlayerWin : ResultType.DealerWin,
                hs > gs ? hn : gn,
                hs, ds, "BlackJack Online",
                hs > ds && hs <= 21 ? 10 : 0
            );
            _gameRepo.SaveResult(result);
        }

        private static void RunOnUI(Action a)
        {
            if (Application.Current?.Dispatcher != null)
                Application.Current.Dispatcher.Invoke(a);
            else
                a();
        }
    }
}