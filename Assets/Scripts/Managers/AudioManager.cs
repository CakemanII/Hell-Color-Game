using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicAudioSource;

    [Header("Music")]
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip[] mainMusic;

    private const string MusicVolKey = "MusicVolume";

    private int currentMainMusicIndex = 0;
    private bool isBossMusicPlaying = false;

    public float MusicVolume => musicAudioSource.volume;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolKey, 1f));
        PlayMusic();
    }

    public void SetMusicVolume(float volume)
    {
        musicAudioSource.volume = volume;
        PlayerPrefs.SetFloat(MusicVolKey, volume);
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

    public void PauseAudios()
    {
        musicAudioSource.Pause();
    }
}
