using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MareGameplay
{
    enum Type
    {
        Houses,
        Roads,
        Thieves
    }
    
    public class CardController : MonoBehaviour
    {
        [SerializeField] private Type type;
        [SerializeField] private int maxCount;
        [SerializeField] private int currentCount;

        [SerializeField] private TextMeshProUGUI currentCountText;

        private void Start()
        {
            currentCountText.text = currentCount.ToString();
        }

        public void Select()
        {
            Use();
        }

        private void Use()
        {
            if (currentCount == 0)
            {
                Debug.Log($"Not enough: {type}!");
                return;
            }

            currentCount--;
            currentCountText.text = currentCount.ToString();
        }
    }
}

