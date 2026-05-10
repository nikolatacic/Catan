using System.Collections.Generic;
using UnityEngine;
using GameCore.Cards;
using GameCore.Player;
using GameCore.Resources;
using GameCore.Turn;

namespace Catan
{
    public class CatanPlayer : IPlayer, ITurnActor
    {
        public string Id { get; }
        public string DisplayName { get; }
        public Color Color { get; }

        public ResourceInventory Resources { get; }
        public CardHand<DevelopmentCard> DevelopmentCards { get; } = new();

        public List<Settlement> Settlements { get; } = new();
        public List<Road> Roads { get; } = new();

        public int KnightsPlayed { get; internal set; }
        public bool HasLargestArmy { get; internal set; }
        public bool HasLongestRoad { get; internal set; }

        public const int MaxSettlements = 5;
        public const int MaxCities = 4;
        public const int MaxRoads = 15;

        public bool CanAct { get; internal set; }

        public CatanPlayer(string id, string displayName, Color color)
        {
            Id = id;
            DisplayName = displayName;
            Color = color;
            Resources = new ResourceInventory(this);
        }

        public bool CanBuild(GameCore.Build.IPlaceable piece)
            => Resources.CanAfford(piece.BuildCost) && HasPiecesRemaining(piece);

        private bool HasPiecesRemaining(GameCore.Build.IPlaceable piece)
            => throw new System.NotImplementedException();
    }
}
