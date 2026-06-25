using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace Catan.UI.Editor
{
    public static class DiscardUICreator
    {
        private const string PrefabFolder = "Assets/Catan/Prefabs";
        private const string CardPrefabPath  = PrefabFolder + "/DiscardCard.prefab";
        private const string PanelPrefabPath = PrefabFolder + "/DiscardPanel.prefab";

        [MenuItem("Catan/Create All UI Prefabs")]
        public static void CreateAllPrefabs()
        {
            EnsureFolder(PrefabFolder);
            CreateCardPrefab();
            CreatePanelPrefab();
            CreateBankTradePanelPrefab();
            CreateStealTargetPanelPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Catan] All UI prefabs saved to {PrefabFolder}");
        }

        [MenuItem("Catan/Create Discard Prefabs")]
        public static void CreateDiscardPrefabs()
        {
            EnsureFolder(PrefabFolder);
            CreateCardPrefab();
            CreatePanelPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Catan] Discard prefabs saved to {PrefabFolder}");
        }

        // ── Card prefab ────────────────────────────────────────────────────────

        private static void CreateCardPrefab()
        {
            var root = new GameObject("DiscardCard");
            var rootRect = root.AddComponent<RectTransform>();
            SetSize(rootRect, 70, 100);

            // Coloured card background
            var bg = root.AddComponent<Image>();
            bg.color = Color.white;
            bg.raycastTarget = true;

            // Button
            var btn = root.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor     = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = colors;

            // Resource icon (top 60 %)
            var iconGo = new GameObject("ResourceIcon");
            iconGo.transform.SetParent(root.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            SetAnchors(iconRect, 0f, 0.35f, 1f, 1f);
            SetOffsets(iconRect, 6f, 6f, -6f, -6f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;

            // Resource label (bottom 35 %)
            var labelGo = new GameObject("ResourceLabel");
            labelGo.transform.SetParent(root.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            SetAnchors(labelRect, 0f, 0f, 1f, 0.35f);
            SetOffsets(labelRect, 2f, 2f, -2f, -2f);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text            = "Resource";
            label.fontSize        = 10;
            label.alignment       = TextAlignmentOptions.Center;
            label.color           = Color.white;
            label.fontStyle       = FontStyles.Bold;
            label.overflowMode    = TextOverflowModes.Ellipsis;

            // Wire DiscardCardView
            var cardView = root.AddComponent<DiscardCardView>();
            cardView.CardBackground = bg;
            cardView.ResourceIcon   = iconImg;
            cardView.ResourceLabel  = label;

            SavePrefab(root, CardPrefabPath);
            Object.DestroyImmediate(root);
        }

        // ── Panel prefab ───────────────────────────────────────────────────────

        private static void CreatePanelPrefab()
        {
            // Root — full-screen blocker
            var root = new GameObject("DiscardPanel");
            var rootRect = root.AddComponent<RectTransform>();
            StretchFull(rootRect);
            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;
            var panelView = root.AddComponent<DiscardPanelView>();

            // Dark background overlay
            var bgGo = CreateChild(root, "Background");
            StretchFull(bgGo.GetComponent<RectTransform>());
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.6f);
            bgImg.raycastTarget = true;

            // White inner panel, centred
            var inner = CreateChild(root, "InnerPanel");
            var innerRect = inner.GetComponent<RectTransform>();
            SetAnchorCenter(innerRect, 750, 560);
            var innerImg = inner.AddComponent<Image>();
            innerImg.color = new Color(0.15f, 0.15f, 0.15f);
            var vlg = inner.AddComponent<VerticalLayoutGroup>();
            vlg.padding   = new RectOffset(20, 20, 20, 20);
            vlg.spacing   = 12;
            vlg.childControlWidth  = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // Player name
            var playerLabel = CreateTMPLabel(inner, "PlayerNameLabel",
                "Player Name", 24, FontStyles.Bold, 50);
            panelView.PlayerNameLabel = playerLabel;

            // Instruction
            var instrLabel = CreateTMPLabel(inner, "InstructionLabel",
                "Select X cards to discard", 16, FontStyles.Normal, 40);
            panelView.InstructionLabel = instrLabel;

            // ── Hand section ──────────────────────────────────────────────────
            var handSection = CreateSection(inner, "HandSection", 180, out var handContainer);
            CreateSectionHeader(handSection, "Your Hand");
            panelView.HandContainer = handContainer.transform;

            // ── Discard section ───────────────────────────────────────────────
            var discardSection = CreateSection(inner, "DiscardSection", 180, out var discardContainer);
            CreateSectionHeader(discardSection, "To Discard");
            panelView.DiscardContainer = discardContainer.transform;

            // ── Confirm button ────────────────────────────────────────────────
            var confirmGo = CreateChild(inner, "ConfirmButton");
            SetLayoutElement(confirmGo, preferredHeight: 50);
            var confirmImg = confirmGo.AddComponent<Image>();
            confirmImg.color = new Color(0.2f, 0.7f, 0.3f);
            var confirmBtn = confirmGo.AddComponent<Button>();
            var confirmColors = confirmBtn.colors;
            confirmColors.disabledColor = new Color(0.3f, 0.3f, 0.3f);
            confirmBtn.colors = confirmColors;
            confirmBtn.targetGraphic = confirmImg;

            var confirmLabel = CreateChild(confirmGo, "Label");
            StretchFull(confirmLabel.GetComponent<RectTransform>());
            var confirmTmp = confirmLabel.AddComponent<TextMeshProUGUI>();
            confirmTmp.text      = "Confirm Discard";
            confirmTmp.fontSize  = 18;
            confirmTmp.fontStyle = FontStyles.Bold;
            confirmTmp.alignment = TextAlignmentOptions.Center;
            confirmTmp.color     = Color.white;

            // Wire confirm button event
            var serializedBtn = new SerializedObject(confirmBtn);
            panelView.ConfirmButton = confirmBtn;

            // Note: OnClick must be wired in the Inspector to DiscardPanelView.OnConfirmDiscard()
            // because UnityEvent cross-object binding can't be set purely in editor code.

            SavePrefab(root, PanelPrefabPath);
            Object.DestroyImmediate(root);
        }

        // ── Bank trade panel prefab ────────────────────────────────────────────

        private static void CreateBankTradePanelPrefab()
        {
            // Root — full-screen overlay
            var root = new GameObject("BankTradePanel");
            StretchFull(root.AddComponent<RectTransform>());
            root.AddComponent<CanvasGroup>().blocksRaycasts = true;
            var panelView = root.AddComponent<BankTradePanelView>();

            // Dark background
            var bgGo = CreateChild(root, "Background");
            StretchFull(bgGo.GetComponent<RectTransform>());
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.6f);
            bgImg.raycastTarget = true;

            // Inner panel
            var inner = CreateChild(root, "InnerPanel");
            SetAnchorCenter(inner.GetComponent<RectTransform>(), 700, 480);
            inner.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);
            var vlg = inner.AddComponent<VerticalLayoutGroup>();
            vlg.padding  = new RectOffset(20, 20, 20, 20);
            vlg.spacing  = 14;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // Title
            CreateTMPLabel(inner, "Title", "Bank Trade", 22, FontStyles.Bold, 44);

            // Give row
            CreateTMPLabel(inner, "GiveLabel", "Give (select one)", 14, FontStyles.Normal, 28);
            var giveRow = CreateResourceButtonRow(inner, "GiveRow", out var giveButtons);

            // Receive row
            CreateTMPLabel(inner, "ReceiveLabel", "Receive (select one)", 14, FontStyles.Normal, 28);
            var receiveRow = CreateResourceButtonRow(inner, "ReceiveRow", out var receiveButtons);

            // Feedback
            var feedbackGo = CreateChild(inner, "FeedbackLabel");
            SetLayoutElement(feedbackGo, preferredHeight: 28);
            var feedbackTmp = feedbackGo.AddComponent<TextMeshProUGUI>();
            feedbackTmp.fontSize  = 14;
            feedbackTmp.alignment = TextAlignmentOptions.Center;
            feedbackTmp.color     = new Color(0.9f, 0.9f, 0.3f);

            // Buttons row
            var buttonRow = CreateChild(inner, "ButtonRow");
            SetLayoutElement(buttonRow, preferredHeight: 50);
            var hlg = buttonRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing             = 12;
            hlg.childControlWidth   = true;
            hlg.childControlHeight  = true;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;

            var cancelBtn = CreateTextButton(buttonRow, "CancelButton", "Cancel",
                new Color(0.6f, 0.2f, 0.2f));
            var confirmBtn = CreateTextButton(buttonRow, "ConfirmButton", "Confirm Trade",
                new Color(0.2f, 0.6f, 0.3f));

            // Wire fields
            panelView.GiveButtons    = giveButtons;
            panelView.ReceiveButtons = receiveButtons;
            panelView.ConfirmButton  = confirmBtn;
            panelView.CancelButton   = cancelBtn;
            panelView.FeedbackLabel  = feedbackTmp;

            // Note: CancelButton.OnClick → BankTradePanelView.Close()
            //       ConfirmButton.OnClick → BankTradePanelView.OnConfirm()
            // Must be wired in Inspector (cross-object UnityEvent binding).

            const string path = PrefabFolder + "/BankTradePanel.prefab";
            SavePrefab(root, path);
            Object.DestroyImmediate(root);
        }

        private static GameObject CreateResourceButtonRow(GameObject parent, string name,
            out Button[] buttons)
        {
            var row = CreateChild(parent, name);
            SetLayoutElement(row, preferredHeight: 90);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing              = 8;
            hlg.childControlWidth    = true;
            hlg.childControlHeight   = true;
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;

            string[] resourceNames = { "Wood", "Brick", "Sheep", "Wheat", "Ore" };
            Color[] resourceColors =
            {
                new Color(0.40f, 0.25f, 0.10f),
                new Color(0.80f, 0.30f, 0.10f),
                new Color(0.55f, 0.85f, 0.35f),
                new Color(0.95f, 0.85f, 0.20f),
                new Color(0.50f, 0.50f, 0.60f),
            };

            buttons = new Button[5];
            for (int i = 0; i < 5; i++)
                buttons[i] = CreateTextButton(row, $"{resourceNames[i]}Btn",
                    resourceNames[i], resourceColors[i]);

            return row;
        }

        private static Button CreateTextButton(GameObject parent, string name,
            string label, Color color)
        {
            var go = CreateChild(parent, name);
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelGo = CreateChild(go, "Label");
            StretchFull(labelGo.GetComponent<RectTransform>());
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 13;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            return btn;
        }

        // ── Steal target panel prefab ──────────────────────────────────────────

        private static void CreateStealTargetPanelPrefab()
        {
            // Root — full-screen overlay
            var root = new GameObject("StealTargetPanel");
            StretchFull(root.AddComponent<RectTransform>());
            root.AddComponent<CanvasGroup>().blocksRaycasts = true;
            var panelView = root.AddComponent<StealTargetPanelView>();

            // Dark background
            var bgGo = CreateChild(root, "Background");
            StretchFull(bgGo.GetComponent<RectTransform>());
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.6f);
            bgImg.raycastTarget = true;

            // Inner panel — compact, centred
            var inner = CreateChild(root, "InnerPanel");
            SetAnchorCenter(inner.GetComponent<RectTransform>(), 400, 300);
            inner.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);
            var vlg = inner.AddComponent<VerticalLayoutGroup>();
            vlg.padding  = new RectOffset(20, 20, 20, 20);
            vlg.spacing  = 14;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // Title
            var titleLabel = CreateTMPLabel(inner, "TitleLabel",
                "Choose a player to steal from", 16, FontStyles.Bold, 40);
            panelView.TitleLabel = titleLabel;

            // Button container
            var container = CreateChild(inner, "ButtonContainer");
            SetLayoutElement(container, preferredHeight: 200, flexibleHeight: 1);
            var containerVlg = container.AddComponent<VerticalLayoutGroup>();
            containerVlg.spacing              = 10;
            containerVlg.childControlWidth    = true;
            containerVlg.childControlHeight   = false;
            containerVlg.childForceExpandWidth  = true;
            containerVlg.childForceExpandHeight = false;
            panelView.ButtonContainer = container.transform;

            // Player button prefab — saved separately
            var playerBtn = CreatePlayerButtonPrefab();
            panelView.PlayerButtonPrefab = playerBtn;

            const string path = PrefabFolder + "/StealTargetPanel.prefab";
            SavePrefab(root, path);
            Object.DestroyImmediate(root);
        }

        private static GameObject CreatePlayerButtonPrefab()
        {
            var go = new GameObject("PlayerButton");
            go.AddComponent<RectTransform>();
            SetLayoutElement(go, preferredHeight: 54);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.8f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelGo = CreateChild(go, "Label");
            StretchFull(labelGo.GetComponent<RectTransform>());
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = "Player";
            tmp.fontSize  = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            const string path = PrefabFolder + "/PlayerButton.prefab";
            var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[Catan] Created {path}");
            return saved;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static GameObject CreateSection(GameObject parent, string name,
            float height, out GameObject container)
        {
            var section = CreateChild(parent, name);
            SetLayoutElement(section, preferredHeight: height);
            var vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.spacing              = 4;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // Spacer to push header up
            var header = CreateChild(section, "Header");
            SetLayoutElement(header, preferredHeight: 28);

            // Card container
            container = CreateChild(section, "CardContainer");
            SetLayoutElement(container, preferredHeight: height - 32, flexibleHeight: 1);
            var hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing              = 6;
            hlg.childControlWidth    = false;
            hlg.childControlHeight   = false;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment       = TextAnchor.MiddleLeft;

            // Background tint on container
            var containerImg = container.AddComponent<Image>();
            containerImg.color = new Color(0.08f, 0.08f, 0.08f, 0.6f);

            return header;
        }

        private static void CreateSectionHeader(GameObject headerGo, string text)
        {
            StretchFull(headerGo.GetComponent<RectTransform>());
            var tmp = headerGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color     = new Color(0.85f, 0.85f, 0.85f);
        }

        private static TextMeshProUGUI CreateTMPLabel(GameObject parent, string goName,
            string text, float fontSize, FontStyles style, float preferredHeight)
        {
            var go = CreateChild(parent, goName);
            SetLayoutElement(go, preferredHeight: preferredHeight);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            return tmp;
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void SetLayoutElement(GameObject go,
            float preferredHeight = -1, float flexibleHeight = -1)
        {
            var le = go.AddComponent<LayoutElement>();
            if (preferredHeight >= 0) le.preferredHeight = preferredHeight;
            if (flexibleHeight   >= 0) le.flexibleHeight  = flexibleHeight;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin     = Vector2.zero;
            rt.anchorMax     = Vector2.one;
            rt.offsetMin     = Vector2.zero;
            rt.offsetMax     = Vector2.zero;
        }

        private static void SetAnchorCenter(RectTransform rt, float width, float height)
        {
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = new Vector2(width, height);
        }

        private static void SetSize(RectTransform rt, float width, float height)
        {
            rt.sizeDelta = new Vector2(width, height);
        }

        private static void SetAnchors(RectTransform rt,
            float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
        }

        private static void SetOffsets(RectTransform rt,
            float left, float bottom, float right, float top)
        {
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(right, top);
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Debug.Log($"[Catan] Created {path}");
        }

        private static void EnsureFolder(string folderPath)
        {
            var parts = folderPath.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
