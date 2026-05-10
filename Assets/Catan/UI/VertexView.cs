using UnityEngine;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Spawned at runtime by BoardRenderer. Uses a CircleCollider2D for clicks.
    // ──────────────────────────────────────────────────────────────────────────

    public class VertexView : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer SettlementRenderer;
        public SpriteRenderer CityRenderer;

        [Header("Sprites")]
        public Sprite[] PlayerSettlementSprites;
        public Sprite[] PlayerCitySprites;

        public GameCore.Board.HexVertex Vertex { get; private set; }

        public void Initialize(GameCore.Board.HexVertex vertex)
        {
            Vertex = vertex;
            Refresh();
        }

        private void OnMouseDown()
        {
            if (GameManager.Instance == null) return;

            var mode = GameManager.Instance.CurrentPlacementMode;
            if (mode == PlacementMode.Settlement)
                GameManager.Instance.TryPlaceSettlement(Vertex);
            else if (mode == PlacementMode.City)
                GameManager.Instance.TryUpgradeCity(Vertex);

            Refresh();
        }

        public void Refresh()
        {
            if (SettlementRenderer == null) return;

            var board = GameManager.Instance?.Board;
            if (board == null || Vertex == null) return;

            if (!board.Settlements.TryGetValue(Vertex, out var settlement))
            {
                SettlementRenderer.enabled = false;
                if (CityRenderer != null) CityRenderer.enabled = false;
                return;
            }

            int playerIndex = GetPlayerIndex(settlement.Owner);
            bool isCity = settlement.IsCity;

            if (isCity)
            {
                SettlementRenderer.enabled = false;
                if (CityRenderer != null)
                {
                    CityRenderer.enabled = true;
                    if (playerIndex >= 0 && PlayerCitySprites != null
                        && playerIndex < PlayerCitySprites.Length)
                        CityRenderer.sprite = PlayerCitySprites[playerIndex];

                    if (settlement.Owner is CatanPlayer catanPlayer)
                        CityRenderer.color = catanPlayer.Color;
                }
            }
            else
            {
                SettlementRenderer.enabled = true;
                if (playerIndex >= 0 && PlayerSettlementSprites != null
                    && playerIndex < PlayerSettlementSprites.Length)
                    SettlementRenderer.sprite = PlayerSettlementSprites[playerIndex];

                if (settlement.Owner is CatanPlayer catanPlayer)
                    SettlementRenderer.color = catanPlayer.Color;

                if (CityRenderer != null) CityRenderer.enabled = false;
            }
        }

        private static int GetPlayerIndex(GameCore.Player.IPlayer player)
        {
            if (GameManager.Instance == null) return -1;
            return GameManager.Instance.Players.FindIndex(catanPlayer => catanPlayer == player);
        }
    }
}
