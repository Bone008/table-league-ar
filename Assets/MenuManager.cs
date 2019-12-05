using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private static int noOfBalls = 2;
    private static int winningPoint = 5;
    private int previousMenu = 0;

    public GameObject playMenu;
    public GameObject multiPlayer;
    public Button defaultBalls;
    public Button defaultPoints;
    
    // Start is called before the first frame update
    void Start()
    {
        defaultBalls.interactable = false;
        defaultPoints.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConfigBack()
    {
        if (previousMenu == 0)
        {
            playMenu.SetActive(true);
        }
        else
        {
            multiPlayer.SetActive(true);
        }
    }

    void SinglePlayer()
    {
        previousMenu = 0;
    }

    void CreateGame()
    {
        previousMenu = 1;
    }
    
    public void SetBalls(int balls)
    {
        noOfBalls = balls;
    }

    public void SetPoints(int points)
    {
        winningPoint = points;
    }
    
}
