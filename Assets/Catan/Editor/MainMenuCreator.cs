using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

namespace Catan.UI.Editor
{
    public static class MainMenuCreator
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Catan/Create Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            EnsureScenesFolder();

            // Create a blank scene and populate it
            var scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);

            BuildScene();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, false);

            UpdateBuildSettings();

            AssetDatabase.Refresh();

            Debug.Log(
                "[Catan] MainMenu scene created at " + ScenePath + "\n" +
                "Build Settings updated: MainMenu=0, SampleScene=1.\n" +
                "Rename SampleScene to 'GameHotseat' when ready " +
                "(Right-click in Project window → Rename).");
        }

        // ── Scene construction ─────────────────────────────────────────────────

        private static void BuildScene()
        {
            // Camera
            var cameraGo = new GameObject("Main Camera");
            var camera   = cameraGo.AddComponent<Camera>();
            camera.clearFlags       = CameraClearFlags.SolidColor;
            camera.backgroundColor  = new Color(0.08f, 0.08f, 0.12f);
            camera.orthographic     = true;
            cameraGo.tag            = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            // Event system (required for UI clicks)
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            // Canvas
            var canvasGo = new GameObject("Canvas");
            var canvas   = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight  = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // MainMenuView on the canvas
            var menuView = canvasGo.AddComponent<MainMenuView>();

            // Root vertical layout (centres content)
            var contentGo   = new GameObject("Content");
            contentGo.transform.SetParent(canvasGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin        = new Vector2(0.5f, 0.5f);
            contentRect.anchorMax        = new Vector2(0.5f, 0.5f);
            contentRect.pivot            = new Vector2(0.5f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta        = new Vector2(600f, 0f);
            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment        = TextAnchor.MiddleCenter;
            vlg.spacing               = 60f;
            vlg.childControlWidth     = true;
            vlg.childControlHeight    = false;
            vlg.childForceExpandWidth = true;
            contentGo.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            // Title label
            var titleGo = CreateChild(contentGo, "TitleLabel");
            SetLayoutElement(titleGo, preferredHeight: 120f);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text      = "CATAN";
            titleTmp.fontSize  = 96;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color     = new Color(0.95f, 0.85f, 0.35f);

            // Subtitle
            var subtitleGo = CreateChild(contentGo, "SubtitleLabel");
            SetLayoutElement(subtitleGo, preferredHeight: 40f);
            var subtitleTmp = subtitleGo.AddComponent<TextMeshProUGUI>();
            subtitleTmp.text      = "Hotseat Edition";
            subtitleTmp.fontSize  = 28;
            subtitleTmp.fontStyle = FontStyles.Italic;
            subtitleTmp.alignment = TextAlignmentOptions.Center;
            subtitleTmp.color     = new Color(0.70f, 0.70f, 0.70f);

            // Play button
            var playGo  = CreateChild(contentGo, "PlayButton");
            SetLayoutElement(playGo, preferredHeight: 90f);
            var playImg = playGo.AddComponent<Image>();
            playImg.color = new Color(0.20f, 0.65f, 0.25f);
            var playBtn = playGo.AddComponent<Button>();
            playBtn.targetGraphic = playImg;
            var playColors = playBtn.colors;
            playColors.highlightedColor = new Color(0.28f, 0.80f, 0.32f);
            playColors.pressedColor     = new Color(0.14f, 0.48f, 0.18f);
            playBtn.colors = playColors;

            var playLabelGo  = CreateChild(playGo, "Label");
            StretchFull(playLabelGo.GetComponent<RectTransform>());
            var playLabelTmp = playLabelGo.AddComponent<TextMeshProUGUI>();
            playLabelTmp.text      = "PLAY";
            playLabelTmp.fontSize  = 42;
            playLabelTmp.fontStyle = FontStyles.Bold;
            playLabelTmp.alignment = TextAlignmentOptions.Center;
            playLabelTmp.color     = Color.white;

            // Wire Play button → MainMenuView.OnPlay (persistent, survives scene save)
            UnityEventTools.AddPersistentListener(
                playBtn.onClick,
                menuView.OnPlay);
        }

        // ── Build Settings ─────────────────────────────────────────────────────

        private static void UpdateBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
            };
            EditorBuildSettings.scenes = scenes;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static GameObject CreateChild(GameObject parent, string childName)
        {
            var go = new GameObject(childName);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void SetLayoutElement(GameObject go, float preferredHeight)
        {
            go.AddComponent<LayoutElement>().preferredHeight = preferredHeight;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }
}
