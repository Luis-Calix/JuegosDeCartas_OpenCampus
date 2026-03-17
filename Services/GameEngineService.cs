using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;

namespace JuegosDeCartas_OpenCampus.Services
{

    public class GameEngineService
    {
        private Deck _deck = new();


        public void ResetDeck()
        {
            _deck = new Deck();
        }

        public Card DealCard(Players player, bool faceDown = false)
        {
            var card = _deck.Deal();
            card.IsFaceDown = faceDown;
            player.AddCard(card);
            return card;
        }

        public void DealInitialCards(Players player, int count = 2, bool lastFaceDown = false)
        {
            for (int i = 0; i < count; i++)
                DealCard(player, faceDown: lastFaceDown && i == count - 1);
        }

        public void PlayDealerTurn(Players dealer)
        {
            // Revelar carta oculta
            foreach (var card in dealer.Hand)
                card.IsFaceDown = false;

            while (dealer.Score < 17)
                DealCard(dealer);
        }

        public GameResult EvaluateBlackjack(Players player, Players dealer, string gameName = "BlackJack")
        {
            // Blackjack natural del jugador
            if (player.HasBlackjack && !dealer.HasBlackjack)
                return new GameResult(ResultType.Blackjack, player.Name,
                                      player.Score, dealer.Score, gameName, pointsEarned: 15);

            // Jugador se pasa
            if (player.IsBusted)
                return new GameResult(ResultType.Bust, player.Name,
                                      player.Score, dealer.Score, gameName);

            // Dealer se pasa
            if (dealer.IsBusted)
                return new GameResult(ResultType.PlayerWin, player.Name,
                                      player.Score, dealer.Score, gameName, pointsEarned: 10);

            // Comparar puntajes
            if (player.Score > dealer.Score)
                return new GameResult(ResultType.PlayerWin, player.Name,
                                      player.Score, dealer.Score, gameName, pointsEarned: 10);

            if (dealer.Score > player.Score)
                return new GameResult(ResultType.DealerWin, dealer.Name,
                                      player.Score, dealer.Score, gameName);

            return new GameResult(ResultType.Tie, string.Empty,
                                  player.Score, dealer.Score, gameName);
        }

        public int CalculateVeintiUnoScore(Players player)
        {
            int total = 0;
            foreach (var card in player.Hand)
            {
                if (card.IsFaceDown) continue;
 
                total += card.Rank == "A" ? 1 : card.Value;
            }
            return total;
        }

        public GameResult EvaluateVeintiUno(Players player, Players dealer)
        {
            int ps = CalculateVeintiUnoScore(player);
            int ds = CalculateVeintiUnoScore(dealer);

            if (ps > 21)
                return new GameResult(ResultType.Bust, player.Name, ps, ds, "Veintiuno");

            if (ds > 21 || ps > ds)
                return new GameResult(ResultType.PlayerWin, player.Name, ps, ds, "Veintiuno", 10);

            if (ds > ps)
                return new GameResult(ResultType.DealerWin, dealer.Name, ps, ds, "Veintiuno");

            return new GameResult(ResultType.Tie, string.Empty, ps, ds, "Veintiuno");
        }

        public Card GetNextPitiparCard() => _deck.Deal();


        /// <param name="currentCard">Carta que el jugador tiene en mano.</param>
        /// <param name="nextCard">Carta que se acaba de revelar.</param>
        /// <param name="prediction">"Pi" (mayor), "Ti" (igual), "Par" (menor).</param>
        public GameResult EvaluatePitipar(Card currentCard, Card nextCard,
                                          string prediction, string playerName)
        {
            bool correct = prediction switch
            {
                "Pi"  => nextCard.Value > currentCard.Value,
                "Ti"  => nextCard.Value == currentCard.Value,
                "Par" => nextCard.Value < currentCard.Value,
                _     => false
            };

            if (correct)
                return new GameResult(ResultType.PlayerWin, playerName,
                                      nextCard.Value, currentCard.Value, "Pitipar", pointsEarned: 10);

            return new GameResult(ResultType.DealerWin, "Casa",
                                  nextCard.Value, currentCard.Value, "Pitipar");
        }
    }
}

