using UnityEngine;
using GameCore.Events;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Spawned at runtime by BoardRenderer. Requires a Collider2D for mouse clicks.
    // RobberView is a separate singleton GameObject; this tile only handles clicks.
    // ──────────────────────────────────────────────────────────────────────────

    public class HexTileView : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer TileRenderer;
        public TMPro.TextMeshPro NumberLabel;

        public CatanHexTile Tile { get; private set; }

        public void Initialize(CatanHexTile tile, Sprite tileSprite)
        {
            Tile = tile;

            if (TileRenderer != null)
            {
                TileRenderer.sprite = tileSprite;
                if (tile.Resource is CatanResource catanResource)
                    TileRenderer.color = catanResource.Color;
            }

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
        }

        private void OnMouseDown()
        {
            if (GameManager.Instance == null || Tile == null) return;

            if (GameManager.Instance.CurrentPlacementMode == PlacementMode.MoveRobber)
                GameManager.Instance.TryMoveRobber(Tile.Coord);
        }
    }
}
