using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace Catan.UI.Editor
{
    public static class CardHandUICreator
    {
        private const string PrefabFolder = "Assets/Catan/Prefabs";

        [MenuItem("Catan/Create Card Hand Prefabs")]
        public static void CreateCardHandPrefabs()
        {
            EnsureFolder(PrefabFolder);
            CreateResourceCardSlotPrefab();
            var devCardItemPrefab = CreateDevCardItemPrefab();
            CreatePlayerHandPanelPrefab();
            CreateDevHandPanelPrefab(devCardItemPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Catan] Card hand prefabs saved to " + PrefabFolder);
        }

        // ── ResourceCardSlot prefab ────────────────────────────────────────────

        private static void CreateResourceCardSlotPrefab()
        {
            var root = new GameObject("ResourceCardSlot");
            root.AddComponent<RectTransform>();
            var layoutElement = root.AddComponent<LayoutElement>();
            layoutElement.preferredWidth  = 70;
            layoutElement.preferredHeight = 100;

            var background = root.AddComponent<Image>();
            background.color = Color.white;

            // Icon — upper 60 %
            var iconGo   = CreateChild(root, "ResourceIcon");
            var iconRect = iconGo.GetComponent<RectTransform>();
            SetAnchors(iconRect, 0.08f, 0.38f, 0.92f, 0.95f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;

            // Count label — lower 38 %
            var countGo   = CreateChild(root, "CountLabel");
            var countRect = countGo.GetComponent<RectTransform>();
            SetAnchors(countRect, 0f, 0f, 1f, 0.38f);
            countRect.offsetMin = new Vector2(4, 4);
            countRect.offsetMax = new Vector2(-4, -4);
            var countTmp = countGo.AddComponent<TextMeshProUGUI>();
            countTmp.text      = "0";
            countTmp.fontSize  = 24;
            countTmp.fontStyle = FontStyles.Bold;
            countTmp.alignment = TextAlignmentOptions.Center;
            countTmp.color     = Color.white;

            var view = root.AddComponent<ResourceCardSlotView>();
            view.CardBackground = background;
            view.ResourceIcon   = iconImg;
            view.CountLabel     = countTmp;

            SavePrefab(root, PrefabFolder + "/ResourceCardSlot.prefab");
            Object.DestroyImmediate(root);
        }

        // ── DevCardItem prefab ─────────────────────────────────────────────────

        private static GameObject CreateDevCardItemPrefab()
        {
            var root = new GameObject("DevCardItem");
            root.AddComponent<RectTransform>();
            var layoutElement = root.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 72;

            var background = root.AddComponent<Image>();
            background.color = new Color(0.20f, 0.55f, 0.20f);

            var hlg = root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding              = new RectOffset(10, 6, 8, 8);
            hlg.spacing              = 8;
            hlg.childControlWidth    = true;
            hlg.childControlHeight   = true;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = true;

            // Card name (fills remaining width)
            var nameGo = CreateChild(root, "NameLabel");
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1;
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text      = "Card Name";
            nameTmp.fontSize  = 16;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.color     = Color.white;

            // Play button (fixed 64 px)
            var playGo = CreateChild(root, "PlayButton");
            var playLe = playGo.AddComponent<LayoutElement>();
            playLe.preferredWidth  = 64;
            playLe.flexibleWidth   = 0;
            var playImg = playGo.AddComponent<Image>();
            playImg.color = new Color(0.15f, 0.40f, 0.75f);
            var playBtn = playGo.AddComponent<Button>();
            playBtn.targetGraphic = playImg;
            var playColors = playBtn.colors;
            playColors.disabledColor = new Color(0.25f, 0.25f, 0.25f);
            playBtn.colors = playColors;

            var playLabelGo = CreateChild(playGo, "Label");
            StretchFull(playLabelGo.GetComponent<RectTransform>());
            var playLabelTmp = playLabelGo.AddComponent<TextMeshProUGUI>();
            playLabelTmp.text      = "Play";
            playLabelTmp.fontSize  = 14;
            playLabelTmp.fontStyle = FontStyles.Bold;
            playLabelTmp.alignment = TextAlignmentOptions.Center;
            playLabelTmp.color     = Color.white;

            var view = root.AddComponent<DevCardItemView>();
            view.CardBackground  = background;
            view.NameLabel       = nameTmp;
            view.PlayButton      = playBtn;
            view.PlayableColor   = new Color(0.20f, 0.55f, 0.20f);
            view.UnplayableColor = new Color(0.30f, 0.30f, 0.30f);

            const string path = PrefabFolder + "/DevCardItem.prefab";
            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            Debug.Log("[Catan] Created " + path);
            return savedPrefab;
        }

        // ── PlayerHandPanel prefab ─────────────────────────────────────────────

        private static void CreatePlayerHandPanelPrefab()
        {
            var root = new GameObject("PlayerHandPanel");
            root.AddComponent<RectTransform>();
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.10f, 0.10f, 0.92f);
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding              = new RectOffset(10, 10, 8, 8);
            vlg.spacing              = 8;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            root.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var view = root.AddComponent<PlayerHandView>();

            // Player name
            var nameGo = CreateChild(root, "PlayerNameLabel");
            SetLayoutElement(nameGo, preferredHeight: 30);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text      = "Player";
            nameTmp.fontSize  = 18;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.color     = Color.white;
            view.PlayerNameLabel = nameTmp;

            // Resource card row
            var cardRow = CreateChild(root, "ResourceCardRow");
            SetLayoutElement(cardRow, preferredHeight: 100);
            var cardHlg = cardRow.AddComponent<HorizontalLayoutGroup>();
            cardHlg.spacing              = 6;
            cardHlg.childControlWidth    = false;
            cardHlg.childControlHeight   = false;
            cardHlg.childForceExpandWidth  = false;
            cardHlg.childForceExpandHeight = false;
            cardHlg.childAlignment       = TextAnchor.MiddleLeft;

            view.WoodSlot  = CreateInlineSlot(cardRow, "WoodSlot",  CatanResourceType.Wood);
            view.BrickSlot = CreateInlineSlot(cardRow, "BrickSlot", CatanResourceType.Brick);
            view.SheepSlot = CreateInlineSlot(cardRow, "SheepSlot", CatanResourceType.Sheep);
            view.WheatSlot = CreateInlineSlot(cardRow, "WheatSlot", CatanResourceType.Wheat);
            view.OreSlot   = CreateInlineSlot(cardRow, "OreSlot",   CatanResourceType.Ore);

            // Bottom info row
            var infoRow = CreateChild(root, "InfoRow");
            SetLayoutElement(infoRow, preferredHeight: 24);
            var infoHlg = infoRow.AddComponent<HorizontalLayoutGroup>();
            infoHlg.spacing              = 16;
            infoHlg.childControlWidth    = false;
            infoHlg.childControlHeight   = true;
            infoHlg.childForceExpandWidth  = false;
            infoHlg.childForceExpandHeight = true;

            var totalGo = CreateChild(infoRow, "TotalCardsLabel");
            totalGo.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 0);
            var totalTmp = totalGo.AddComponent<TextMeshProUGUI>();
            totalTmp.text      = "Cards: 0";
            totalTmp.fontSize  = 13;
            totalTmp.color     = new Color(0.75f, 0.75f, 0.75f);
            totalTmp.alignment = TextAlignmentOptions.MidlineLeft;
            view.TotalCardsLabel = totalTmp;

            var vpGo = CreateChild(infoRow, "VictoryPointsLabel");
            vpGo.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 0);
            var vpTmp = vpGo.AddComponent<TextMeshProUGUI>();
            vpTmp.text      = "VP: 0";
            vpTmp.fontSize  = 13;
            vpTmp.color     = new Color(1f, 0.9f, 0.3f);
            vpTmp.alignment = TextAlignmentOptions.MidlineLeft;
            view.VictoryPointsLabel = vpTmp;

            SavePrefab(root, PrefabFolder + "/PlayerHandPanel.prefab");
            Object.DestroyImmediate(root);
        }

        // Creates one ResourceCardSlotView inline (not a nested prefab) and
        // attempts to auto-find the matching CatanResource ScriptableObject.
        private static ResourceCardSlotView CreateInlineSlot(
            GameObject parent, string slotName, CatanResourceType resourceType)
        {
            var go = CreateChild(parent, slotName);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(70, 100);

            var background = go.AddComponent<Image>();
            background.color = Color.white;

            // Icon — upper 60 %
            var iconGo   = CreateChild(go, "ResourceIcon");
            var iconRect = iconGo.GetComponent<RectTransform>();
            SetAnchors(iconRect, 0.08f, 0.38f, 0.92f, 0.95f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;

            // Count label — lower 38 %
            var countGo   = CreateChild(go, "CountLabel");
            var countRect = countGo.GetComponent<RectTransform>();
            SetAnchors(countRect, 0f, 0f, 1f, 0.38f);
            countRect.offsetMin = new Vector2(4, 4);
            countRect.offsetMax = new Vector2(-4, -4);
            var countTmp = countGo.AddComponent<TextMeshProUGUI>();
            countTmp.text      = "0";
            countTmp.fontSize  = 24;
            countTmp.fontStyle = FontStyles.Bold;
            countTmp.alignment = TextAlignmentOptions.Center;
            countTmp.color     = Color.white;

            var slotView = go.AddComponent<ResourceCardSlotView>();
            slotView.CardBackground = background;
            slotView.ResourceIcon   = iconImg;
            slotView.CountLabel     = countTmp;
            slotView.Resource       = FindCatanResource(resourceType);

            if (slotView.Resource != null)
            {
                background.color = slotView.Resource.Color;
                Debug.Log($"[Catan] Auto-connected {resourceType} resource to {slotName}");
            }
            else
            {
                Debug.LogWarning(
                    $"[Catan] Could not find CatanResource for {resourceType}. " +
                    $"Assign it manually to {slotName}.Resource in the prefab.");
            }

            return slotView;
        }

        // ── DevHandPanel prefab ────────────────────────────────────────────────

        private static void CreateDevHandPanelPrefab(GameObject devCardItemPrefab)
        {
            var root = new GameObject("DevHandPanel");
            root.AddComponent<RectTransform>();
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.10f, 0.10f, 0.92f);
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.padding              = new RectOffset(8, 8, 8, 8);
            vlg.spacing              = 6;
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            var devHandView = root.AddComponent<DevHandView>();

            // Header
            var headerGo = CreateChild(root, "Header");
            SetLayoutElement(headerGo, preferredHeight: 28);
            var headerTmp = headerGo.AddComponent<TextMeshProUGUI>();
            headerTmp.text      = "Dev Cards";
            headerTmp.fontSize  = 16;
            headerTmp.fontStyle = FontStyles.Bold;
            headerTmp.alignment = TextAlignmentOptions.MidlineLeft;
            headerTmp.color     = new Color(0.85f, 0.85f, 0.85f);

            // Card container — DevHandView spawns items here
            var containerGo = CreateChild(root, "CardContainer");
            SetLayoutElement(containerGo, preferredHeight: 200, flexibleHeight: 1);
            var containerVlg = containerGo.AddComponent<VerticalLayoutGroup>();
            containerVlg.spacing              = 4;
            containerVlg.childControlWidth    = true;
            containerVlg.childControlHeight   = false;
            containerVlg.childForceExpandWidth  = true;
            containerVlg.childForceExpandHeight = false;
            containerGo.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            devHandView.CardContainer      = containerGo.transform;
            devHandView.DevCardItemPrefab  = devCardItemPrefab;

            SavePrefab(root, PrefabFolder + "/DevHandPanel.prefab");
            Object.DestroyImmediate(root);
        }

        // ── Resource ScriptableObject lookup ───────────────────────────────────

        private static CatanResource FindCatanResource(CatanResourceType resourceType)
        {
            var guids = AssetDatabase.FindAssets("t:CatanResource");
            foreach (var guid in guids)
            {
                var path  = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<CatanResource>(path);
                if (asset != null && asset.Type == resourceType)
                    return asset;
            }
            return null;
        }

        // ── Shared helpers (mirrored from DiscardUICreator) ────────────────────

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

        private static void SetAnchors(RectTransform rt,
            float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Debug.Log("[Catan] Created " + path);
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
    }
}
