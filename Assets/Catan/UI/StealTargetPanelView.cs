using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.Player;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Place on Canvas (starts inactive). Wire GameManager.StealTargetPanel to
    // this object. Run Catan → Create All UI Prefabs to generate the prefab
    // with a matching PlayerButtonPrefab already wired.
    // ──────────────────────────────────────────────────────────────────────────

    public class StealTargetPanelView : MonoBehaviour
    {
        [Header("Layout")]
        public TextMeshProUGUI TitleLabel;
        public Transform ButtonContainer;

        [Header("Prefab")]
        public GameObject PlayerButtonPrefab;

        private GameCore.Board.HexCoord _pendingCoord;

        private void Awake() => gameObject.SetActive(false);

        public void Show(GameCore.Board.HexCoord coord, List<IPlayer> victims)
        {
            _pendingCoord = coord;

            foreach (Transform child in ButtonContainer)
                Destroy(child.gameObject);

            if (TitleLabel != null)
                TitleLabel.text = "Choose a player to steal from";

            foreach (var victim in victims)
            {
                if (PlayerButtonPrefab == null) break;

                var go = Instantiate(PlayerButtonPrefab, ButtonContainer);
                var image = go.GetComponent<Image>();
                var label = go.GetComponentInChildren<TextMeshProUGUI>();
                var button = go.GetComponent<Button>();

                if (image != null && victim is CatanPlayer catanPlayer)
                    image.color = catanPlayer.Color;

                if (label != null)
                    label.text = victim.DisplayName;

                IPlayer capturedVictim = victim;
                button?.onClick.AddListener(() => OnVictimSelected(capturedVictim));
            }

            gameObject.SetActive(true);
        }

        private void OnVictimSelected(IPlayer victim)
        {
            gameObject.SetActive(false);
            GameManager.Instance?.CompleteRobberMove(_pendingCoord, victim);
        }
    }
}
