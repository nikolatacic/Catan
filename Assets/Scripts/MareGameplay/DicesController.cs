using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MareGameplay
{
    public class DicesController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI firstDiceGeneratedNumber;
        [SerializeField] private TextMeshProUGUI secondDiceGeneratedNumber;

        private int randomGeneratedNumber;
        
        public void Roll()
        {
            randomGeneratedNumber = Random.Range(1, 7);
            firstDiceGeneratedNumber.text = randomGeneratedNumber.ToString();
            
            randomGeneratedNumber = Random.Range(1, 7);
            secondDiceGeneratedNumber.text = randomGeneratedNumber.ToString();
        }
    }
}

