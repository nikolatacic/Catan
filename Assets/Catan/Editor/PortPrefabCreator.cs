using System.IO;
using UnityEngine;
using UnityEditor;
using Catan.UI;

namespace Catan.Editor
{
    public static class PortPrefabCreator
    {
        private const string PrefabPath = "Assets/Catan/Prefabs/PortView.prefab";

        [MenuItem("Catan/Board/Create Port Prefab")]
        public static void CreatePortPrefab()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));

            var rootGo = new GameObject("PortView");

            var backgroundRenderer = rootGo.AddComponent<SpriteRenderer>();
            backgroundRenderer.sortingLayerName = "Default";
            backgroundRenderer.sortingOrder = 0;

            var iconGo = new GameObject("ResourceIcon");
            iconGo.transform.SetParent(rootGo.transform, false);
            iconGo.transform.localPosition = Vector3.zero;
            iconGo.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            var iconRenderer = iconGo.AddComponent<SpriteRenderer>();
            iconRenderer.sortingLayerName = "Default";
            iconRenderer.sortingOrder = 1;

            var portView = rootGo.AddComponent<PortView>();
            portView.BackgroundRenderer = backgroundRenderer;
            portView.ResourceIconRenderer = iconRenderer;

            var prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath);
            Object.DestroyImmediate(rootGo);

            AssetDatabase.Refresh();

            if (prefab != null)
                Debug.Log($"[PortPrefabCreator] Port prefab created at {PrefabPath}. " +
                          "Assign it to BoardRenderer.PortPrefab in the scene, then assign port sprites.");
            else
                Debug.LogError("[PortPrefabCreator] Failed to create port prefab.");
        }
    }
}
