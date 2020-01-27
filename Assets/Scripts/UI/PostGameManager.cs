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
    public static GameStatistics opStatistics = null;

    public TMPro.TextMeshProUGUI resultText;
    public TMPro.TextMeshProUGUI scoreOwn;
    public TMPro.TextMeshProUGUI scoreOpponent;
    public TMPro.TextMeshProUGUI ballHits;
    public TMPro.TextMeshProUGUI ballHitsOp;
    public TMPro.TextMeshProUGUI towersBuilt;
    public TMPro.TextMeshProUGUI towersBuiltOp;
    public TMPro.TextMeshProUGUI saves;
    public TMPro.TextMeshProUGUI savesOp;
    public TMPro.TextMeshProUGUI powerupsUsed;
    public TMPro.TextMeshProUGUI powerupsUsedOp;
    public TMPro.TextMeshProUGUI DistanceTravelled;
    public TMPro.TextMeshProUGUI DistanceTravelledOp;

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
                resultText.text = "You WON!";
                break;
            case WinResult.Loser:
                resultText.text = "You LOST!";
                break;
            case WinResult.Draw:
                resultText.text = "DRAW!";
                break;
        }
        scoreOwn.text = statistics.finalScoreSelf.ToString();
        scoreOpponent.text = statistics.finalScoreOpponent.ToString();
        ballHits.text = statistics.numberOfBallHits.ToString();
        ballHitsOp.text = opStatistics.numberOfBallHits.ToString();
        towersBuilt.text = statistics.towersBuilt.ToString();
        towersBuiltOp.text = opStatistics.towersBuilt.ToString();
        saves.text = statistics.saves.ToString();
        savesOp.text = opStatistics.saves.ToString();
        powerupsUsed.text = statistics.powerupsUsed.ToString();
        powerupsUsedOp.text = opStatistics.powerupsUsed.ToString();
        DistanceTravelled.text = System.Math.Round(statistics.distanceTravelled, 2).ToString() + "m";
        DistanceTravelledOp.text = System.Math.Round(opStatistics.distanceTravelled, 2).ToString() + "m";
    }
}
