using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// every sound effect in the game, by name
public enum Sfx
{
    BagPickup, Blast, CandyPickup, ColorEffect, GameOver, GhostAppear, GhostDeath, GhostHit, KidSpawn,
    OrderFail, OrderSuccess, PlayerHurt, PowerOutage, TankAppear, ThiefSteal, TrashDump, UiClick, YouWin
}

// plays the looping background music and every sound effect
// survives Restart (DontDestroyOnLoad)

public class AudioManager : MonoBehaviour
{
    // one row in the Inspector list: which sound, which file, how loud
    [Serializable]
    private class SoundEntry
    {
        public Sfx sound;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip music;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.35f;

    [Header("Sound effects")]
    [SerializeField] private SoundEntry[] sounds;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private readonly Dictionary<Sfx, SoundEntry> lookup = new Dictionary<Sfx, SoundEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // on restart: keep old AudioManager, hook up new scene, remove this copy
            Instance.SetUpScene();
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // only works on root objects

        // two speakers: one for music, one shared by all sound effects
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        // build the name -> sound table, and warn about anything missing
        foreach (SoundEntry s in sounds)
        {
            if (s.clip == null) continue;
            lookup[s.sound] = s;
            if (s.volume <= 0f) Debug.LogWarning($"AudioManager: {s.sound} has volume 0");
        }
        foreach (Sfx s in Enum.GetValues(typeof(Sfx)))
        {
            if (!lookup.ContainsKey(s)) Debug.LogWarning($"AudioManager: no clip set for {s}");
        }

        SetUpScene();
    }

    // runs once per scene load: every button clicks, and the music (re)starts
    private void SetUpScene()
    {
        foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            b.onClick.AddListener(PlayClick);
        }

        if (!musicSource.isPlaying) musicSource.Play(); // the game over screen stops it
    }

    private void PlayClick()
    {
        Play(Sfx.UiClick);
    }

    public static void Play(Sfx sound)
    {
        if (Instance == null) return;
        if (!Instance.lookup.TryGetValue(sound, out SoundEntry s)) return;
        Instance.sfxSource.PlayOneShot(s.clip, s.volume);
    }

    public static void StopMusic()
    {
        if (Instance != null) Instance.musicSource.Stop();
    }
}