using UnityEngine;
using UnityEngine.UIElements;
using GameCore.Events;
using GameCore.Resources;
using GameCore.Score;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class PlayerHandView : MonoBehaviour
    {
        private Label _playerNameLabel;
        private Label _totalCardsLabel;
        private Label _victoryPointsLabel;

        private VisualElement _woodIcon;
        private VisualElement _brickIcon;
        private VisualElement _sheepIcon;
        private VisualElement _wheatIcon;
        private VisualElement _oreIcon;

        private Label _woodCountLabel;
        private Label _brickCountLabel;
        private Label _sheepCountLabel;
        private Label _wheatCountLabel;
        private Label _oreCountLabel;

        private CatanPlayer _trackedPlayer;
        private bool _spritesApplied;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _playerNameLabel    = root.Q<Label>("HandPlayerNameLabel");
            _totalCardsLabel    = root.Q<Label>("HandTotalCardsLabel");
            _victoryPointsLabel = root.Q<Label>("HandVPLabel");

            _woodIcon  = root.Q<VisualElement>("WoodIcon");
            _brickIcon = root.Q<VisualElement>("BrickIcon");
            _sheepIcon = root.Q<VisualElement>("SheepIcon");
            _wheatIcon = root.Q<VisualElement>("WheatIcon");
            _oreIcon   = root.Q<VisualElement>("OreIcon");

            _woodCountLabel  = root.Q<Label>("WoodCount");
            _brickCountLabel = root.Q<Label>("BrickCount");
            _sheepCountLabel = root.Q<Label>("SheepCount");
            _wheatCountLabel = root.Q<Label>("WheatCount");
            _oreCountLabel   = root.Q<Label>("OreCount");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ResourceAddedEvent>(OnResourceAdded);
            EventBus.Subscribe<ResourceRemovedEvent>(OnResourceRemoved);
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<LocalPlayerAssignedEvent>(OnLocalPlayerAssigned);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ResourceAddedEvent>(OnResourceAdded);
            EventBus.Unsubscribe<ResourceRemovedEvent>(OnResourceRemoved);
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<LocalPlayerAssignedEvent>(OnLocalPlayerAssigned);
        }

        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent)
        {
            ResolveTrackedPlayer(gameEvent.Actor as CatanPlayer);
            Refresh();
        }

        private void OnLocalPlayerAssigned(LocalPlayerAssignedEvent gameEvent)
        {
            ResolveTrackedPlayer(activePlayer: null);
            Refresh();
        }

        // Hotseat: track whoever's turn it is. Networked: track local player.
        private void ResolveTrackedPlayer(CatanPlayer activePlayer)
        {
            if (Catan.NetworkSession.IsNetworked)
            {
                var manager = GameManager.Instance;
                int localPlayerIndex = Catan.NetworkSession.LocalPlayerIndex;
                if (manager != null && localPlayerIndex >= 0 && localPlayerIndex < manager.Players.Count)
                    _trackedPlayer = manager.Players[localPlayerIndex];
            }
            else
            {
                _trackedPlayer = activePlayer ?? _trackedPlayer;
            }
        }

        private void OnResourceAdded(ResourceAddedEvent gameEvent)
        {
            if (gameEvent.Player == _trackedPlayer) Refresh();
        }

        private void OnResourceRemoved(ResourceRemovedEvent gameEvent)
        {
            if (gameEvent.Player == _trackedPlayer) Refresh();
        }

        private void OnScoreChanged(ScoreChangedEvent gameEvent)
        {
            if (gameEvent.Player == _trackedPlayer) RefreshScore();
        }

        public void Refresh()
        {
            if (_trackedPlayer == null) return;

            if (!_spritesApplied)
            {
                ApplyResourceCardSprites();
                _spritesApplied = true;
            }

            if (_playerNameLabel != null)
                _playerNameLabel.text = _trackedPlayer.DisplayName;

            var currentResources = _trackedPlayer.Resources.Current;

            SetResourceCount(_woodCountLabel,  currentResources.Get(CatanResources.Wood));
            SetResourceCount(_brickCountLabel, currentResources.Get(CatanResources.Brick));
            SetResourceCount(_sheepCountLabel, currentResources.Get(CatanResources.Sheep));
            SetResourceCount(_wheatCountLabel, currentResources.Get(CatanResources.Wheat));
            SetResourceCount(_oreCountLabel,   currentResources.Get(CatanResources.Ore));

            int totalCardCount = currentResources.Get(CatanResources.Wood)
                               + currentResources.Get(CatanResources.Brick)
                               + currentResources.Get(CatanResources.Sheep)
                               + currentResources.Get(CatanResources.Wheat)
                               + currentResources.Get(CatanResources.Ore);

            if (_totalCardsLabel != null)
                _totalCardsLabel.text = $"{totalCardCount} cards";

            RefreshScore();
        }

        private void RefreshScore()
        {
            if (_trackedPlayer == null || _victoryPointsLabel == null) return;
            var manager = GameManager.Instance;
            if (manager == null) return;
            _victoryPointsLabel.text = $"{manager.ScoreManager.GetScore(_trackedPlayer)} VP";
        }

        // Sets the card art from each CatanResource ScriptableObject's Icon sprite.
        // Only called once — sprites don't change between turns.
        private void ApplyResourceCardSprites()
        {
            ApplySprite(_woodIcon,  CatanResources.Wood?.Icon);
            ApplySprite(_brickIcon, CatanResources.Brick?.Icon);
            ApplySprite(_sheepIcon, CatanResources.Sheep?.Icon);
            ApplySprite(_wheatIcon, CatanResources.Wheat?.Icon);
            ApplySprite(_oreIcon,   CatanResources.Ore?.Icon);
        }

        private static void ApplySprite(VisualElement iconElement, UnityEngine.Sprite sprite)
        {
            if (iconElement == null || sprite == null) return;
            iconElement.style.backgroundImage = new StyleBackground(sprite);
        }

        private static void SetResourceCount(Label countLabel, int count)
        {
            if (countLabel != null)
                countLabel.text = count.ToString();
        }
    }
}
