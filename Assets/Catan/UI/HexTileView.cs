using System;
using System.Collections.Generic;
using UnityEngine;
using Catan.Commands;
using GameCore.Events;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Spawned at runtime by BoardRenderer. Requires a Collider2D for mouse clicks.
    // RobberView is a separate singleton GameObject; this tile only handles clicks.
    // Assign NumberSprites in the Inspector — one entry per dice number (2-12, no 7).
    // ──────────────────────────────────────────────────────────────────────────

    public class HexTileView : MonoBehaviour
    {
        [Serializable]
        public struct DiceNumberSprite
        {
            public int DiceNumber;
            public Sprite Sprite;
        }

        [Header("Renderers")]
        public SpriteRenderer HexBackgroundRenderer;
        public SpriteRenderer NumberRenderer;

        [Header("Number sprites (one per dice number, 2-12)")]
        public List<DiceNumberSprite> NumberSprites = new();

        public CatanHexTile Tile { get; private set; }

        public void Initialize(CatanHexTile tile, Sprite tileSprite)
        {
            Tile = tile;

            if (HexBackgroundRenderer != null)
                HexBackgroundRenderer.sprite = tileSprite;

            if (NumberRenderer != null)
            {
                var numberSprite = tile.DiceNumber > 0 ? GetNumberSprite(tile.DiceNumber) : null;
                NumberRenderer.sprite = numberSprite;
                NumberRenderer.gameObject.SetActive(numberSprite != null);
            }
        }

        private Sprite GetNumberSprite(int diceNumber)
        {
            foreach (var entry in NumberSprites)
            {
                if (entry.DiceNumber == diceNumber)
                    return entry.Sprite;
            }
            return null;
        }

        private void OnMouseDown()
        {
            if (GameManager.Instance == null || Tile == null) return;

            if (GameManager.Instance.CurrentPlacementMode == PlacementMode.MoveRobber)
                CommandDispatcher.Send(new MoveRobberCommand { Coord = Tile.Coord });
        }
    }
}
