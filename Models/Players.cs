using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegosDeCartas_OpenCampus.Models
{

    public class Players
    {

        public string Name { get; set; } = string.Empty;

        public List<Card> Hand { get; private set; } = new();

        public int Score => CalculateScore();

        public bool HasStood { get; set; } = false;

        public bool IsBusted => Score > 21;

        public bool HasBlackjack => Hand.Count == 2 && Score == 21;


        public Players(string name)
        {
            Name = name;
        }

        public void AddCard(Card card) => Hand.Add(card);

        public void ResetHand()
        {
            Hand.Clear();
            HasStood = false;
        }


        private int CalculateScore()
        {
            int total = 0;
            int aces  = 0;

            foreach (Card card in Hand)
            {
                if (card.IsFaceDown) continue;

                total += card.Value;
                if (card.Rank == "A") aces++;
            }

            while (total > 21 && aces > 0)
            {
                total -= 10;
                aces--;
            }

            return total;
        }

        public override string ToString() => $"{Name} — {Score} pts ({Hand.Count} cartas)";
    }
}
