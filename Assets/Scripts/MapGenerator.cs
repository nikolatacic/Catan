using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ResourceSettings
{
    // TODO: Umesto name, da ide enum?
    public ResourceType resourceType;
    public Color tileColor;
    public Sprite resourceIcon;
    public int countPerMap;
}
public class MapGenerator : MonoBehaviour
{
    [SerializeField] private List<ResourceSettings> resourceSettings;
    [SerializeField] private List<Transform> fieldHolders;
    [SerializeField] private GameObject fieldPrefab;
    [SerializeField] private Transform fieldsParent;
    
    
    // TODO: For now, fields are allready placed on locations.


    private void OnValidate()
    {
        // TODO: Number of resource count per map should be less than available fields. Rest fill with desert.
        // TODO: Or in CanGenerateMap
    }

    public void Initialize()
    {
        
    }

    public void GenerateMap()
    {
        // Instantiate prefabs and get references to their class. 
        // Create Resource map. (Same resources can touch?)
        // Fully random resource placement from ResourceSettings. Set up sprites and colors. 
        //Number generation.
        // 2 3 4 5 6 8 9 10 11 12
        
        //Check constraint list (list of bool functions, like 6 and 8 cannot be near, 2 of the same, 2 and 12.)
    }
}
