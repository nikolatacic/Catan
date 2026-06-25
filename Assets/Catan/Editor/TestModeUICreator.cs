using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace Catan.UI.Editor
{
    public static class TestModeUICreator
    {
        private const string PrefabFolder = "Assets/Catan/Prefabs";
        private const string PrefabPath   = PrefabFolder + "/CheatMenu.prefab";

        [MenuItem("Catan/Create Test Mode UI")]
        public static void CreateTestModeUI()
        {
            EnsureFolder(PrefabFolder);
            CreateCheatMenuPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Catan] CheatMenu prefab saved to " + PrefabPath);
        }

        private static void CreateCheatMenuPrefab()
        {
            // ── Root ───────────────────────────────────────────────────────────
            // Anchored to the top-left corner so it stays out of the way.
            var root = new GameObject("CheatMenu");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin        = new Vector2(0f, 1f);
            rootRect.anchorMax        = new Vector2(0f, 1f);
            rootRect.pivot            = new Vector2(0f, 1f);
            rootRect.anchoredPosition = new Vector2(10f, -10f);
            rootRect.sizeDelta        = new Vector2(220f, 0f);

            root.AddComponent<VerticalLayoutGroup>().SetupCompact();
            root.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var view = root.AddComponent<CheatMenuView>();

            // ── Toggle button ──────────────────────────────────────────────────
            var toggleGo = CreateChild(root, "ToggleButton");
            SetLayoutElement(toggleGo, preferredHeight: 32);
            var toggleImg = toggleGo.AddComponent<Image>();
            toggleImg.color = new Color(0.80f, 0.20f, 0.10f);
            var toggleBtn = toggleGo.AddComponent<Button>();
            toggleBtn.targetGraphic = toggleImg;

            var toggleLabel = CreateChild(toggleGo, "Label");
            StretchFull(toggleLabel.GetComponent<RectTransform>());
            var toggleTmp = toggleLabel.AddComponent<TextMeshProUGUI>();
            toggleTmp.text      = "⚙ TEST MODE";
            toggleTmp.fontSize  = 13;
            toggleTmp.fontStyle = FontStyles.Bold;
            toggleTmp.alignment = TextAlignmentOptions.Center;
            toggleTmp.color     = Color.white;

            view.ToggleButton = toggleBtn;

            // ── Cheat panel (hidden by default) ───────────────────────────────
            var panel = CreateChild(root, "CheatPanel");
            SetLayoutElement(panel, preferredHeight: -1, flexibleHeight: 0);
            panel.AddComponent<Image>().color = new Color(0.12f, 0.05f, 0.05f, 0.96f);
            var panelVlg = panel.AddComponent<VerticalLayoutGroup>();
            panelVlg.SetupCompact(top: 8, bottom: 8, left: 8, right: 8, spacing: 6);
            panel.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            view.CheatPanel = panel;

            // Active player label
            var playerLabelGo = CreateChild(panel, "PlayerLabel");
            SetLayoutElement(playerLabelGo, preferredHeight: 24);
            var playerTmp = playerLabelGo.AddComponent<TextMeshProUGUI>();
            playerTmp.text      = "Active: —";
            playerTmp.fontSize  = 12;
            playerTmp.fontStyle = FontStyles.Italic;
            playerTmp.color     = new Color(0.75f, 0.75f, 0.75f);
            playerTmp.alignment = TextAlignmentOptions.Center;
            view.PlayerLabel = playerTmp;

            // Divider
            var divider = CreateChild(panel, "Divider");
            SetLayoutElement(divider, preferredHeight: 1);
            divider.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);

            // Cheat buttons
            view.AddVPButton = CreateCheatButton(panel, "AddVPButton",
                "+ 1 VP (current player)", new Color(0.20f, 0.55f, 0.85f));

            view.GiveAllResourcesButton = CreateCheatButton(panel, "GiveAllResourcesButton",
                "+ 5 of each resource", new Color(0.55f, 0.75f, 0.20f));

            view.SkipToEndTurnButton = CreateCheatButton(panel, "SkipToEndTurnButton",
                "Skip → End Turn", new Color(0.55f, 0.40f, 0.10f));

            // ── Save ───────────────────────────────────────────────────────────
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log("[Catan] Created " + PrefabPath);
        }

        private static Button CreateCheatButton(GameObject parent, string goName,
            string label, Color color)
        {
            var go = CreateChild(parent, goName);
            SetLayoutElement(go, preferredHeight: 36);
            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var btnColors = btn.colors;
            btnColors.highlightedColor = Brighten(color, 0.15f);
            btnColors.pressedColor     = Darken(color, 0.15f);
            btn.colors = btnColors;

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

        // ── Helpers ────────────────────────────────────────────────────────────

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

        private static void EnsureFolder(string folderPath)
        {
            var parts   = folderPath.Split('/');
            var current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static Color Brighten(Color color, float amount) =>
            new Color(
                Mathf.Clamp01(color.r + amount),
                Mathf.Clamp01(color.g + amount),
                Mathf.Clamp01(color.b + amount),
                color.a);

        private static Color Darken(Color color, float amount) =>
            Brighten(color, -amount);
    }

    // Extension methods scoped to this editor file
    internal static class LayoutGroupExtensions
    {
        internal static void SetupCompact(this VerticalLayoutGroup vlg,
            int top = 0, int bottom = 0, int left = 0, int right = 0, float spacing = 0)
        {
            vlg.padding              = new RectOffset(left, right, top, bottom);
            vlg.spacing              = spacing;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
        }
    }
}
