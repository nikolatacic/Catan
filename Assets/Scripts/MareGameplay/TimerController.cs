using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MareGameplay
{
    public class TimerController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [Header("Initial timer time:")]
        [SerializeField] private float timeRemaining = 120; 
        
        bool isRunning = true;
        
        private void Start()
        {
            Update();
        }

        private void Update()
        {
            if (isRunning)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining < 0)
                {
                    timeRemaining = 0;
                    isRunning = false;
                    Invoke(nameof(ResetTimer), 1f); 
                }

                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);

                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        private void ResetTimer()
        {
            timeRemaining = 120; 
            isRunning = true;
        }
    }
}

