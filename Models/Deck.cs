using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuegosDeCartas_OpenCampus.Models
{
    public class Deck
    {

        private static readonly string[] Suits = { "Corazones", "Diamantes", "Tréboles", "Picas" };

        private static readonly (string Rank, int Value)[] Ranks =
        {
            ("A",  11),
            ("2",   2), ("3",  3), ("4",  4), ("5",  5),
            ("6",   6), ("7",  7), ("8",  8), ("9",  9),
            ("10", 10), ("J", 10), ("Q", 10), ("K", 10)
        };


        private readonly List<Card> _cards = new();
        private readonly Random     _rng   = new();

        public int RemainingCards => _cards.Count;


        public Deck()
        {
            Initialize();
        }

        public void Reset()
        {
            _cards.Clear();
            Initialize();
        }

        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }
        /// <exception cref="InvalidOperationException">Si el mazo está vacío.</exception>
        public Card Deal()
        {
            if (_cards.Count == 0)
                throw new InvalidOperationException("El mazo está vacío.");

            Card card = _cards[^1];
            _cards.RemoveAt(_cards.Count - 1);
            return card;
        }

        // ── Privado ───────────────────────────────────────────────────────────

        private void Initialize()
        {
            foreach (string suit in Suits)
                foreach (var (rank, value) in Ranks)
                    _cards.Add(new Card(suit, rank, value));

            Shuffle();
        }
    }
}
