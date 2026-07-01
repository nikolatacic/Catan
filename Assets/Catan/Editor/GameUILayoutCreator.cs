using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Catan.UI.Editor
{
    /// <summary>
    /// Positions every UI panel in GameHotseat.unity for a clean Catan-like layout.
    /// Run via  Catan ▸ Setup Game UI Layout.
    ///
    /// Reference resolution: 1920 × 1080.
    ///
    /// ┌─────────────────────────────────────┬─────────────┐
    /// │  TurnPanel (top, ~83 % width)       │  CheatMenu  │
    /// ├─────────────────────────────────────┤             │
    /// │                                     │ SummaryPanel│
    /// │     BOARD  (camera viewport 0–0.84) │             │
    /// │                                     │ DevHandPanel│
    /// ├───────────────────┬─────────────────┤             │
    /// │  PlayerHandPanel  │  ActionBar      │             │
    /// └───────────────────┴─────────────────┴─────────────┘
    ///
    /// IMPORTANT — Transform.Find() skips inactive GameObjects, exactly like
    /// GameObject.Find(). All child lookups use GetChild(index) iteration instead,
    /// which works regardless of active state.
    ///
    /// ACTIVE STATE RULES
    /// ──────────────────
    /// ActionPanel:     MUST stay active — it hosts ActionButtonsView whose OnEnable()
    ///                  subscribes to turn/phase events and drives button interactability.
    ///
    /// PlayerHandPanel: MUST stay active — it hosts PlayerHandView whose OnEnable()
    ///                  subscribes to resource/turn events and shows card counts.
    ///
    /// ActivePlayerPanel: deactivated — simplified name/VP strip superseded by
    ///                    PlayerHandPanel which also shows resource card slots.
    ///
    /// Modals (BankTradePanel, DiscardPanel, StealTargetPanel, VictoryScreen):
    ///                  MUST start active in the scene so their Awake() runs at scene
    ///                  load. Each modal's Awake() wires button listeners then calls
    ///                  gameObject.SetActive(false) to hide itself. If a stale
    ///                  m_IsActive=0 scene override exists (from a previous layout run),
    ///                  Awake() is never called — so when Open() activates the panel at
    ///                  runtime, Awake() fires for the first time and immediately
    ///                  deactivates it again, making the modal invisible forever.
    /// </summary>
    public static class GameUILayoutCreator
    {
        // ── Reference constants ────────────────────────────────────────────────
        private const float W      = 1920f;
        private const float H      = 1080f;
        private const float Side   = 310f;
        private const float Bottom = 130f;
        private const float Top    = 90f;
        private const float Hand   = 500f;
        private const float Gap    = 12f;

        private const float RightColFrac = (W - Side) / W;   // ≈ 0.839

        // ── Entry point ────────────────────────────────────────────────────────

        [MenuItem("Catan/Setup Game UI Layout")]
        public static void SetupLayout()
        {
            var canvas = FindCanvas();
            if (canvas == null)
            {
                Debug.LogError("[Catan] No Canvas found. Open GameHotseat scene first.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Catan UI Layout");

            FixCanvasScaler(canvas);
            FixMainCameraViewport();
            RemoveScriptAddedCanvasComponents(canvas);
            FixPanelActiveStates(canvas);
            PositionAllPanels(canvas);
            MoveModalsToTopOfSiblingOrder(canvas);

            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Debug.Log("[Catan] Game UI layout applied. Press Ctrl+S to save.");
        }

        // ── Canvas scaler ──────────────────────────────────────────────────────

        private static void FixCanvasScaler(Canvas canvas)
        {
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(W, H);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;
        }

        // ── Board camera viewport ──────────────────────────────────────────────

        private static void FixMainCameraViewport()
        {
            var mainCamera = GameObject.Find("MainCamera")?.GetComponent<Camera>();
            if (mainCamera == null)
            {
                Debug.LogWarning("[Catan] MainCamera not found.");
                return;
            }
            mainCamera.rect = new Rect(0f, 0f, RightColFrac, 1f);
        }

        // ── Remove Canvas components added by a previous layout run ────────────
        // A previous version of this script called EnsureOverlayCanvas() which
        // added a Canvas + GraphicRaycaster to modals. Those Canvas components were
        // saved with RenderMode=WorldSpace and a mismatched SortingLayerID, causing
        // the modal to render into the wrong layer and appear invisible.
        // Remove any Canvas/GraphicRaycaster that has no corresponding prefab source.

        private static void RemoveScriptAddedCanvasComponents(Canvas rootCanvas)
        {
            string[] panelNames = { "BankTradePanel", "DiscardPanel", "StealTargetPanel", "VictoryScreen" };
            foreach (string panelName in panelNames)
            {
                var panelGo = GetDirectChildByName(rootCanvas.transform, panelName);
                if (panelGo == null) continue;

                RemoveNonPrefabComponent<Canvas>(panelGo);
                RemoveNonPrefabComponent<GraphicRaycaster>(panelGo);
            }
        }

        private static void RemoveNonPrefabComponent<T>(GameObject go) where T : Component
        {
            foreach (var component in go.GetComponents<T>())
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(component) == null)
                    Undo.DestroyObjectImmediate(component);
            }
        }

        // ── Active state correction ────────────────────────────────────────────
        // Uses GetChild(index) iteration — the only reliable way to find children
        // regardless of their active state. Transform.Find and GameObject.Find both
        // skip inactive GameObjects.

        private static void FixPanelActiveStates(Canvas canvas)
        {
            SetDirectChildActive(canvas.transform, "ActivePlayerPanel",  false);
            SetDirectChildActive(canvas.transform, "ActionPanel",        true);
            SetDirectChildActive(canvas.transform, "PlayerHandPanel",    true);
            SetDirectChildActive(canvas.transform, "BankTradePanel",     true);
            SetDirectChildActive(canvas.transform, "DiscardPanel",        true);
            SetDirectChildActive(canvas.transform, "StealTargetPanel",    true);
            SetDirectChildActive(canvas.transform, "VictoryScreen",       true);
            // Dev card modals — silently skipped if not yet added to the scene.
            SetDirectChildActive(canvas.transform, "MonopolyPanel",     true,  warnIfMissing: false);
            SetDirectChildActive(canvas.transform, "YearOfPlentyPanel", true,  warnIfMissing: false);
        }

        // ── Panel positioning ──────────────────────────────────────────────────

        private static void PositionAllPanels(Canvas canvas)
        {
            // Panels are all active now (FixPanelActiveStates just ran), so
            // FindByName<T> / GameObject.Find work correctly from here on.

            var turnPanel = FindByName<TurnIndicatorView>("TurnPanel");
            if (turnPanel != null)
            {
                Anchor(turnPanel,
                    min: V2(0, 1), max: V2(RightColFrac, 1), pivot: V2(0, 1),
                    size: V2(0, Top), pos: V2(0, 0));
                FixTurnPanelElementHolder(turnPanel.gameObject);
            }

            var handPanel = FindByName<PlayerHandView>("PlayerHandPanel");
            if (handPanel != null)
            {
                DisableContentSizeFitter(handPanel.gameObject);
                Anchor(handPanel,
                    min: V2(0, 0), max: V2(0, 0), pivot: V2(0, 0),
                    size: V2(Hand, Bottom), pos: V2(Gap, Gap));
            }

            var actionBar = FindByName<ActionButtonsView>("ActionBar");
            if (actionBar != null)
            {
                float actionBarWidth = W - Hand - Gap * 3 - Side;
                Anchor(actionBar,
                    min: V2(0, 0), max: V2(0, 0), pivot: V2(0, 0),
                    size: V2(actionBarWidth, Bottom), pos: V2(Hand + Gap * 2, Gap));
                FixActionBarButtons(actionBar.gameObject);
            }

            var summaryPanel = FindByName<AllPlayersSummaryView>("SummaryPanel");
            if (summaryPanel != null)
            {
                DisableContentSizeFitter(summaryPanel.gameObject);
                Anchor(summaryPanel,
                    min: V2(RightColFrac, 1), max: V2(1, 1), pivot: V2(0, 1),
                    size: V2(0, 460), pos: V2(0, -Top - Gap));
            }

            var devHand = FindByName<DevHandView>("DevHandPanel");
            if (devHand != null)
            {
                DisableContentSizeFitter(devHand.gameObject);
                Anchor(devHand,
                    min: V2(RightColFrac, 0), max: V2(1, 0), pivot: V2(0, 0),
                    size: V2(0, 360), pos: V2(0, Bottom + Gap * 2));
            }

            // BankTradePanel root is already full-screen in the prefab.
            // Do NOT change layout here — its Awake() manages active state.
            // Rendering order is handled by MoveModalsToTopOfSiblingOrder.

            var discard = FindByName<DiscardPanelView>("DiscardPanel");
            if (discard != null)
                StretchFull(discard.gameObject);

            var steal = FindByName<StealTargetPanelView>("StealTargetPanel");
            if (steal != null)
                StretchFull(steal.gameObject);

            var victory = FindByName<VictoryScreenView>("VictoryScreen");
            if (victory != null)
                StretchFull(victory.gameObject);

            var cheat = FindByName<CheatMenuView>("CheatMenu");
            if (cheat != null)
            {
                Anchor(cheat,
                    min: V2(1, 1), max: V2(1, 1), pivot: V2(1, 1),
                    size: V2(210f, 170f), pos: V2(-Gap, -Gap));
                cheat.gameObject.SetActive(false);
            }
        }

        // ── TurnPanel ElementHolder fix ────────────────────────────────────────

        private static void FixTurnPanelElementHolder(GameObject turnPanelGo)
        {
            // TurnPanel is active so Transform.Find works here.
            var elementHolderTransform = turnPanelGo.transform.Find("ElementHolder");
            if (elementHolderTransform == null)
            {
                Debug.LogWarning("[Catan] TurnPanel/ElementHolder not found.");
                return;
            }

            var elementHolderRect = elementHolderTransform.GetComponent<RectTransform>();
            if (elementHolderRect == null) return;

            elementHolderRect.anchorMin        = Vector2.zero;
            elementHolderRect.anchorMax        = Vector2.one;
            elementHolderRect.pivot            = new Vector2(0.5f, 0.5f);
            elementHolderRect.offsetMin        = Vector2.zero;
            elementHolderRect.offsetMax        = Vector2.zero;
        }

        // ── Sibling order ──────────────────────────────────────────────────────
        // Last sibling in a Screen Space canvas renders on top. Uses GetChild
        // iteration because modals may be inactive during this pass.

        private static void MoveModalsToTopOfSiblingOrder(Canvas canvas)
        {
            MoveDirectChildToLastSibling(canvas.transform, "BankTradePanel");
            MoveDirectChildToLastSibling(canvas.transform, "DiscardPanel");
            MoveDirectChildToLastSibling(canvas.transform, "StealTargetPanel");
            MoveDirectChildToLastSibling(canvas.transform, "MonopolyPanel",     warnIfMissing: false);
            MoveDirectChildToLastSibling(canvas.transform, "YearOfPlentyPanel", warnIfMissing: false);
            MoveDirectChildToLastSibling(canvas.transform, "VictoryScreen");
        }

        private static void MoveDirectChildToLastSibling(Transform parentTransform, string childName,
            bool warnIfMissing = true)
        {
            var childGo = GetDirectChildByName(parentTransform, childName, warnIfMissing);
            if (childGo != null) childGo.transform.SetAsLastSibling();
        }

        // ── ContentSizeFitter ──────────────────────────────────────────────────

        private static void DisableContentSizeFitter(GameObject go)
        {
            var contentSizeFitter = go.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null) contentSizeFitter.enabled = false;
        }

        // ── ActionBar button height fix ────────────────────────────────────────

        private static void FixActionBarButtons(GameObject actionBarGo)
        {
            var horizontalLayoutGroup = actionBarGo.GetComponentInChildren<HorizontalLayoutGroup>(true);
            if (horizontalLayoutGroup == null) return;
            horizontalLayoutGroup.childControlHeight     = true;
            horizontalLayoutGroup.childForceExpandHeight = true;
            horizontalLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
            horizontalLayoutGroup.spacing = 6;
        }

        // ── RectTransform helpers ──────────────────────────────────────────────

        private static void Anchor(MonoBehaviour view,
            Vector2 min, Vector2 max, Vector2 pivot, Vector2 size, Vector2 pos)
        {
            var rectTransform = view.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            rectTransform.anchorMin        = min;
            rectTransform.anchorMax        = max;
            rectTransform.pivot            = pivot;
            rectTransform.sizeDelta        = size;
            rectTransform.anchoredPosition = pos;
        }

        private static void StretchFull(GameObject go)
        {
            var rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot     = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        // ── Find helpers ───────────────────────────────────────────────────────

        private static Canvas FindCanvas()
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<Canvas>();
#else
            return Object.FindObjectOfType<Canvas>();
#endif
        }

        // Iterates DIRECT children by index — works for inactive GameObjects.
        // Transform.Find and GameObject.Find both skip inactive GameObjects.
        private static GameObject GetDirectChildByName(Transform parentTransform, string childName,
            bool warnIfMissing = true)
        {
            for (int childIndex = 0; childIndex < parentTransform.childCount; childIndex++)
            {
                Transform child = parentTransform.GetChild(childIndex);
                if (child.name == childName) return child.gameObject;
            }
            if (warnIfMissing)
                Debug.LogWarning($"[Catan] Canvas child not found: {childName}");
            return null;
        }

        private static void SetDirectChildActive(Transform parentTransform, string childName, bool active,
            bool warnIfMissing = true)
        {
            var childGo = GetDirectChildByName(parentTransform, childName, warnIfMissing);
            if (childGo != null) childGo.SetActive(active);
        }

        // Finds a component T by GameObject name. Works after FixPanelActiveStates
        // has made all panels active, so GameObject.Find is reliable.
        private static T FindByName<T>(string objectName) where T : MonoBehaviour
        {
            var go = GameObject.Find(objectName);
            if (go != null)
            {
                var component = go.GetComponent<T>();
                if (component != null) return component;
                component = go.GetComponentInChildren<T>(includeInactive: true);
                if (component != null) return component;
            }

#if UNITY_2023_1_OR_NEWER
            return Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
            return Object.FindObjectOfType<T>(includeInactive: true);
#endif
        }

        private static Vector2 V2(float x, float y) => new Vector2(x, y);
    }
}
