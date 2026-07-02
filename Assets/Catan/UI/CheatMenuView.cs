using UnityEngine;
using UnityEngine.UIElements;
using Catan.Commands;
using GameCore.Resources;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class CheatMenuView : MonoBehaviour
    {
        private VisualElement _cheatPanel;
        private Label _cheatPlayerLabel;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _cheatPanel       = root.Q<VisualElement>("CheatPanel");
            _cheatPlayerLabel = root.Q<Label>("CheatPlayerLabel");

            root.Q<Button>("CheatToggleButton")?.RegisterCallback<ClickEvent>(_ => Toggle());
            root.Q<Button>("CheatAddVPButton")?.RegisterCallback<ClickEvent>(_ => CheatAddVP());
            root.Q<Button>("CheatGiveResourcesButton")?.RegisterCallback<ClickEvent>(_ => CheatGiveAllResources());
            root.Q<Button>("CheatSkipToEndTurnButton")?.RegisterCallback<ClickEvent>(_ => CheatSkipToEndTurn());
        }

        private void Toggle()
        {
            if (_cheatPanel == null) return;
            bool isCurrentlyHidden = _cheatPanel.ClassListContains("hidden");
            if (isCurrentlyHidden)
            {
                _cheatPanel.RemoveFromClassList("hidden");
                RefreshPlayerLabel();
            }
            else
            {
                _cheatPanel.AddToClassList("hidden");
            }
        }

        private void RefreshPlayerLabel()
        {
            if (_cheatPlayerLabel == null) return;
            var activePlayer = GameManager.Instance?.ActivePlayer;
            _cheatPlayerLabel.text = activePlayer != null
                ? $"Active: {activePlayer.DisplayName}"
                : "No active player";
        }

        private void CheatAddVP()
        {
            var manager     = GameManager.Instance;
            var activePlayer = manager?.ActivePlayer;
            if (activePlayer == null) return;

            activePlayer.DevelopmentCards.Add(new VictoryPointCard());
            manager.ScoreManager.RecalculateAll();
            RefreshPlayerLabel();
            Debug.Log($"[CheatMenu] +1 VP → {activePlayer.DisplayName}");
        }

        private void CheatGiveAllResources()
        {
            var activePlayer = GameManager.Instance?.ActivePlayer;
            if (activePlayer == null) return;

            var resourceBundle = new ResourceBundle()
                .Add(CatanResources.Wood,  5)
                .Add(CatanResources.Brick, 5)
                .Add(CatanResources.Sheep, 5)
                .Add(CatanResources.Wheat, 5)
                .Add(CatanResources.Ore,   5);

            activePlayer.Resources.TryAdd(resourceBundle);
            Debug.Log($"[CheatMenu] +5 of each resource → {activePlayer.DisplayName}");
        }

        private void CheatSkipToEndTurn()
        {
            CommandDispatcher.Send(new EndTurnCommand());
            Debug.Log("[CheatMenu] Skipped to end turn");
        }
    }
}
