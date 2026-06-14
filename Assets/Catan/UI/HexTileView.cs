using UnityEngine;
using GameCore.Events;
using TMPro;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Spawned at runtime by BoardRenderer. No manual scene placement needed.
    // ──────────────────────────────────────────────────────────────────────────

    public class HexTileView : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer TileRenderer;
        public SpriteRenderer RobberIconRenderer;
        public TextMeshProUGUI NumberLabel;

        public CatanHexTile Tile { get; private set; }

        public void Initialize(CatanHexTile tile, Sprite tileSprite)
        {
            Tile = tile;

            if (TileRenderer != null)
                TileRenderer.sprite = tileSprite;

            if (NumberLabel != null)
            {
                if (tile.DiceNumber > 0)
                {
                    NumberLabel.text = tile.DiceNumber.ToString();
                    NumberLabel.color = (tile.DiceNumber == 6 || tile.DiceNumber == 8)
                        ? Color.red
                        : Color.black;
                }
                else
                {
                    NumberLabel.gameObject.SetActive(false);
                }
            }

            RefreshRobber();
        }

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
            RefreshRobber();
        }

        private void RefreshRobber()
        {
            if (RobberIconRenderer == null || Tile == null) return;

            var board = GameManager.Instance?.Board;
            if (board == null) return;

            RobberIconRenderer.enabled = board.RobberPosition.Equals(Tile.Coord);
        }
    }
}
