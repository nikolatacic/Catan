using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Catan.Commands;

namespace Catan.UI
{
    // ── Prefab setup ───────────────────────────────────────────────────────────
    // Spawned at runtime by DevHandView. Wire CardBackground (Image),
    // NameLabel (TMP), PlayButton (Button). PlayButton's OnClick is wired in code.
    // ──────────────────────────────────────────────────────────────────────────

    public class DevCardItemView : MonoBehaviour
    {
        [Header("Visuals")]
        public Image CardBackground;
        public TextMeshProUGUI NameLabel;
        public Button PlayButton;

        [Header("Colors")]
        public Color PlayableColor   = new Color(0.20f, 0.55f, 0.20f);
        public Color UnplayableColor = new Color(0.30f, 0.30f, 0.30f);

        private DevelopmentCard _card;

        public void Initialize(DevelopmentCard card, bool isPlayable)
        {
            _card = card;

            if (NameLabel != null)
                NameLabel.text = card.DisplayName;

            if (CardBackground != null)
                CardBackground.color = isPlayable ? PlayableColor : UnplayableColor;

            if (PlayButton != null)
            {
                PlayButton.interactable = isPlayable;
                PlayButton.onClick.AddListener(OnPlayClicked);
            }
        }

        private void OnDestroy()
        {
            PlayButton?.onClick.RemoveListener(OnPlayClicked);
        }

        private void OnPlayClicked()
        {
            if (_card == null) return;
            CommandDispatcher.Send(new PlayDevCardCommand { Card = _card });
        }
    }
}
