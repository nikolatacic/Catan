using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GameCore.Events;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Attach to a UI overlay panel (set inactive by default).
    // Assign WinnerLabel, WinnerColorIndicator, RestartButton.
    // Wire RestartButton.OnClick to OnRestartClicked().
    // ──────────────────────────────────────────────────────────────────────────

    public class VictoryScreenView : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI WinnerLabel;
        public Image WinnerColorIndicator;
        public TextMeshProUGUI VictoryPointsLabel;
        public Button RestartButton;

        private void OnEnable()
        {
            EventBus.Subscribe<VictoryAchievedEvent>(OnVictoryAchieved);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<VictoryAchievedEvent>(OnVictoryAchieved);
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void OnVictoryAchieved(VictoryAchievedEvent gameEvent)
        {
            gameObject.SetActive(true);

            if (WinnerLabel != null)
                WinnerLabel.text = $"{gameEvent.Winner.DisplayName} wins!";

            if (WinnerColorIndicator != null && gameEvent.Winner is CatanPlayer catanPlayer)
                WinnerColorIndicator.color = catanPlayer.Color;

            if (VictoryPointsLabel != null)
            {
                var manager = GameManager.Instance;
                if (manager != null)
                {
                    int score = manager.ScoreManager.GetScore(gameEvent.Winner);
                    VictoryPointsLabel.text = $"{score} Victory Points";
                }
            }
        }

        public void OnRestartClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
