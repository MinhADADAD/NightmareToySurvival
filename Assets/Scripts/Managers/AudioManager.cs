using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioClip backgroundMusic;
    public AudioSource music, effects;
    public float MusicVolume { get; private set; }
    public float EffectsVolume { get; private set; }
    void Awake()
    {
        Instance = this;
        SetMusic(PlayerPrefs.GetFloat("NTS.Music", .35f)); SetEffects(PlayerPrefs.GetFloat("NTS.Effects", .7f));
        music.clip = backgroundMusic; music.loop = true; music.Play();
    }
    public void Play(AudioClip clip) { if (clip) effects.PlayOneShot(clip, .65f); }
    public void SetMusic(float value) { MusicVolume = Mathf.Clamp01(value); music.volume = MusicVolume; PlayerPrefs.SetFloat("NTS.Music", MusicVolume); }
    public void SetEffects(float value) { EffectsVolume = Mathf.Clamp01(value); effects.volume = EffectsVolume; PlayerPrefs.SetFloat("NTS.Effects", EffectsVolume); }
    void OnDestroy() { PlayerPrefs.Save(); if (Instance == this) Instance = null; }
}
