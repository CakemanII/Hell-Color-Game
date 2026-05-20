using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("Music")]
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip[] mainMusic;

    private const string MusicVolKey = "MusicVolume";
    private const string SFXVolKey = "SFXVolume";

    private int currentMainMusicIndex = 0;
    private bool isBossMusicPlaying = false;

    public float MusicVolume => musicAudioSource.volume;
    public float SFXVolume => sfxAudioSource.volume;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolKey, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFXVolKey, 1f));
    }

    public void SetMusicVolume(float volume)
    {
        musicAudioSource.volume = volume;
        PlayerPrefs.SetFloat(MusicVolKey, volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxAudioSource.volume = volume;
        PlayerPrefs.SetFloat(SFXVolKey, volume);
    }

    public void SelectMainMusic()
    {
        currentMainMusicIndex = (currentMainMusicIndex + 1) % mainMusic.Length;
    }

    public void SetSelectBossMusic(bool play)
    {
        isBossMusicPlaying = play;
    }

    public void PlayMusic()
    {
        if (isBossMusicPlaying)
        {
            musicAudioSource.clip = bossMusic;
        }
        else
        {
            musicAudioSource.clip = mainMusic[currentMainMusicIndex];
        }
        musicAudioSource.Play();
    }
}
