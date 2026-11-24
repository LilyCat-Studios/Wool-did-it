using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Audio Mixer")] [SerializeField]
    private AudioMixer mixer;

    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambianceSlider;
    [SerializeField] private Slider proximityChatSlider;

    private void Start()
    {
        LoadSettings();
    }

    public void SetMasterVolume()
    {
        SetVolume(masterSlider, "MasterVolume");
    }

    public void SetMusicVolume()
    {
        SetVolume(musicSlider, "MusicVolume");
    }

    public void SetSfxVolume()
    {
        SetVolume(sfxSlider, "SFXVolume");
    }

    public void SetAmbianceVolume()
    {
        SetVolume(ambianceSlider, "AmbianceVolume");
    }

    public void SetProximityChatVolume()
    {
        SetVolume(proximityChatSlider, "ProximityChatVolume");
    }
    private void SetVolume(Slider slider, string parameter)
    {
       float sliderValue = slider.value;
       float volume = Mathf.Log10(sliderValue) * 20f;
       mixer.SetFloat(parameter, volume);
       PlayerPrefs.SetFloat(parameter, sliderValue);
    }

    private void LoadSettings()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        ambianceSlider.value = PlayerPrefs.GetFloat("AmbianceVolume", 0.75f);
        proximityChatSlider.value = PlayerPrefs.GetFloat("ProximityChatVolume", 0.75f);
    }

    private void LoadVolume(Slider slider, string parameter)
    {
        float savedValue = PlayerPrefs.GetFloat(parameter, 0.75f);
        slider.SetValueWithoutNotify(savedValue);
        SetVolume(slider, parameter);
    }
}
