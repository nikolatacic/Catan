using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.Resources;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Drop the CheatMenu prefab anywhere inside your Canvas. All button
    // listeners are wired in Awake — no Inspector OnClick setup needed.
    // The panel starts hidden; click the TEST button to show/hide it.
    // ──────────────────────────────────────────────────────────────────────────

    public class CheatMenuView : MonoBehaviour
    {
        [Header("Structure")]
        public GameObject CheatPanel;
        public Button ToggleButton;
        public TextMeshProUGUI PlayerLabel;

        [Header("Cheat buttons")]
        public Button AddVPButton;
        public Button GiveAllResourcesButton;
        public Button SkipToEndTurnButton;

        private void Awake()
        {
            if (CheatPanel != null) CheatPanel.SetActive(false);
            if (ToggleButton != null)            ToggleButton.onClick.AddListener(Toggle);
            if (AddVPButton != null)             AddVPButton.onClick.AddListener(CheatAddVP);
            if (GiveAllResourcesButton != null)  GiveAllResourcesButton.onClick.AddListener(CheatGiveAllResources);
            if (SkipToEndTurnButton != null)     SkipToEndTurnButton.onClick.AddListener(CheatSkipToEndTurn);
        }

        private void OnDestroy()
        {
            if (ToggleButton != null)           ToggleButton.onClick.RemoveListener(Toggle);
            if (AddVPButton != null)            AddVPButton.onClick.RemoveListener(CheatAddVP);
            if (GiveAllResourcesButton != null) GiveAllResourcesButton.onClick.RemoveListener(CheatGiveAllResources);
            if (SkipToEndTurnButton != null)    SkipToEndTurnButton.onClick.RemoveListener(CheatSkipToEndTurn);
        }

        // ── Toggle ─────────────────────────────────────────────────────────────

        public void Toggle()
        {
            if (CheatPanel == null) return;
            bool opening = !CheatPanel.activeSelf;
            CheatPanel.SetActive(opening);
            if (opening) RefreshPlayerLabel();
        }

        private void RefreshPlayerLabel()
        {
            if (PlayerLabel == null) return;
            var player = GameManager.Instance?.ActivePlayer;
            PlayerLabel.text = player != null
                ? $"Active: {player.DisplayName}"
                : "No active player";
        }

        // ── Cheats ─────────────────────────────────────────────────────────────

        private void CheatAddVP()
        {
            var manager = GameManager.Instance;
            var player  = manager?.ActivePlayer;
            if (player == null) return;

            player.DevelopmentCards.Add(new VictoryPointCard());
            manager.ScoreManager.RecalculateAll();
            RefreshPlayerLabel();
            Debug.Log($"[CheatMenu] +1 VP → {player.DisplayName}");
        }

        private void CheatGiveAllResources()
        {
            var manager = GameManager.Instance;
            var player  = manager?.ActivePlayer;
            if (player == null) return;

            var bundle = new ResourceBundle()
                .Add(CatanResources.Wood,  5)
                .Add(CatanResources.Brick, 5)
                .Add(CatanResources.Sheep, 5)
                .Add(CatanResources.Wheat, 5)
                .Add(CatanResources.Ore,   5);

            player.Resources.TryAdd(bundle);
            Debug.Log($"[CheatMenu] +5 of each resource → {player.DisplayName}");
        }

        private void CheatSkipToEndTurn()
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            manager.EndTurn();
            Debug.Log("[CheatMenu] Skipped to end turn");
        }
    }
}
