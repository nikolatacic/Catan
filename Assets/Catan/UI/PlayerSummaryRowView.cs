using UnityEngine;
using UnityEngine.UIElements;
using GameCore.Resources;

namespace Catan.UI
{
    // No longer a MonoBehaviour — creates and owns a VisualElement row.
    // Instantiated at runtime by AllPlayersSummaryView.
    public class PlayerSummaryRowView
    {
        public CatanPlayer Player { get; }
        public VisualElement Root { get; }

        private readonly VisualElement _colorBar;
        private readonly VisualElement _activeDot;
        private readonly Label _nameLabel;
        private readonly Label _vpLabel;
        private readonly Label _cardsLabel;
        private readonly Label _knightsLabel;
        private readonly Label _longestRoadBadge;
        private readonly Label _largestArmyBadge;

        public PlayerSummaryRowView(CatanPlayer player)
        {
            Player = player;

            Root = new VisualElement();
            Root.AddToClassList("summary-row");

            _colorBar = new VisualElement();
            _colorBar.AddToClassList("summary-row-color-bar");
            _colorBar.style.backgroundColor = new StyleColor(player.Color);
            Root.Add(_colorBar);

            _activeDot = new VisualElement();
            _activeDot.AddToClassList("summary-row-active-dot");
            _activeDot.style.display = DisplayStyle.None;
            Root.Add(_activeDot);

            _nameLabel = new Label(player.DisplayName);
            _nameLabel.AddToClassList("summary-row-name");
            Root.Add(_nameLabel);

            var statsContainer = new VisualElement();
            statsContainer.AddToClassList("summary-row-stats");
            Root.Add(statsContainer);

            _vpLabel = new Label("0 VP");
            _vpLabel.AddToClassList("summary-row-stat");
            statsContainer.Add(_vpLabel);

            _cardsLabel = new Label("0");
            _cardsLabel.AddToClassList("summary-row-stat");
            statsContainer.Add(_cardsLabel);

            _knightsLabel = new Label("0⚔");
            _knightsLabel.AddToClassList("summary-row-stat");
            statsContainer.Add(_knightsLabel);

            _longestRoadBadge = new Label("LR");
            _longestRoadBadge.AddToClassList("summary-row-badge");
            _longestRoadBadge.style.display = DisplayStyle.None;
            statsContainer.Add(_longestRoadBadge);

            _largestArmyBadge = new Label("LA");
            _largestArmyBadge.AddToClassList("summary-row-badge");
            _largestArmyBadge.style.display = DisplayStyle.None;
            statsContainer.Add(_largestArmyBadge);
        }

        public void Refresh(bool isActive, int score)
        {
            if (Player == null) return;

            if (_activeDot != null)
                _activeDot.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;

            if (_nameLabel != null)
            {
                _nameLabel.text = Player.DisplayName;
                if (isActive)
                    _nameLabel.AddToClassList("summary-row-name--active");
                else
                    _nameLabel.RemoveFromClassList("summary-row-name--active");
            }

            if (_vpLabel != null)
                _vpLabel.text = $"{score} VP";

            var currentResources = Player.Resources?.Current ?? new ResourceBundle();
            int totalCardCount = currentResources.Get(CatanResources.Wood)
                               + currentResources.Get(CatanResources.Brick)
                               + currentResources.Get(CatanResources.Sheep)
                               + currentResources.Get(CatanResources.Wheat)
                               + currentResources.Get(CatanResources.Ore);

            if (_cardsLabel != null)
                _cardsLabel.text = $"{totalCardCount}";

            if (_knightsLabel != null)
                _knightsLabel.text = $"{Player.KnightsPlayed}⚔";

            if (_longestRoadBadge != null)
                _longestRoadBadge.style.display = Player.HasLongestRoad ? DisplayStyle.Flex : DisplayStyle.None;

            if (_largestArmyBadge != null)
                _largestArmyBadge.style.display = Player.HasLargestArmy ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
