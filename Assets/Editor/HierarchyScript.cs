using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HierarchyScript : EditorWindow
{
    // Ctrl (%) + g
    [MenuItem("Window/Custom Editor %g")]
    public static void ShowWindow()
    {
        //GetWindow<HierarchyScript>("Custom Editor");
        CreateGameObject();
    }

    private static void CreateGameObject()
    {
        // Get the currently selected GameObject in the Hierarchy
        GameObject parentObject = Selection.activeGameObject;

        // Check if a GameObject is selected
        if (parentObject != null)
        {
            // Create an empty child GameObject
            GameObject emptyChild = new GameObject("GameObject");

            // Make the empty GameObject a child of the selected GameObject
            emptyChild.transform.parent = parentObject.transform;

            // Reset the local position, rotation, and scale of the empty GameObject
            emptyChild.transform.localPosition = Vector3.zero;
            emptyChild.transform.localRotation = Quaternion.identity;
            emptyChild.transform.localScale = Vector3.one;

            // Select the newly created child GameObject
            Selection.activeGameObject = emptyChild;
        }
        else
        {
            Debug.LogWarning("Please select a GameObject in the Hierarchy to create an empty child.");
        }
    }
}
