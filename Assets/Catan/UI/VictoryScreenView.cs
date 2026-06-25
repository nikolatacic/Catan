using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GameCore.Events;
using GameCore.Score;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Attach to a UI overlay panel (set inactive by default).
    // Assign WinnerLabel, WinnerColorIndicator, RestartButton.
    // RestartButton's OnClick is wired in code (Awake) — no manual Inspector wiring needed.
    // ──────────────────────────────────────────────────────────────────────────

    public class VictoryScreenView : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI WinnerLabel;
        public Image WinnerColorIndicator;
        public TextMeshProUGUI VictoryPointsLabel;
        public TextMeshProUGUI FinalStandingsLabel;
        public Button RestartButton;

        private void Awake()
        {
            EventBus.Subscribe<VictoryAchievedEvent>(OnVictoryAchieved);
            RestartButton?.onClick.AddListener(OnRestartClicked);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<VictoryAchievedEvent>(OnVictoryAchieved);
        }

        private void OnVictoryAchieved(VictoryAchievedEvent gameEvent)
        {
            gameObject.SetActive(true);

            if (WinnerLabel != null)
                WinnerLabel.text = $"{gameEvent.Winner.DisplayName} wins!";

            if (WinnerColorIndicator != null && gameEvent.Winner is CatanPlayer catanPlayer)
                WinnerColorIndicator.color = catanPlayer.Color;

            var gameManager = GameManager.Instance;
            if (gameManager == null) return;

            if (VictoryPointsLabel != null)
            {
                int score = gameManager.ScoreManager.GetScore(gameEvent.Winner);
                VictoryPointsLabel.text = $"{score} Victory Points";
            }

            if (FinalStandingsLabel != null)
            {
                var standings = gameManager.Players
                    .OrderByDescending(player => gameManager.ScoreManager.GetScore(player))
                    .Select(player => $"{player.DisplayName}: {gameManager.ScoreManager.GetScore(player)} pts");
                FinalStandingsLabel.text = string.Join("\n", standings);
            }
        }

        public void OnRestartClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
