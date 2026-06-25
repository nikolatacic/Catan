using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Catan.Network;

namespace Catan.UI.Editor
{
    public static class NetworkBridgeCreator
    {
        private const string GameScenePath = "Assets/Scenes/GameHotseat.unity";

        [MenuItem("Catan/Add Network Bridges to GameHotseat")]
        public static void AddBridgesToGameHotseat()
        {
            if (!System.IO.File.Exists(GameScenePath))
            {
                EditorUtility.DisplayDialog("GameHotseat missing",
                    $"Could not find {GameScenePath}.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

            var existing = Object.FindObjectOfType<NetworkCommandBridge>();
            GameObject host;
            if (existing != null)
            {
                host = existing.gameObject;
                Debug.Log("[Catan] NetworkCommandBridge already present; reusing GameObject.");
            }
            else
            {
                host = new GameObject("NetworkBridges");
                host.AddComponent<NetworkObject>();
                host.AddComponent<NetworkCommandBridge>();
                Debug.Log("[Catan] Created NetworkBridges GameObject with NetworkCommandBridge.");
            }

            if (host.GetComponent<NetworkEventBridge>() == null)
            {
                host.AddComponent<NetworkEventBridge>();
                Debug.Log("[Catan] Added NetworkEventBridge to NetworkBridges.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log(
                "[Catan] Network bridges ready in GameHotseat.\n" +
                "Clients now route commands to the host AND receive host events back.");
        }
    }
}
