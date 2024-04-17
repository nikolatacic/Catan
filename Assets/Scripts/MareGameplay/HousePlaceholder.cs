using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MareGameplay
{
    public class HousePlaceholder : PlaceholderElement
    {
        [SerializeField] private List<PlaceholderElement> neighbourHouses;

        public void Initialize()
        {
            OnSelected += DisableNeighbours;
        }

        private void DisableNeighbours()
        {
            neighbourHouses.ForEach(house => house.IsAvailable = false);
        }
    }
}
