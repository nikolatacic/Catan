using System.Collections.Generic;
using GameCore.Cards;
using GameCore.Player;

namespace Catan
{
    // Passed to ICard.IsPlayable and ICard.OnPlay so cards can inspect game state
    // and trigger effects without holding direct references to managers.
    public class CatanGameContext : IGameContext
    {
        public IPlayer ActivePlayer { get; }
        public CatanTurnManager TurnManager { get; }
        public CatanBoard Board { get; }
        public IReadOnlyList<IPlayer> AllPlayers { get; }

        public CatanGameContext(
            IPlayer activePlayer,
            CatanTurnManager turnManager,
            CatanBoard board,
            IReadOnlyList<IPlayer> allPlayers)
        {
            ActivePlayer = activePlayer;
            TurnManager = turnManager;
            Board = board;
            AllPlayers = allPlayers;
        }
    }
}
