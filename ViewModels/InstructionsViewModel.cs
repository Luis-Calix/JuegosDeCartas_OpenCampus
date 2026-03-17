using JuegosDeCartas_OpenCampus.Services;
using System.Windows.Input;

namespace JuegosDeCartas_OpenCampus.ViewModels
{
    public class InstructionsViewModel : BaseViewModel
    {
        private readonly NavigationService _nav;
        private readonly ViewName _selectedGame;

        private string _instructionsText = string.Empty;
        public string InstructionsText
        {
            get => _instructionsText;
            set => SetProperty(ref _instructionsText, value);
        }

        public ICommand StartGameCommand { get; }
        public ICommand BackCommand      { get; }

        public InstructionsViewModel(NavigationService nav, ViewName selectedGame)
        {
            _nav          = nav;
            _selectedGame = selectedGame;

            StartGameCommand = new RelayCommand(() =>
                _nav.NavigateTo(_selectedGame, addToHistory: false));

            BackCommand = new RelayCommand(() => _nav.GoBack());

            InstructionsText = _selectedGame switch
            {
                ViewName.BlackJack => GetBlackjackInstructions(),
                ViewName.VeintiUno => GetGuerraInstructions(),
                ViewName.Pitipar   => GetPitiparInstructions(),
                _                  => string.Empty
            };
        }

        private static string GetBlackjackInstructions() =>
            """
            🃏 BLACKJACK
            ──────────────────────────────────────
            Objetivo: conseguir una mano lo más cercana posible a 21 sin pasarse,
            superando al dealer.

            Valores de las cartas:
            • As: 11 puntos (se reduce a 1 si el total supera 21)
            • Figuras (J, Q, K): 10 puntos
            • Resto: su valor numérico

            Cómo jugar:
            1. Al inicio se reparten 2 cartas al jugador y 2 al dealer
               (una del dealer queda boca abajo).
            2. El jugador puede:
               - Pedir Carta: recibir una carta adicional.
               - Plantarse: no recibir más cartas.
            3. El dealer revela su carta oculta y pide cartas hasta llegar a 17+.
            4. Gana quien esté más cerca de 21 sin pasarse.

            Resultados especiales:
            • Blackjack natural: As + figura en 2 cartas → victoria inmediata.
            • Bust: superar 21 → derrota inmediata.
            • Empate: misma puntuación → nadie gana ni pierde.
            """;

        private static string GetGuerraInstructions() =>
            """
            ⚔ GUERRA
            ──────────────────────────────────────
            Objetivo: tener la carta de mayor valor en cada ronda
            y acumular más victorias que el dealer.

            Valores de las cartas:
            • As: 11 puntos (el más alto)
            • Figuras (K, Q, J): 10 puntos
            • Resto: su valor numérico

            Cómo jugar:
            1. Presiona "Robar carta" — ambos reciben una carta al azar.
            2. La carta más alta gana la ronda.
            3. En caso de empate, ¡va a GUERRA! Roba otra carta.
            4. El marcador acumula victorias de cada ronda.

            Estrategia:
            • No hay decisiones — la suerte decide.
            • ¡Sigue jugando para remontar el marcador!
            """;

        private static string GetPitiparInstructions() =>
            """
            ♦ PITIPAR
            ──────────────────────────────────────
            Objetivo: adivinar si la siguiente carta del mazo será
            mayor, igual o menor que la carta actual.

            Cómo jugar:
            1. Se muestra una carta boca arriba.
            2. El jugador elige una de tres opciones:
               - Pi: la siguiente carta será MAYOR.
               - Ti: la siguiente carta será IGUAL.
               - Par: la siguiente carta será MENOR.
            3. Se revela la siguiente carta.
            4. Si acertaste, ganas 10 puntos.
            5. La carta revelada se convierte en la nueva carta actual.

            Puntuación:
            • Acierto → +10 puntos
            • Fallo   → sin puntos
            """;
    }
}
