using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class ResourceSettings
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
    [Tooltip("Production dots, values from 1-4, index 0 represents value 1")] 
    [SerializeField] private List<Sprite> productionDots;
    [SerializeField] private Color fieldNormalProductionTextColor;
    [SerializeField] private Color fieldMaxProductionTextColor;
    [SerializeField] private List<Transform> fieldHolders;
    [SerializeField] private List<Sprite> numberSprites;
    [SerializeField] private GameObject fieldPrefab;
    [SerializeField] private Transform fieldsParent;
    [SerializeField] private int maxFieldCount;
    [SerializeField] private bool fillRestWithDesert;

    
    // Maybe this should be default settings for map saved in scriptable object for it? 
    // TODO: For now, fields are already placed on locations.


    private void OnValidate()
    {
        // TODO: Number of resource count per map should be less than available fields. Rest fill with desert.
        // TODO: Or in CanGenerateMap
    }

    private void Awake()
    {
        GenerateMap();
    }

    public void Initialize()
    {
        
    }

    public void GenerateMap()
    {
        if (!CanGenerateMap())
        {
            Debug.Log("Cannot generate map. Check variables");
        }
        
        UpdateDesertCount();
        List<Field> fields = InstantiateFields();

        List<int> numberPossibilities = GetPossibleNumbers(2);
        List<int> resourcePossibilities = GetPossibleResourcesIndexes();
        GenerateFields(numberPossibilities, resourcePossibilities, fields);
        Debug.Log("Yes");
        //Check constraint list (list of bool functions, like 6 and 8 cannot be near, 2 of the same, 2 and 12.)
        
    }

    private int GetFieldsCount()
    {
        int fieldsCount = 0;
        foreach (ResourceSettings resourceSetting in resourceSettings)
        {
            fieldsCount += resourceSetting.countPerMap;
        }

        return fieldsCount;
    }

    public bool CanGenerateMap()
    {
        int fieldsCount = GetFieldsCount();
        
        // If no filling with desert, maxCount must be same as filed count, meaning you add deserts manually.
        // Also if 
        if ((!fillRestWithDesert && fieldsCount != maxFieldCount) || (fillRestWithDesert && fieldsCount > maxFieldCount))
        {
            return false;
        }
        
        if (fillRestWithDesert && fieldsCount > maxFieldCount){ // Ovo sve treba u onvalidate, ne treba u produkciji. Jedino ako  //se ne setuje iz koda
            return false;
        }

        return true;
    }

    public void UpdateDesertCount()
    {
        int fieldsCount = GetFieldsCount();
        
        //primer 4 4 4 3 3 1 je normalno. ako stavis 4 4 4 3 3 0 da dopuni 1. 19 je maxcount podesen, a 18 kad se ovde saberu. znaci jos 1 pustinja da se doda
        if (fillRestWithDesert && fieldsCount < maxFieldCount)
        {
            ResourceSettings desertResource = resourceSettings.FirstOrDefault(res => res.resourceType == ResourceType.Desert);
            if (desertResource != null)
            {
                desertResource.countPerMap += maxFieldCount - fieldsCount;
            }
        }
    }

    private List<Field> InstantiateFields()
    {
        List<Field> fields = new List<Field>();
        for (int i = 0; i < maxFieldCount; i++)
        {
            GameObject fieldObject = Instantiate(fieldPrefab, fieldHolders[i].position, Quaternion.identity, fieldsParent);
            if (fieldObject == null)
            {
                Debug.Log("Cannot create field object");
                return null;
            }
            Field field = fieldObject.GetComponent<Field>();
            if (field == null)
            {
                Debug.Log("Cannot find field component");
                return null;
            }

            field.holder = fieldHolders[i];
            fields.Add(field);
        }

        return fields;
    }

    private List<int> GetPossibleNumbers(int numberOfDices)
    {
        // for now, only 2 dices and hardcoded solution
        return new List<int> {2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12};
    }

    private List<int> GetPossibleResourcesIndexes()
    {
        List<int> possibleResourceIndexes = new List<int>();
        for (int i = 0; i < resourceSettings.Count; i++)
        {
            ResourceSettings resourceSetting = resourceSettings[i];
            for (int j = 0; j < resourceSetting.countPerMap; j++)
            {
                possibleResourceIndexes.Add(i);
            }
        }

        return possibleResourceIndexes;
    }

    private void GenerateFields(List<int> possibleNumbers, List<int> possibleResources, List<Field> fields)
    {
        Random random = new Random();
        foreach (Field field in fields)
        {
            int chosenResourceIndex = random.Next(0, possibleResources.Count); // exclusive upper bound?
            ResourceSettings chosenResource = resourceSettings[possibleResources[chosenResourceIndex]];
            
            if (chosenResource.resourceType == ResourceType.Desert)
            {
                field.resourceIconRenderer.sprite = null;
                field.diceNumberRenderer.sprite = null;
                field.productionTextMeshPro.text = "";
                field.productionRenderer.sprite = null;
                field.infoWindowRenderer.sprite = null;
            }
            else
            {
                int chosenNumberIndex = random.Next(0, possibleNumbers.Count - 1);
                int chosenNumber = possibleNumbers[chosenNumberIndex];
                field.resourceIconRenderer.sprite = chosenResource.resourceIcon;
                field.diceNumber = chosenNumber;
                //TODO: MAYBE REMOVE
                //field.diceNumberRenderer.sprite = numberSprites[chosenNumber];
                possibleNumbers.RemoveAt(chosenNumberIndex);
                field.production = CalculateProduction(2, chosenNumber);
                field.productionRenderer.sprite = productionDots[field.production - 1];
                field.productionTextMeshPro.text = chosenNumber.ToString();
                if (field.production == 5)
                {
                    field.productionRenderer.color = fieldMaxProductionTextColor;
                    field.diceNumberRenderer.color = fieldMaxProductionTextColor;
                    field.productionTextMeshPro.color = fieldMaxProductionTextColor;
                }
                else
                {
                    field.productionRenderer.color = fieldNormalProductionTextColor;
                    field.diceNumberRenderer.color = fieldNormalProductionTextColor;
                    field.productionTextMeshPro.color = fieldNormalProductionTextColor;
                }
                //field.diceNumberRenderer.sprite = cho;
            }
            
            // For each resource
            field.tileRenderer.color = chosenResource.tileColor;
            field.resourceType = chosenResource.resourceType;

            
            possibleResources.RemoveAt(chosenResourceIndex);
        }
    }

    private int CalculateProduction(int numberOfDices, int diceNumber)
    {
        // TODO: Hardcoded for 2 dices. Change that later
        if (numberOfDices != 2)
        {
            Debug.LogError("Not yer defined for != 2 dices");
            return 0;
        }

        if (diceNumber > 7)
        {
            diceNumber = 14 - diceNumber;
        }

        return diceNumber - 1;
    }
}