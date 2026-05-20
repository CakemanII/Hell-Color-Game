using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider musicSlider;

    private void OnEnable()
    {
        if (AudioManager.Instance == null) return;
        musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
    }

    public void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnResumeButtonClicked()
    {
        GameManager.Instance.TogglePause();
    }

    public void OnMainMenuButtonClicked()
    {
        SceneLoader.Instance.LoadMainMenu();
    }
}
