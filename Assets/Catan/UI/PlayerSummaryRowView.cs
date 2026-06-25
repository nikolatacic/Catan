using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.Resources;

namespace Catan.UI
{
    public class PlayerSummaryRowView : MonoBehaviour
    {
        [Header("Identity")]
        public Image ColorBar;
        public TextMeshProUGUI NameLabel;
        public Image ActiveIndicator;

        [Header("Stats")]
        public TextMeshProUGUI VPLabel;
        public TextMeshProUGUI CardsLabel;
        public TextMeshProUGUI KnightsLabel;

        [Header("Badges (enable/disable based on ownership)")]
        public GameObject LongestRoadBadge;
        public GameObject LargestArmyBadge;

        public CatanPlayer Player { get; private set; }

        public void Initialize(CatanPlayer player)
        {
            Player = player;
            if (ColorBar != null) ColorBar.color = player.Color;
            Refresh(isActive: false, score: 0);
        }

        public void Refresh(bool isActive, int score)
        {
            if (Player == null) return;

            if (NameLabel != null)
            {
                NameLabel.text      = Player.DisplayName;
                NameLabel.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
            }

            if (ActiveIndicator != null)
                ActiveIndicator.enabled = isActive;

            if (VPLabel != null)
                VPLabel.text = $"{score} VP";

            var resources = Player.Resources?.Current ?? new ResourceBundle();
            int totalCards = resources.Get(CatanResources.Wood)
                           + resources.Get(CatanResources.Brick)
                           + resources.Get(CatanResources.Sheep)
                           + resources.Get(CatanResources.Wheat)
                           + resources.Get(CatanResources.Ore);

            if (CardsLabel != null)
                CardsLabel.text = $"{totalCards} cards";

            if (KnightsLabel != null)
                KnightsLabel.text = $"{Player.KnightsPlayed} knights";

            if (LongestRoadBadge != null) LongestRoadBadge.SetActive(Player.HasLongestRoad);
            if (LargestArmyBadge != null) LargestArmyBadge.SetActive(Player.HasLargestArmy);
        }
    }
}
