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
        public float HexSize = 1.0f;

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
            transform.position = HexToWorld(gameEvent.To);
        }

        public void SnapToCurrentPosition()
        {
            var board = GameManager.Instance?.Board;
            if (board == null) return;
            transform.position = HexToWorld(board.RobberPosition);
        }

        private Vector3 HexToWorld(GameCore.Board.HexCoord coord)
        {
            float x = HexSize * (Mathf.Sqrt(3) * (coord.Q + coord.R * 0.5f));
            float y = HexSize * (1.5f * coord.R);
            return new Vector3(x, y, 0f);
        }
    }
}
