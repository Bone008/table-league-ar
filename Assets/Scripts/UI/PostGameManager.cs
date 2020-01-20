using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PostGameManager : MonoBehaviour
{
    /// <summary>Stats of the game. This should be set before loading this scene.</summary>
    public static GameStatistics statistics = null;

    public TMPro.TextMeshProUGUI resultText;
    public TMPro.TextMeshProUGUI scoreOwn;
    public TMPro.TextMeshProUGUI scoreOpponent;

    void Start()
    {
        if (statistics == null)
        {
            Debug.LogWarning("No statistics were found :(");
            return;
        }

        switch(statistics.result)
        {
            case WinResult.Winner:
                resultText.text = "You WON the game!";
                break;
            case WinResult.Loser:
                resultText.text = "You LOST the game!";
                break;
            case WinResult.Draw:
                resultText.text = "The game was a DRAW!";
                break;
        }
        scoreOwn.text = statistics.finalScoreSelf.ToString();
        scoreOpponent.text = statistics.finalScoreOpponent.ToString();
    }
}
