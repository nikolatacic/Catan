using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MareGameplay
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] private List<HousePlaceholder> houses;

        private void Awake()
        {
            houses.ForEach(house => house.Initialize());
        }
    }
}
