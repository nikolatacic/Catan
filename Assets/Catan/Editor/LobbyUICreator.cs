using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Catan.UI.Editor
{
    public static class LobbyUICreator
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Catan/Add Multiplayer Lobby to Main Menu")]
        public static void AddLobbyToMainMenu()
        {
            if (!System.IO.File.Exists(MainMenuScenePath))
            {
                EditorUtility.DisplayDialog("Main Menu missing",
                    "Run Catan → Create Main Menu Scene first.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            var canvas = FindCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Canvas missing",
                    "MainMenu has no Canvas. Regenerate the scene first.", "OK");
                return;
            }

            var menuView = Object.FindObjectOfType<MainMenuView>();
            if (menuView == null)
            {
                EditorUtility.DisplayDialog("MainMenuView missing",
                    "MainMenu has no MainMenuView component. Regenerate the scene first.", "OK");
                return;
            }

            EnsureNetworkManager();
            var lobbyView = EnsureLobbyPanel(canvas.gameObject);
            menuView.Lobby = lobbyView;

            AddHostJoinButtonsIfMissing(menuView);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log(
                "[Catan] Lobby + NetworkManager added to MainMenu.\n" +
                "Manual step: in Edit → Project Settings → Services, link this " +
                "project to a Unity Cloud project so Relay and Authentication work.");
        }

        // ── NetworkManager ─────────────────────────────────────────────────────

        private static void EnsureNetworkManager()
        {
            var existing = Object.FindObjectOfType<NetworkManager>();
            if (existing != null) return;

            var go = new GameObject("NetworkManager");
            var manager = go.AddComponent<NetworkManager>();
            var transport = go.AddComponent<UnityTransport>();
            manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
            };
            Debug.Log("[Catan] Created NetworkManager with UnityTransport.");
        }

        // ── Lobby panel ────────────────────────────────────────────────────────

        private static LobbyView EnsureLobbyPanel(GameObject canvas)
        {
            var existing = Object.FindObjectOfType<LobbyView>();
            if (existing != null) return existing;

            // Root — full-screen overlay (starts inactive)
            var root = new GameObject("LobbyPanel");
            root.transform.SetParent(canvas.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            StretchFull(rootRect);
            root.AddComponent<CanvasGroup>().blocksRaycasts = true;

            // Dark background
            var bgGo = CreateChild(root, "Background");
            StretchFull(bgGo.GetComponent<RectTransform>());
            bgGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

            // Inner panel — centred
            var inner = CreateChild(root, "InnerPanel");
            var innerRect = inner.GetComponent<RectTransform>();
            innerRect.anchorMin        = new Vector2(0.5f, 0.5f);
            innerRect.anchorMax        = new Vector2(0.5f, 0.5f);
            innerRect.pivot            = new Vector2(0.5f, 0.5f);
            innerRect.anchoredPosition = Vector2.zero;
            innerRect.sizeDelta        = new Vector2(720, 540);
            inner.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.16f);
            var vlg = inner.AddComponent<VerticalLayoutGroup>();
            vlg.padding              = new RectOffset(28, 28, 28, 28);
            vlg.spacing              = 16;
            vlg.childAlignment       = TextAnchor.UpperCenter;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // Title
            var titleTmp = CreateLabel(inner, "TitleLabel", "Multiplayer", 36,
                FontStyles.Bold, 50, Color.white);
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Status label
            var statusTmp = CreateLabel(inner, "StatusLabel", "—", 16,
                FontStyles.Normal, 40, new Color(0.85f, 0.85f, 0.55f));
            statusTmp.alignment = TextAlignmentOptions.Center;

            // ── Host section ──────────────────────────────────────────────────
            var hostSection = CreateChild(inner, "HostSection");
            SetLayoutElement(hostSection, preferredHeight: 110);
            var hostVlg = hostSection.AddComponent<VerticalLayoutGroup>();
            hostVlg.spacing              = 6;
            hostVlg.childControlWidth    = true;
            hostVlg.childControlHeight   = false;
            hostVlg.childForceExpandWidth = true;

            CreateLabel(hostSection, "HostHint", "Share this code with friends:",
                14, FontStyles.Italic, 24, new Color(0.7f, 0.7f, 0.7f))
                .alignment = TextAlignmentOptions.Center;
            var joinCodeDisplay = CreateLabel(hostSection, "JoinCodeDisplay", "—",
                42, FontStyles.Bold, 64, new Color(1f, 0.85f, 0.35f));
            joinCodeDisplay.alignment = TextAlignmentOptions.Center;

            // ── Client section ────────────────────────────────────────────────
            var clientSection = CreateChild(inner, "ClientSection");
            SetLayoutElement(clientSection, preferredHeight: 110);
            var clientVlg = clientSection.AddComponent<VerticalLayoutGroup>();
            clientVlg.spacing              = 8;
            clientVlg.childControlWidth    = true;
            clientVlg.childControlHeight   = false;
            clientVlg.childForceExpandWidth = true;

            CreateLabel(clientSection, "ClientHint", "Enter join code:",
                14, FontStyles.Italic, 24, new Color(0.7f, 0.7f, 0.7f))
                .alignment = TextAlignmentOptions.Center;
            var joinCodeInput = CreateInputField(clientSection, "JoinCodeInput",
                placeholder: "ABCDEF");

            // ── Buttons ───────────────────────────────────────────────────────
            var buttonRow = CreateChild(inner, "ButtonRow");
            SetLayoutElement(buttonRow, preferredHeight: 60);
            var rowHlg = buttonRow.AddComponent<HorizontalLayoutGroup>();
            rowHlg.spacing              = 16;
            rowHlg.childControlWidth    = true;
            rowHlg.childControlHeight   = true;
            rowHlg.childForceExpandWidth  = true;
            rowHlg.childForceExpandHeight = true;

            var backBtn    = CreateTextButton(buttonRow, "BackButton", "Back",
                new Color(0.40f, 0.40f, 0.40f));
            var confirmBtn = CreateTextButton(buttonRow, "ConfirmButton", "Start",
                new Color(0.20f, 0.65f, 0.25f));

            // ── Wire LobbyView ────────────────────────────────────────────────
            var lobbyView = root.AddComponent<LobbyView>();
            lobbyView.Root             = root;
            lobbyView.StatusLabel      = statusTmp;
            lobbyView.ConfirmButton    = confirmBtn;
            lobbyView.BackButton       = backBtn;
            lobbyView.HostSection      = hostSection;
            lobbyView.JoinCodeDisplay  = joinCodeDisplay;
            lobbyView.ClientSection    = clientSection;
            lobbyView.JoinCodeInput    = joinCodeInput;

            root.SetActive(false);

            return lobbyView;
        }

        // ── Host/Join buttons on main menu ─────────────────────────────────────

        private static void AddHostJoinButtonsIfMissing(MainMenuView menuView)
        {
            var content = GameObject.Find("Content");
            if (content == null) return;

            if (content.transform.Find("HostButton") == null)
            {
                var hostBtn = CreateMenuButton(content, "HostButton", "HOST", 90,
                    new Color(0.20f, 0.45f, 0.85f));
                UnityEventTools.AddPersistentListener(hostBtn.onClick, menuView.OnHost);
            }

            if (content.transform.Find("JoinButton") == null)
            {
                var joinBtn = CreateMenuButton(content, "JoinButton", "JOIN", 90,
                    new Color(0.75f, 0.45f, 0.15f));
                UnityEventTools.AddPersistentListener(joinBtn.onClick, menuView.OnJoin);
            }
        }

        private static Button CreateMenuButton(GameObject parent, string goName,
            string label, float height, Color color)
        {
            var go = CreateChild(parent, goName);
            SetLayoutElement(go, preferredHeight: height);
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelGo = CreateChild(go, "Label");
            StretchFull(labelGo.GetComponent<RectTransform>());
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 36;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            return btn;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static Canvas FindCanvas()
        {
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var c = go.GetComponentInChildren<Canvas>();
                if (c != null) return c;
            }
            return null;
        }

        private static GameObject CreateChild(GameObject parent, string childName)
        {
            var go = new GameObject(childName);
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
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI CreateLabel(GameObject parent, string goName,
            string text, float fontSize, FontStyles style, float preferredHeight, Color color)
        {
            var go = CreateChild(parent, goName);
            SetLayoutElement(go, preferredHeight: preferredHeight);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.fontStyle = style;
            tmp.color     = color;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            return tmp;
        }

        private static Button CreateTextButton(GameObject parent, string goName,
            string label, Color color)
        {
            var go = CreateChild(parent, goName);
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var labelGo = CreateChild(go, "Label");
            StretchFull(labelGo.GetComponent<RectTransform>());
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            return btn;
        }

        private static TMP_InputField CreateInputField(GameObject parent, string goName,
            string placeholder)
        {
            var go = CreateChild(parent, goName);
            SetLayoutElement(go, preferredHeight: 50);
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.20f, 0.20f, 0.24f);

            var input = go.AddComponent<TMP_InputField>();

            // Text area child
            var textArea = CreateChild(go, "TextArea");
            var taRect = textArea.GetComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(12, 6);
            taRect.offsetMax = new Vector2(-12, -6);
            textArea.AddComponent<RectMask2D>();

            // Placeholder
            var placeholderGo = CreateChild(textArea, "Placeholder");
            StretchFull(placeholderGo.GetComponent<RectTransform>());
            var placeholderTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text      = placeholder;
            placeholderTmp.fontSize  = 22;
            placeholderTmp.fontStyle = FontStyles.Italic;
            placeholderTmp.color     = new Color(0.5f, 0.5f, 0.5f);
            placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Text component
            var textGo = CreateChild(textArea, "Text");
            StretchFull(textGo.GetComponent<RectTransform>());
            var textTmp = textGo.AddComponent<TextMeshProUGUI>();
            textTmp.fontSize  = 22;
            textTmp.color     = Color.white;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;

            input.textViewport      = taRect;
            input.textComponent     = textTmp;
            input.placeholder       = placeholderTmp;
            input.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
            input.characterLimit    = 6;

            return input;
        }
    }
}
