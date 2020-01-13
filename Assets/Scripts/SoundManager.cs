using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * To add a new sound effect, you have to:
 *  1) Add entry to SoundEffect enum.
 *  2) Add public AudioField field to SoundManager.
 *  3) Add mapping to clipsByEffect dictionary in SoundManager.Awake().
 *  4) Assign respective AudioClip in Unity inspector.
 */
public enum SoundEffect
{
    ButtonClick,
    Invalid,
    Time,
    GameStart,
    BallHit,
    GoalScore,
    CollectableSpawn,
    CollectableCollecting,
    CollectableCollected,
    TowerBuilding,
    TowerDestroying,
    MagnetActivate,
    BarrierKnockOver,
    BallFreeze,
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : NetworkBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioClip clipButtonClick;
    public AudioClip clipInvalid;
    public AudioClip clipTime;
    public AudioClip clipGameStart;
    public AudioClip clipBallHit;
    public AudioClip clipGoalScore;
    public AudioClip clipCollectableSpawn;
    public AudioClip clipCollectableCollecting;
    public AudioClip clipCollectableCollected;
    public AudioClip clipTowerBuilding;
    public AudioClip clipTowerDestroying;
    public AudioClip clipMagnetActivate;
    public AudioClip clipBarrierKnockOver;
    public AudioClip clipBallFreeze;

    private Dictionary<SoundEffect, AudioClip> clipsByEffect = new Dictionary<SoundEffect, AudioClip>();
    private AudioSource source;

    void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();

        clipsByEffect.Add(SoundEffect.ButtonClick, clipButtonClick);
        clipsByEffect.Add(SoundEffect.Invalid, clipInvalid);
        clipsByEffect.Add(SoundEffect.Time, clipTime);
        clipsByEffect.Add(SoundEffect.GameStart, clipGameStart);
        clipsByEffect.Add(SoundEffect.BallHit, clipBallHit);
        clipsByEffect.Add(SoundEffect.GoalScore, clipGoalScore);
        clipsByEffect.Add(SoundEffect.CollectableSpawn, clipCollectableSpawn);
        clipsByEffect.Add(SoundEffect.CollectableCollecting, clipCollectableCollecting);
        clipsByEffect.Add(SoundEffect.CollectableCollected, clipCollectableCollected);
        clipsByEffect.Add(SoundEffect.TowerBuilding, clipTowerBuilding);
        clipsByEffect.Add(SoundEffect.TowerDestroying, clipTowerDestroying);
        clipsByEffect.Add(SoundEffect.MagnetActivate, clipMagnetActivate);
        clipsByEffect.Add(SoundEffect.BarrierKnockOver, clipBarrierKnockOver);
        clipsByEffect.Add(SoundEffect.BallFreeze, clipBallFreeze);
        
        // Completeness check.
        foreach(SoundEffect sound in System.Enum.GetValues(typeof(SoundEffect)))
        {
            if (!clipsByEffect.ContainsKey(sound))
                Debug.LogError("Sound effect " + sound + " has no associated clip! Please add it in SoundManager.");
        }
    }

    [Client]
    private void ClientPlaySound(SoundEffect sound)
    {
        if(!clipsByEffect.TryGetValue(sound, out AudioClip clip))
        {
            Debug.LogError("Cannot play sound effect without registered clip: " + sound);
            return;
        }
        if(clip == null)
        {
            Debug.LogWarning("Cannot play null sound effect: " + sound);
            return;
        }
        
        source.PlayOneShot(clip);
    }

    [ClientRpc]
    public void RpcPlaySoundAll(SoundEffect sound)
    {
        ClientPlaySound(sound);
    }

    [ClientRpc]
    public void RpcPlaySoundPlayer(SoundEffect sound, int playerId)
    {
        if (PlayerNetController.LocalInstance?.player?.playerId == playerId)
            ClientPlaySound(sound);
    }


    [ClientRpc]
    public void RpcStopSoundAll(SoundEffect sound)
    {
        // TODO currently no good way to stop a specific sound that was played with PlayOneShot :(
        source.Stop();
    }

    [ClientRpc]
    public void RpcStopSoundPlayer(SoundEffect sound, int playerId)
    {
        // TODO currently no good way to stop a specific sound that was played with PlayOneShot :(
        if (PlayerNetController.LocalInstance?.player?.playerId == playerId)
            source.Stop();
    }
}
