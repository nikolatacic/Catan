using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Catan.Network;

namespace Catan.UI.Editor
{
    public static class NetworkBridgeCreator
    {
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";

        [MenuItem("Catan/Add Network Command Bridge to MainScene")]
        public static void AddBridgeToMainScene()
        {
            if (!System.IO.File.Exists(MainScenePath))
            {
                EditorUtility.DisplayDialog("MainScene missing",
                    $"Could not find {MainScenePath}.", "OK");
                return;
            }

            var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            if (Object.FindObjectOfType<NetworkCommandBridge>() != null)
            {
                EditorUtility.DisplayDialog("Already present",
                    "NetworkCommandBridge already exists in MainScene.", "OK");
                return;
            }

            var go = new GameObject("NetworkCommandBridge");
            go.AddComponent<NetworkObject>();
            go.AddComponent<NetworkCommandBridge>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log(
                "[Catan] NetworkCommandBridge added to MainScene.\n" +
                "Clients now route their commands to the host via ServerRpc.");
        }
    }
}
