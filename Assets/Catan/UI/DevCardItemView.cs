using UnityEngine;
using UnityEngine.UIElements;
using Catan.Commands;

namespace Catan.UI
{
    // No longer a MonoBehaviour — creates and owns a VisualElement card item.
    // Instantiated at runtime by DevHandView.
    public class DevCardItemView
    {
        public VisualElement Root { get; }

        private readonly DevelopmentCard _card;

        public DevCardItemView(DevelopmentCard card, bool isPlayable, Sprite cardSprite)
        {
            _card = card;

            Root = new VisualElement();
            Root.AddToClassList("dev-card-item");
            Root.AddToClassList(isPlayable ? "dev-card-item--playable" : "dev-card-item--unplayable");

            var cardIconElement = new VisualElement();
            cardIconElement.AddToClassList("dev-card-icon");
            if (cardSprite != null)
                cardIconElement.style.backgroundImage = new StyleBackground(cardSprite);
            Root.Add(cardIconElement);

            var cardNameLabel = new Label(card.DisplayName);
            cardNameLabel.AddToClassList("dev-card-name");
            Root.Add(cardNameLabel);

            var playButton = new Button(OnPlayClicked) { text = "Play" };
            playButton.AddToClassList("btn");
            playButton.AddToClassList("dev-card-play-btn");
            playButton.SetEnabled(isPlayable);
            Root.Add(playButton);
        }

        private void OnPlayClicked()
        {
            if (_card == null) return;
            CommandDispatcher.Send(new PlayDevCardCommand { Card = _card });
        }
    }
}
