using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public enum GameState
    {
        FirstSettlementPlacement,
        SecondSettlementPlacement,
        Gameplay,
        Results
    }
    
    [SerializeField] private List<Player> players;
    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        
    }
}
