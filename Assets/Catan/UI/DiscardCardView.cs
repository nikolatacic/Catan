using System;
using UnityEngine;
using UnityEngine.UIElements;
using GameCore.Resources;

namespace Catan.UI
{
    // Plain C# class — no longer a MonoBehaviour/prefab.
    // Builds its own VisualElement; DiscardPanelView moves Root between containers.
    public class DiscardCardView
    {
        public IResource Resource { get; }
        public VisualElement Root { get; }

        private readonly Action<DiscardCardView> _onClicked;

        public DiscardCardView(IResource resource, Action<DiscardCardView> onClicked)
        {
            Resource   = resource;
            _onClicked = onClicked;

            Root = new VisualElement();
            Root.AddToClassList("discard-card");

            var cardSprite = (resource as CatanResource)?.Icon;
            if (cardSprite != null)
                Root.style.backgroundImage = new StyleBackground(cardSprite);

            Root.RegisterCallback<ClickEvent>(_ => _onClicked?.Invoke(this));
        }
    }
}
