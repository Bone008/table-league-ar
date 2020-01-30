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
    TowerJamming,
    BallUnfreeze,
    NiceSave,
    ClockTick,
    TimeWarning,
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : NetworkBehaviour
{
    private const float FADE_OUT_DURATION = 0.15f;

    public static SoundManager Instance { get; private set; }

    public SoundEffect[] interruptibleEffects;
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
    public AudioClip clipTowerJamming;
    public AudioClip clipBallUnfreeze;
    public AudioClip clipNiceSave;
    public AudioClip clipClockTick;
    public AudioClip clipTimeWarning;

    private Dictionary<SoundEffect, AudioClip> clipsByEffect = new Dictionary<SoundEffect, AudioClip>();
    private Dictionary<SoundEffect, AudioSource> spawnedSourcesByEffect = new Dictionary<SoundEffect, AudioSource>();
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
        clipsByEffect.Add(SoundEffect.TowerJamming, clipTowerJamming);
        clipsByEffect.Add(SoundEffect.BallUnfreeze, clipBallUnfreeze);
        clipsByEffect.Add(SoundEffect.NiceSave, clipNiceSave);
        clipsByEffect.Add(SoundEffect.ClockTick, clipClockTick);
        clipsByEffect.Add(SoundEffect.TimeWarning, clipTimeWarning);

        // Completeness check.
        foreach (SoundEffect sound in System.Enum.GetValues(typeof(SoundEffect)))
        {
            if (!clipsByEffect.ContainsKey(sound))
                Debug.LogError("Sound effect " + sound + " has no associated clip! Please add it in SoundManager.");
        }
    }

    [Client]
    private void ClientPlaySound(SoundEffect sound)
    {
        if (!clipsByEffect.TryGetValue(sound, out AudioClip clip))
        {
            Debug.LogError("Cannot play sound effect without registered clip: " + sound);
            return;
        }
        if (clip == null)
        {
            Debug.LogWarning("Cannot play null sound effect: " + sound);
            return;
        }

        if (interruptibleEffects.Contains(sound))
        {
            if (spawnedSourcesByEffect.TryGetValue(sound, out var existingSource) && existingSource)
            {
                existingSource.Stop();
                Destroy(existingSource.gameObject);
            }

            var go = new GameObject("Audio-" + sound);
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.Play();
            spawnedSourcesByEffect[sound] = source;

            this.Delayed(clip.length + 0.1f, () =>
            {
                if (go) Destroy(go);
                if (spawnedSourcesByEffect[sound] == source) spawnedSourcesByEffect[sound] = null;
            });
        }
        else
        {
            source.PlayOneShot(clip);
        }
    }

    [Client]
    private void ClientStopSound(SoundEffect sound)
    {
        if (!interruptibleEffects.Contains(sound))
        {
            Debug.LogWarning($"Tried to stop sound effect \"{sound}\", but it is not marked as interruptible.", this);
            return;
        }

        if (spawnedSourcesByEffect.TryGetValue(sound, out var source) && source)
        {
            // Simply fade it out. The source will automatically be destroyed when the clip ends.
            this.AnimateScalar(FADE_OUT_DURATION, source.volume, 0, v =>
            {
                if (source) source.volume = v;
            }, true);
        }
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
        ClientStopSound(sound);
    }

    [ClientRpc]
    public void RpcStopSoundPlayer(SoundEffect sound, int playerId)
    {
        // TODO currently no good way to stop a specific sound that was played with PlayOneShot :(
        if (PlayerNetController.LocalInstance?.player?.playerId == playerId)
            ClientStopSound(sound);
    }
}
