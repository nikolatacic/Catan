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

            var portIconRenderer = rootGo.AddComponent<SpriteRenderer>();
            portIconRenderer.sortingLayerName = "Default";
            portIconRenderer.sortingOrder = 2;

            var portView = rootGo.AddComponent<PortView>();
            portView.PortIconRenderer = portIconRenderer;

            var prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath);
            Object.DestroyImmediate(rootGo);

            AssetDatabase.Refresh();

            if (prefab != null)
                Debug.Log($"[PortPrefabCreator] Port prefab created at {PrefabPath}. " +
                          "Assign it to BoardRenderer.PortPrefab. " +
                          "Assign port sprites to BoardRenderer.ResourcePortSprites[0-4] and GenericPortSprite.");
            else
                Debug.LogError("[PortPrefabCreator] Failed to create port prefab.");
        }
    }
}
