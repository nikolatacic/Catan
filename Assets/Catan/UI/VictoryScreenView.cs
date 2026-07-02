using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GameCore.Events;
using GameCore.Score;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class VictoryScreenView : MonoBehaviour
    {
        private VisualElement _screenRoot;
        private VisualElement _colorBar;
        private Label         _winnerLabel;
        private Label         _pointsLabel;
        private Label         _standingsLabel;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _screenRoot     = root.Q<VisualElement>("VictoryScreenRoot");
            _colorBar       = root.Q<VisualElement>("VictoryColorBar");
            _winnerLabel    = root.Q<Label>("VictoryWinnerLabel");
            _pointsLabel    = root.Q<Label>("VictoryPointsLabel");
            _standingsLabel = root.Q<Label>("VictoryStandingsLabel");

            root.Q<Button>("VictoryRestartButton")?.RegisterCallback<ClickEvent>(_ => OnRestartClicked());

            if (_screenRoot != null) _screenRoot.style.display = DisplayStyle.None;

            EventBus.Subscribe<VictoryAchievedEvent>(OnVictoryAchieved);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<VictoryAchievedEvent>(OnVictoryAchieved);
        }

        // ── Event handling ─────────────────────────────────────────────────────

        private void OnVictoryAchieved(VictoryAchievedEvent gameEvent)
        {
            if (_screenRoot != null) _screenRoot.style.display = DisplayStyle.Flex;

            if (_winnerLabel != null)
                _winnerLabel.text = $"{gameEvent.Winner.DisplayName} wins!";

            if (_colorBar != null && gameEvent.Winner is CatanPlayer catanPlayer)
                _colorBar.style.backgroundColor = new StyleColor(catanPlayer.Color);

            var gameManager = GameManager.Instance;
            if (gameManager == null) return;

            if (_pointsLabel != null)
            {
                int winnerScore = gameManager.ScoreManager.GetScore(gameEvent.Winner);
                _pointsLabel.text = $"{winnerScore} Victory Points";
            }

            if (_standingsLabel != null)
            {
                var standingsLines = gameManager.Players
                    .OrderByDescending(player => gameManager.ScoreManager.GetScore(player))
                    .Select(player => $"{player.DisplayName}: {gameManager.ScoreManager.GetScore(player)} pts");
                _standingsLabel.text = string.Join("\n", standingsLines);
            }
        }

        // ── Callbacks ──────────────────────────────────────────────────────────

        private void OnRestartClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
