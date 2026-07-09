using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Catan.Commands;
using GameCore.Player;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class StealTargetPanelView : MonoBehaviour
    {
        private VisualElement _panelRoot;
        private VisualElement _buttonContainer;

        private GameCore.Board.HexCoord _pendingCoord;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _panelRoot        = root.Q<VisualElement>("StealTargetPanelRoot");
            _buttonContainer  = root.Q<VisualElement>("StealButtonContainer");

            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;
        }

        private void Start()
        {
            _panelRoot?.StretchTemplateContainerToFill();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void Show(GameCore.Board.HexCoord coord, List<IPlayer> victims)
        {
            _pendingCoord = coord;

            _buttonContainer?.Clear();

            foreach (var victim in victims)
                _buttonContainer?.Add(BuildPlayerButton(victim));

            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.Flex;
        }

        // ── Builder ────────────────────────────────────────────────────────────

        private Button BuildPlayerButton(IPlayer player)
        {
            var playerButton = new Button();
            playerButton.AddToClassList("steal-player-btn");

            var colorSwatch = new VisualElement();
            colorSwatch.AddToClassList("steal-player-color-swatch");

            if (player is CatanPlayer catanPlayer)
                colorSwatch.style.backgroundColor = new StyleColor(catanPlayer.Color);

            var nameLabel = new Label(player.DisplayName);
            nameLabel.AddToClassList("label--md");

            playerButton.Add(colorSwatch);
            playerButton.Add(nameLabel);

            IPlayer capturedPlayer = player;
            playerButton.RegisterCallback<ClickEvent>(_ => OnVictimSelected(capturedPlayer));

            return playerButton;
        }

        // ── Callbacks ──────────────────────────────────────────────────────────

        private void OnVictimSelected(IPlayer victim)
        {
            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;
            CommandDispatcher.Send(new CompleteRobberMoveCommand { Coord = _pendingCoord, Victim = victim });
        }
    }
}
