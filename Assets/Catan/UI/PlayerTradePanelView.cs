using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Catan.Commands;
using GameCore.Events;
using GameCore.Resources;
using GameCore.Trade;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class PlayerTradePanelView : MonoBehaviour
    {
        private enum PanelMode { Proposing, Waiting, Responding, Countering }

        private static readonly string[] ResourceElementNames = { "Wood", "Brick", "Sheep", "Wheat", "Ore" };

        private VisualElement _panelRoot;
        private Label _titleLabel;

        private VisualElement _targetSection;
        private VisualElement _targetRow;
        private VisualElement _resourceSection;
        private VisualElement _waitingSection;
        private Label _waitingLabel;
        private VisualElement _respondingSection;
        private Label _offerLabel;
        private Label _feedbackLabel;

        private VisualElement _proposingButtons;
        private VisualElement _waitingButtons;
        private VisualElement _respondingButtons;
        private VisualElement _counteringButtons;

        private readonly Label[] _offerCountLabels = new Label[5];
        private readonly Label[] _wantCountLabels  = new Label[5];

        private readonly int[] _offerCounts = new int[5];
        private readonly int[] _wantCounts  = new int[5];

        private int _selectedTargetIndex = -1;
        private Button[] _targetButtons;
        private ITradeOffer _currentOffer;
        private PanelMode _mode;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _panelRoot = root.Q<VisualElement>("PlayerTradePanelRoot");
            _titleLabel = root.Q<Label>("PlayerTradeTitleLabel");

            _targetSection     = root.Q<VisualElement>("PlayerTradeTargetSection");
            _targetRow         = root.Q<VisualElement>("PlayerTradeTargetRow");
            _resourceSection   = root.Q<VisualElement>("PlayerTradeResourceSection");
            _waitingSection    = root.Q<VisualElement>("PlayerTradeWaitingSection");
            _waitingLabel      = root.Q<Label>("PlayerTradeWaitingLabel");
            _respondingSection = root.Q<VisualElement>("PlayerTradeRespondingSection");
            _offerLabel        = root.Q<Label>("PlayerTradeOfferLabel");
            _feedbackLabel     = root.Q<Label>("PlayerTradeFeedbackLabel");

            _proposingButtons  = root.Q<VisualElement>("PlayerTradeProposingButtons");
            _waitingButtons    = root.Q<VisualElement>("PlayerTradeWaitingButtons");
            _respondingButtons = root.Q<VisualElement>("PlayerTradeRespondingButtons");
            _counteringButtons = root.Q<VisualElement>("PlayerTradeCounteringButtons");

            for (int resourceIndex = 0; resourceIndex < 5; resourceIndex++)
            {
                string resourceName = ResourceElementNames[resourceIndex];
                _offerCountLabels[resourceIndex] = root.Q<Label>($"Offer{resourceName}Count");
                _wantCountLabels[resourceIndex]  = root.Q<Label>($"Want{resourceName}Count");

                int capturedIndex = resourceIndex;
                root.Q<Button>($"Offer{resourceName}Plus")? .RegisterCallback<ClickEvent>(_ => AdjustOffer(capturedIndex, +1));
                root.Q<Button>($"Offer{resourceName}Minus")?.RegisterCallback<ClickEvent>(_ => AdjustOffer(capturedIndex, -1));
                root.Q<Button>($"Want{resourceName}Plus")? .RegisterCallback<ClickEvent>(_ => AdjustWant(capturedIndex, +1));
                root.Q<Button>($"Want{resourceName}Minus")?.RegisterCallback<ClickEvent>(_ => AdjustWant(capturedIndex, -1));
            }

            root.Q<Button>("PlayerTradeProposeBtn")?.      RegisterCallback<ClickEvent>(_ => OnPropose());
            root.Q<Button>("PlayerTradeCancelBtn")?.       RegisterCallback<ClickEvent>(_ => Close());
            root.Q<Button>("PlayerTradeWithdrawBtn")?.     RegisterCallback<ClickEvent>(_ => OnWithdraw());
            root.Q<Button>("PlayerTradeAcceptBtn")?.       RegisterCallback<ClickEvent>(_ => OnAccept());
            root.Q<Button>("PlayerTradeDeclineBtn")?.      RegisterCallback<ClickEvent>(_ => OnDecline());
            root.Q<Button>("PlayerTradeCounterBtn")?.      RegisterCallback<ClickEvent>(_ => OnEnterCountering());
            root.Q<Button>("PlayerTradeCancelCounterBtn")?.RegisterCallback<ClickEvent>(_ => OnCancelCounter());
            root.Q<Button>("PlayerTradeSubmitCounterBtn")?.RegisterCallback<ClickEvent>(_ => OnSubmitCounter());
        }

        private void OnEnable()
        {
            EventBus.Subscribe<TradeProposedEvent>(OnTradeProposed);
            EventBus.Subscribe<TradeAcceptedEvent>(OnTradeAccepted);
            EventBus.Subscribe<TradeRejectedEvent>(OnTradeRejected);
            EventBus.Subscribe<TradeCancelledEvent>(OnTradeCancelled);
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TradeProposedEvent>(OnTradeProposed);
            EventBus.Unsubscribe<TradeAcceptedEvent>(OnTradeAccepted);
            EventBus.Unsubscribe<TradeRejectedEvent>(OnTradeRejected);
            EventBus.Unsubscribe<TradeCancelledEvent>(OnTradeCancelled);
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void Open()
        {
            ResetCounts();
            _selectedTargetIndex = -1;
            _currentOffer = null;
            RebuildTargetButtons();
            SetMode(PanelMode.Proposing);
            SetVisible(_panelRoot, true);
        }

        public void Close()
        {
            SetVisible(_panelRoot, false);
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void OnTradeProposed(TradeProposedEvent gameEvent)
        {
            var offer = gameEvent.Offer;
            if (offer.Target == null) return; // bank trade

            _currentOffer = offer;

            var manager = GameManager.Instance;
            if (manager == null) return;

            int localPlayerIndex = GetLocalPlayerIndex();
            int proposerIndex = manager.Players.IndexOf(offer.Proposer as CatanPlayer);
            int targetIndex   = manager.Players.IndexOf(offer.Target as CatanPlayer);

            if (localPlayerIndex == proposerIndex)
            {
                if (_waitingLabel != null)
                    _waitingLabel.text = $"Waiting for {offer.Target.DisplayName}...";
                SetMode(PanelMode.Waiting);
                SetVisible(_panelRoot, true);
            }
            else if (localPlayerIndex == targetIndex)
            {
                if (_offerLabel != null)
                    _offerLabel.text = BuildOfferSummaryText(offer);
                SetMode(PanelMode.Responding);
                SetVisible(_panelRoot, true);
            }
        }

        private void OnTradeAccepted(TradeAcceptedEvent gameEvent)
        {
            if (gameEvent.Offer.Target == null) return;
            Close();
        }

        private void OnTradeRejected(TradeRejectedEvent gameEvent)
        {
            if (gameEvent.Offer.Target == null) return;
            Close();
        }

        private void OnTradeCancelled(TradeCancelledEvent gameEvent)
        {
            if (gameEvent.Offer.Target == null) return;
            if (_mode == PanelMode.Countering) return; // counter-offer in flight; new TradeProposedEvent will follow
            Close();
        }

        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent)
        {
            Close();
        }

        // ── Button callbacks ───────────────────────────────────────────────────

        private void OnPropose()
        {
            if (_selectedTargetIndex < 0)
            {
                if (_feedbackLabel != null) _feedbackLabel.text = "Select a player to trade with.";
                return;
            }

            var offering   = BuildBundle(_offerCounts);
            var requesting = BuildBundle(_wantCounts);

            CommandDispatcher.Send(new ProposePlayerTradeCommand
            {
                TargetPlayerIndex = _selectedTargetIndex,
                Offering          = offering,
                Requesting        = requesting,
            });
        }

        private void OnWithdraw()
        {
            CommandDispatcher.Send(new CancelPlayerTradeCommand());
            Close();
        }

        private void OnAccept()
        {
            if (_currentOffer == null) return;
            CommandDispatcher.Send(new AcceptPlayerTradeCommand { AcceptingPlayerIndex = GetLocalPlayerIndex() });
            Close();
        }

        private void OnDecline()
        {
            if (_currentOffer == null) return;
            CommandDispatcher.Send(new DeclinePlayerTradeCommand { DecliningPlayerIndex = GetLocalPlayerIndex() });
            Close();
        }

        private void OnEnterCountering()
        {
            if (_currentOffer == null) return;

            for (int resourceIndex = 0; resourceIndex < 5; resourceIndex++)
            {
                var resource = CatanResources.All[resourceIndex];
                _offerCounts[resourceIndex] = _currentOffer.Requesting.Get(resource);
                _wantCounts[resourceIndex]  = _currentOffer.Offering.Get(resource);
            }
            RefreshCountLabels();
            SetMode(PanelMode.Countering);
        }

        private void OnCancelCounter()
        {
            CommandDispatcher.Send(new DeclinePlayerTradeCommand { DecliningPlayerIndex = GetLocalPlayerIndex() });
            Close();
        }

        private void OnSubmitCounter()
        {
            CommandDispatcher.Send(new CounterPlayerTradeCommand
            {
                CounteringPlayerIndex = GetLocalPlayerIndex(),
                CounterOffering       = BuildBundle(_offerCounts),
                CounterRequesting     = BuildBundle(_wantCounts),
            });
        }

        // ── Mode management ────────────────────────────────────────────────────

        private void SetMode(PanelMode mode)
        {
            _mode = mode;

            bool isProposing  = mode == PanelMode.Proposing;
            bool isWaiting    = mode == PanelMode.Waiting;
            bool isResponding = mode == PanelMode.Responding;
            bool isCountering = mode == PanelMode.Countering;

            SetVisible(_targetSection,     isProposing);
            SetVisible(_resourceSection,   isProposing || isCountering);
            SetVisible(_waitingSection,    isWaiting);
            SetVisible(_respondingSection, isResponding);
            SetVisible(_proposingButtons,  isProposing);
            SetVisible(_waitingButtons,    isWaiting);
            SetVisible(_respondingButtons, isResponding);
            SetVisible(_counteringButtons, isCountering);

            if (_titleLabel != null)
            {
                _titleLabel.text = mode switch
                {
                    PanelMode.Proposing  => "PLAYER TRADE",
                    PanelMode.Waiting    => "WAITING...",
                    PanelMode.Responding => "TRADE OFFER",
                    PanelMode.Countering => "COUNTER OFFER",
                    _                    => "TRADE",
                };
            }

            if (_feedbackLabel != null) _feedbackLabel.text = "";
        }

        // ── Target player buttons ──────────────────────────────────────────────

        private void RebuildTargetButtons()
        {
            if (_targetRow == null) return;
            _targetRow.Clear();

            var manager = GameManager.Instance;
            if (manager == null) return;

            int localPlayerIndex = GetLocalPlayerIndex();
            _targetButtons = new Button[manager.Players.Count];

            for (int playerIndex = 0; playerIndex < manager.Players.Count; playerIndex++)
            {
                if (playerIndex == localPlayerIndex) continue;

                var player = manager.Players[playerIndex];
                int capturedIndex = playerIndex;
                var button = new Button(() => OnTargetSelected(capturedIndex))
                {
                    text = player.DisplayName ?? $"Player {playerIndex + 1}"
                };
                button.AddToClassList("btn");
                button.AddToClassList("trade-target-btn");
                _targetRow.Add(button);
                _targetButtons[playerIndex] = button;
            }
        }

        private void OnTargetSelected(int playerIndex)
        {
            _selectedTargetIndex = playerIndex;
            if (_feedbackLabel != null) _feedbackLabel.text = "";

            for (int buttonIndex = 0; buttonIndex < _targetButtons.Length; buttonIndex++)
            {
                var button = _targetButtons[buttonIndex];
                if (button == null) continue;
                if (buttonIndex == playerIndex) button.AddToClassList("btn--selected");
                else button.RemoveFromClassList("btn--selected");
            }
        }

        // ── Resource counters ──────────────────────────────────────────────────

        private void AdjustOffer(int resourceIndex, int delta)
        {
            var activePlayer = GameManager.Instance?.ActivePlayer;
            var resource = CatanResources.All[resourceIndex];
            int ownedCount = activePlayer?.Resources.Current.Get(resource) ?? 0;
            _offerCounts[resourceIndex] = Mathf.Clamp(_offerCounts[resourceIndex] + delta, 0, ownedCount);
            RefreshCountLabels();
        }

        private void AdjustWant(int resourceIndex, int delta)
        {
            _wantCounts[resourceIndex] = Mathf.Max(0, _wantCounts[resourceIndex] + delta);
            RefreshCountLabels();
        }

        private void RefreshCountLabels()
        {
            for (int resourceIndex = 0; resourceIndex < 5; resourceIndex++)
            {
                if (_offerCountLabels[resourceIndex] != null)
                    _offerCountLabels[resourceIndex].text = _offerCounts[resourceIndex].ToString();
                if (_wantCountLabels[resourceIndex] != null)
                    _wantCountLabels[resourceIndex].text = _wantCounts[resourceIndex].ToString();
            }
        }

        private void ResetCounts()
        {
            for (int resourceIndex = 0; resourceIndex < 5; resourceIndex++)
            {
                _offerCounts[resourceIndex] = 0;
                _wantCounts[resourceIndex]  = 0;
            }
            RefreshCountLabels();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static ResourceBundle BuildBundle(int[] counts)
        {
            var bundle = new ResourceBundle();
            var resources = CatanResources.All;
            for (int resourceIndex = 0; resourceIndex < counts.Length; resourceIndex++)
            {
                if (counts[resourceIndex] > 0)
                    bundle = bundle.Add(resources[resourceIndex], counts[resourceIndex]);
            }
            return bundle;
        }

        private static string BuildOfferSummaryText(ITradeOffer offer)
        {
            var offerParts = new List<string>();
            var wantParts  = new List<string>();

            foreach (var resource in CatanResources.All)
            {
                int offerAmount = offer.Offering.Get(resource);
                int wantAmount  = offer.Requesting.Get(resource);
                if (offerAmount > 0) offerParts.Add($"{offerAmount} {resource.DisplayName}");
                if (wantAmount  > 0) wantParts.Add($"{wantAmount} {resource.DisplayName}");
            }

            string offerText = offerParts.Count > 0 ? string.Join(", ", offerParts) : "nothing";
            string wantText  = wantParts.Count  > 0 ? string.Join(", ", wantParts)  : "nothing";
            return $"{offer.Proposer.DisplayName} offers: {offerText}\nfor your: {wantText}";
        }

        private static void SetVisible(VisualElement element, bool isVisible)
        {
            if (element != null)
                element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static int GetLocalPlayerIndex()
        {
            if (NetworkSession.IsNetworked)
                return NetworkSession.LocalPlayerIndex;
            var manager = GameManager.Instance;
            if (manager == null) return -1;
            return manager.Players.IndexOf(manager.ActivePlayer);
        }
    }
}
