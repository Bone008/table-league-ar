using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Singleton responsible showing information about the game on the UI.</summary>
public class GameUIManager : MonoBehaviour
{
    public Text score1Text;
    public Text score2Text;
    public Text resourceText;

    public void UpdateScores(int score1, int score2)
    {
        score1Text.text = score1.ToString();
        score2Text.text = score2.ToString();
    }

    public void UpdateResource(int resource)
    {
        resourceText.text = resource.ToString();
    }
}
