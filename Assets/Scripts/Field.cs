using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    // All generated from map generator
    [SerializeField] public Transform holder;
    [SerializeField] public SpriteRenderer resourceIconRenderer;
    [SerializeField] public SpriteRenderer tileRenderer;
    [SerializeField] public ResourceType resourceType;
    [SerializeField] public int diceNumber;
    [SerializeField] public SpriteRenderer diceNumberRenderer;
    

    public void SetDiceNumber(int number)
    {
        CalculateProduction();
    }

    private void CalculateProduction()
    {
        
    }
    
    //private void SetUpPoints
}
