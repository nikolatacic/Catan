using UnityEngine;
using GameCore.Events;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Spawned at runtime by BoardRenderer. Uses a CircleCollider2D for clicks.
    // Add a child GameObject "Highlight" with a circle SpriteRenderer and assign
    // it to HighlightRenderer. The highlight shows valid placement spots.
    // ──────────────────────────────────────────────────────────────────────────

    public class VertexView : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer SettlementRenderer;
        public SpriteRenderer CityRenderer;
        public SpriteRenderer HighlightRenderer;

        [Header("Sprites")]
        public Sprite[] PlayerSettlementSprites;
        public Sprite[] PlayerCitySprites;

        public GameCore.Board.HexVertex Vertex { get; private set; }

        private void OnEnable()
        {
            EventBus.Subscribe<PlacementModeChangedEvent>(OnPlacementModeChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlacementModeChangedEvent>(OnPlacementModeChanged);
        }

        public void Initialize(GameCore.Board.HexVertex vertex)
        {
            Vertex = vertex;
            SetHighlight(false, Color.clear);
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

        private void OnPlacementModeChanged(PlacementModeChangedEvent gameEvent)
        {
            UpdateHighlight(gameEvent.Mode);
        }

        private void UpdateHighlight(PlacementMode mode)
        {
            var manager = GameManager.Instance;
            if (manager == null || Vertex == null)
            {
                SetHighlight(false, Color.clear);
                return;
            }

            switch (mode)
            {
                case PlacementMode.Settlement:
                    bool validSettlement = manager.IsValidSettlementSpot(Vertex);
                    var baseColor = manager.ActivePlayer?.Color ?? Color.white;
                    SetHighlight(validSettlement, new Color(baseColor.r, baseColor.g, baseColor.b, 0.6f));
                    break;

                case PlacementMode.City:
                    bool validCity = manager.IsValidCitySpot(Vertex);
                    SetHighlight(validCity, new Color(1f, 0.85f, 0.1f, 0.8f));
                    break;

                default:
                    SetHighlight(false, Color.clear);
                    break;
            }
        }

        private void SetHighlight(bool visible, Color color)
        {
            if (HighlightRenderer == null) return;
            HighlightRenderer.enabled = visible;
            if (visible) HighlightRenderer.color = color;
        }

        public void Refresh()
        {
            if (SettlementRenderer == null) return;

            var board = GameManager.Instance?.Board;
            if (board == null || Vertex == null)
            {
                SettlementRenderer.enabled = false;
                if (CityRenderer != null) CityRenderer.enabled = false;
                return;
            }

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
