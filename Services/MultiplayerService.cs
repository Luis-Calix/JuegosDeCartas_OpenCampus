using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JuegosDeCartas_OpenCampus.Models;

namespace JuegosDeCartas_OpenCampus.Services
{
    public enum GameMode { SinglePlayer, LocalMultiplayer, OnlineMultiplayer }


    public class MultiplayerService
    {

        public GameMode CurrentMode  { get; private set; } = GameMode.SinglePlayer;
        public string   SelectedGame { get; private set; } = string.Empty;

        private readonly List<Players> _humanPlayers = new();
        public IReadOnlyList<Players> HumanPlayers => _humanPlayers.AsReadOnly();
        public Players? LocalPlayer => _humanPlayers.Count > 0 ? _humanPlayers[0] : null;

        public Players Dealer { get; } = new Players("Dealer");


        public void SetupSinglePlayer(string playerName, string gameName)
        {
            CurrentMode  = GameMode.SinglePlayer;
            SelectedGame = gameName;
            _humanPlayers.Clear();
            _humanPlayers.Add(new Players(playerName));
            ResetAllHands();
        }

        public void SetupLocalMultiplayer(IEnumerable<string> playerNames, string gameName)
        {
            CurrentMode  = GameMode.LocalMultiplayer;
            SelectedGame = gameName;
            _humanPlayers.Clear();

            foreach (var name in playerNames.Take(4))
                _humanPlayers.Add(new Players(name));

            ResetAllHands();
        }


        public void SetupOnline(string playerName, string gameName)
        {
            SetupSinglePlayer(playerName, gameName);
        }


        public void ResetAllHands()
        {
            foreach (var p in _humanPlayers) p.ResetHand();
            Dealer.ResetHand();
        }
    }
}

