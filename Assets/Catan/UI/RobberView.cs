using UnityEngine;
using GameCore.Events;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Attach to a world-space GameObject. Assign the BoardRenderer reference.
    // The GameObject will teleport to the robber's current hex tile on each move.
    // ──────────────────────────────────────────────────────────────────────────

    public class RobberView : MonoBehaviour
    {
        public BoardRenderer BoardRenderer;

        private void OnEnable()
        {
            EventBus.Subscribe<RobberMovedEvent>(OnRobberMoved);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<RobberMovedEvent>(OnRobberMoved);
        }

        private void OnRobberMoved(RobberMovedEvent gameEvent)
        {
            if (BoardRenderer == null) return;
            transform.position = BoardRenderer.GetHexWorldPosition(gameEvent.To);
        }

        public void SnapToCurrentPosition()
        {
            var board = GameManager.Instance?.Board;
            if (board == null || BoardRenderer == null) return;
            transform.position = BoardRenderer.GetHexWorldPosition(board.RobberPosition);
        }
    }
}
