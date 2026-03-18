using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegosDeCartas_OpenCampus.Models
{

    public class Card
    {
 
        /// <summary>
        /// Handles suits.
        /// </summary>
        public string Suit { get; set; } = string.Empty;

        public string Rank { get; set; } = string.Empty;

        public int Value { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public bool IsFaceDown { get; set; } = false;


        public Card(string suit, string rank, int value)
        {
            Suit      = suit;
            Rank      = rank;
            Value     = 10;
            ImagePath = BuildImagePath(suit, rank);
        }

        private static string BuildImagePath(string suit, string rank)
            => $"/Assets/Cards/{rank}{suit[0]}.png";

        public override string ToString() => $"{Rank} de {Suit} ({Value})";
    }
}
