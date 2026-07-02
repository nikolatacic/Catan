using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;

namespace Catan.UI.Editor
{
    // ── Menu paths ────────────────────────────────────────────────────────────────
    //   Catan / UI Toolkit / Setup Game Scene HUD
    //   Catan / UI Toolkit / Setup Main Menu Scene
    // ─────────────────────────────────────────────────────────────────────────────

    public static class UIToolkitSceneSetup
    {
        // ── Asset paths ───────────────────────────────────────────────────────────

        private const string GameHUDUxmlPath     = "Assets/Catan/UI/UIToolkit/HUD/GameHUD.uxml";
        private const string MainMenuUxmlPath    = "Assets/Catan/UI/UIToolkit/Scenes/MainMenu.uxml";
        private const string LobbyUxmlPath       = "Assets/Catan/UI/UIToolkit/Scenes/Lobby.uxml";
        private const string PanelSettingsPath   = "Assets/Catan/UI/UIToolkit/CatanPanelSettings.asset";
        private const string CatanElementsPath   = "Assets/Textures/GameplayTextures/CatanElements.png";
        private const string CatanDevCardsPath   = "Assets/Textures/GameplayTextures/CatanDevCards.png";

        // Sprite names inside CatanElements.png used by the action buttons
        private const string SpriteHouse       = "House";
        private const string SpriteCityTile    = "City";
        private const string SpriteRoad        = "Road";
        private const string SpriteDevCard     = "Development_Card";

        // Dev card sprite → CardId mapping (sprite index order in CatanDevCards.png)
        private static readonly (string spriteIndex, string cardId)[] DevCardMapping =
        {
            ("CatanDevCards_0", "knight"),
            ("CatanDevCards_1", "victory_point"),
            ("CatanDevCards_2", "road_building"),
            ("CatanDevCards_3", "year_of_plenty"),
            ("CatanDevCards_4", "monopoly"),
        };

        // ── Game scene ────────────────────────────────────────────────────────────

        [MenuItem("Catan/UI Toolkit/Setup Game Scene HUD")]
        public static void SetupGameSceneHUD()
        {
            if (!ConfirmActiveSceneIs("GameHotseat")) return;

            var panelSettings    = FindOrCreatePanelSettings();
            var gameHUDAsset     = LoadVisualTreeAsset(GameHUDUxmlPath);
            if (gameHUDAsset == null) return;

            var existingHUD = GameObject.Find("HUD");
            if (existingHUD != null)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "HUD already exists",
                    "A GameObject named 'HUD' already exists. Replace it?",
                    "Replace", "Cancel");
                if (!replace) return;
                Undo.DestroyObjectImmediate(existingHUD);
            }

            // ── Create HUD GameObject ─────────────────────────────────────────────
            var hudObject = new GameObject("HUD");
            Undo.RegisterCreatedObjectUndo(hudObject, "Create HUD");

            // UIDocument
            var uiDocument = Undo.AddComponent<UIDocument>(hudObject);
            uiDocument.panelSettings = panelSettings;
            uiDocument.visualTreeAsset = gameHUDAsset;

            // ── Add all view MonoBehaviours ───────────────────────────────────────
            var turnIndicatorView     = Undo.AddComponent<TurnIndicatorView>(hudObject);
            var playerHandView        = Undo.AddComponent<PlayerHandView>(hudObject);
            var allPlayersSummaryView = Undo.AddComponent<AllPlayersSummaryView>(hudObject);
            var devHandView           = Undo.AddComponent<DevHandView>(hudObject);
            var cheatMenuView         = Undo.AddComponent<CheatMenuView>(hudObject);
            var actionButtonsView     = Undo.AddComponent<ActionButtonsView>(hudObject);
            var bankTradePanelView    = Undo.AddComponent<BankTradePanelView>(hudObject);
            var discardPanelView      = Undo.AddComponent<DiscardPanelView>(hudObject);
            var monopolyPanelView     = Undo.AddComponent<MonopolyPanelView>(hudObject);
            var yearOfPlentyView      = Undo.AddComponent<YearOfPlentyPanelView>(hudObject);
            var stealTargetPanelView  = Undo.AddComponent<StealTargetPanelView>(hudObject);
            var victoryScreenView     = Undo.AddComponent<VictoryScreenView>(hudObject);

            // ── Wire ActionButtonsView ────────────────────────────────────────────
            WireActionButtonSprites(actionButtonsView);
            WireBankTradePanelRef(actionButtonsView, bankTradePanelView);

            // ── Wire DevHandView card sprites ─────────────────────────────────────
            WireDevCardSprites(devHandView);

            // ── Wire GameManager references ───────────────────────────────────────
            WireGameManagerRefs(stealTargetPanelView, monopolyPanelView, yearOfPlentyView);

            // ── Offer to remove old Canvas hierarchy ──────────────────────────────
            RemoveOldCanvases("Game");

            EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[UIToolkitSetup] Game scene HUD created. " +
                      "Remember to set PlayerHandView.PlayerIndex in the Inspector.");
        }

        // ── Main Menu scene ───────────────────────────────────────────────────────

        [MenuItem("Catan/UI Toolkit/Setup Main Menu Scene")]
        public static void SetupMainMenuScene()
        {
            if (!ConfirmActiveSceneIs("MainMenu")) return;

            var panelSettings = FindOrCreatePanelSettings();
            var mainMenuAsset = LoadVisualTreeAsset(MainMenuUxmlPath);
            var lobbyAsset    = LoadVisualTreeAsset(LobbyUxmlPath);
            if (mainMenuAsset == null) return;

            // ── Main Menu UI ──────────────────────────────────────────────────────
            var existingMainMenu = GameObject.Find("MainMenuUI");
            if (existingMainMenu != null)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "MainMenuUI already exists",
                    "A GameObject named 'MainMenuUI' already exists. Replace it?",
                    "Replace", "Cancel");
                if (!replace) return;
                Undo.DestroyObjectImmediate(existingMainMenu);
            }

            var mainMenuObject = new GameObject("MainMenuUI");
            Undo.RegisterCreatedObjectUndo(mainMenuObject, "Create MainMenuUI");

            var mainMenuDocument = Undo.AddComponent<UIDocument>(mainMenuObject);
            mainMenuDocument.panelSettings = panelSettings;
            mainMenuDocument.visualTreeAsset = mainMenuAsset;

            var mainMenuView = Undo.AddComponent<MainMenuView>(mainMenuObject);

            // ── Lobby UI (separate GameObject so it can be hidden independently) ──
            var existingLobby = GameObject.Find("LobbyUI");
            if (existingLobby != null)
                Undo.DestroyObjectImmediate(existingLobby);

            var lobbyObject = new GameObject("LobbyUI");
            Undo.RegisterCreatedObjectUndo(lobbyObject, "Create LobbyUI");

            if (lobbyAsset != null)
            {
                var lobbyDocument = Undo.AddComponent<UIDocument>(lobbyObject);
                lobbyDocument.panelSettings = panelSettings;
                lobbyDocument.visualTreeAsset = lobbyAsset;
            }

            var lobbyView = Undo.AddComponent<LobbyView>(lobbyObject);

            // Wire MainMenuView.Lobby → LobbyView
            var mainMenuSerialized = new SerializedObject(mainMenuView);
            var lobbyProperty = mainMenuSerialized.FindProperty("Lobby");
            if (lobbyProperty != null)
            {
                lobbyProperty.objectReferenceValue = lobbyView;
                mainMenuSerialized.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[UIToolkitSetup] Could not find 'Lobby' field on MainMenuView — wire it manually.");
            }

            RemoveOldCanvases("Main Menu");

            EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("[UIToolkitSetup] Main Menu scene setup complete.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static bool ConfirmActiveSceneIs(string expectedSceneName)
        {
            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (activeScene.name != expectedSceneName)
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Wrong scene?",
                    $"Active scene is '{activeScene.name}', expected '{expectedSceneName}'. Proceed anyway?",
                    "Yes", "Cancel");
                return proceed;
            }
            return true;
        }

        private static PanelSettings FindOrCreatePanelSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (existing != null) return existing;

            // Search anywhere in the project first
            var guids = AssetDatabase.FindAssets("t:PanelSettings");
            if (guids.Length > 0)
            {
                var found = AssetDatabase.LoadAssetAtPath<PanelSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (found != null)
                {
                    Debug.Log($"[UIToolkitSetup] Using existing PanelSettings: {AssetDatabase.GetAssetPath(found)}");
                    return found;
                }
            }

            // Create one
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;

            System.IO.Directory.CreateDirectory("Assets/Catan/UI/UIToolkit");
            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UIToolkitSetup] Created PanelSettings at {PanelSettingsPath}");
            return settings;
        }

        private static VisualTreeAsset LoadVisualTreeAsset(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            if (asset == null)
                Debug.LogError($"[UIToolkitSetup] Could not find UXML at '{path}'. Check that the file exists.");
            return asset;
        }

        private static Sprite FindSpriteInAtlas(string texturePath, string spriteName)
        {
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            return allAssets.OfType<Sprite>().FirstOrDefault(sprite => sprite.name == spriteName);
        }

        private static void WireActionButtonSprites(ActionButtonsView actionButtonsView)
        {
            var actionSerialized = new SerializedObject(actionButtonsView);

            SetSpriteField(actionSerialized, "SettlementSprite", CatanElementsPath, SpriteHouse);
            SetSpriteField(actionSerialized, "CitySprite",        CatanElementsPath, SpriteCityTile);
            SetSpriteField(actionSerialized, "RoadSprite",        CatanElementsPath, SpriteRoad);
            SetSpriteField(actionSerialized, "DevCardSprite",     CatanElementsPath, SpriteDevCard);

            actionSerialized.ApplyModifiedProperties();
        }

        private static void SetSpriteField(SerializedObject serializedObject, string fieldName,
                                            string texturePath, string spriteName)
        {
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning($"[UIToolkitSetup] Field '{fieldName}' not found — wire manually.");
                return;
            }

            var sprite = FindSpriteInAtlas(texturePath, spriteName);
            if (sprite == null)
            {
                Debug.LogWarning($"[UIToolkitSetup] Sprite '{spriteName}' not found in '{texturePath}' — wire manually.");
                return;
            }

            property.objectReferenceValue = sprite;
        }

        private static void WireBankTradePanelRef(ActionButtonsView actionButtonsView,
                                                   BankTradePanelView bankTradePanelView)
        {
            var serialized = new SerializedObject(actionButtonsView);
            var property   = serialized.FindProperty("BankTradePanel");
            if (property != null)
            {
                property.objectReferenceValue = bankTradePanelView;
                serialized.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[UIToolkitSetup] 'BankTradePanel' field not found on ActionButtonsView — wire manually.");
            }
        }

        private static void WireDevCardSprites(DevHandView devHandView)
        {
            var devHandSerialized = new SerializedObject(devHandView);
            var cardSpritesProperty = devHandSerialized.FindProperty("CardSprites");
            if (cardSpritesProperty == null)
            {
                Debug.LogWarning("[UIToolkitSetup] 'CardSprites' field not found on DevHandView — wire manually.");
                return;
            }

            cardSpritesProperty.ClearArray();

            var allDevCardAssets = AssetDatabase.LoadAllAssetsAtPath(CatanDevCardsPath);
            for (int mappingIndex = 0; mappingIndex < DevCardMapping.Length; mappingIndex++)
            {
                var (spriteIndex, cardId) = DevCardMapping[mappingIndex];
                var sprite = allDevCardAssets.OfType<Sprite>()
                    .FirstOrDefault(sprite => sprite.name == spriteIndex);

                cardSpritesProperty.InsertArrayElementAtIndex(mappingIndex);
                var entryProperty = cardSpritesProperty.GetArrayElementAtIndex(mappingIndex);
                entryProperty.FindPropertyRelative("CardId").stringValue = cardId;
                entryProperty.FindPropertyRelative("Sprite").objectReferenceValue = sprite;

                if (sprite == null)
                    Debug.LogWarning($"[UIToolkitSetup] Dev card sprite '{spriteIndex}' not found — wire '{cardId}' manually.");
            }

            devHandSerialized.ApplyModifiedProperties();
        }

        private static void WireGameManagerRefs(StealTargetPanelView stealTargetPanelView,
                                                 MonopolyPanelView monopolyPanelView,
                                                 YearOfPlentyPanelView yearOfPlentyView)
        {
            var gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("[UIToolkitSetup] GameManager not found in scene — wire MonopolyPanel, " +
                                 "YearOfPlentyPanel, StealTargetPanel manually.");
                return;
            }

            var gmSerialized = new SerializedObject(gameManager);
            SetComponentRef(gmSerialized, "StealTargetPanel",  stealTargetPanelView);
            SetComponentRef(gmSerialized, "MonopolyPanel",     monopolyPanelView);
            SetComponentRef(gmSerialized, "YearOfPlentyPanel", yearOfPlentyView);
            gmSerialized.ApplyModifiedProperties();
        }

        private static void SetComponentRef(SerializedObject serializedObject, string fieldName,
                                             Object targetComponent)
        {
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning($"[UIToolkitSetup] Field '{fieldName}' not found on {serializedObject.targetObject.name}.");
                return;
            }
            property.objectReferenceValue = targetComponent;
        }

        private static void RemoveOldCanvases(string contextLabel)
        {
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Where(canvas => canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                .ToArray();

            if (allCanvases.Length == 0) return;

            var canvasNames = string.Join("\n  • ", allCanvases.Select(canvas => canvas.gameObject.name));
            bool shouldDelete = EditorUtility.DisplayDialog(
                $"Remove old {contextLabel} Canvas?",
                $"Found {allCanvases.Length} Screen Space Overlay Canvas object(s):\n\n  • {canvasNames}\n\n" +
                $"Remove them? (This is undoable.)",
                "Remove", "Keep");

            if (!shouldDelete) return;

            foreach (var canvas in allCanvases)
                Undo.DestroyObjectImmediate(canvas.gameObject);

            Debug.Log($"[UIToolkitSetup] Removed {allCanvases.Length} legacy Canvas object(s).");
        }
    }
}
