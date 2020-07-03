using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer mixer;
    public Dropdown resolutionDropdown;
    public Dropdown graphicsDropdown;
    public Toggle fullscreenToggle;
    public Slider volumeSlider;

    Resolution[] resolutions;
    private void Start()
    {
        SetupSettings();
    }

    /// <summary>
    /// Also sets up audio
    /// </summary>
    public void SetupSettings ()
    {
        resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToArray();
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        graphicsDropdown.value = QualitySettings.GetQualityLevel();
        fullscreenToggle.isOn = Screen.fullScreen;
        float volume;
        mixer.GetFloat("volume", out volume);

        volumeSlider.value = Mathf.Pow(10 ,volume / 20);
    }

    public void SetResolution (int index)
    {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void SetVolume(float volume)
    {
        mixer.SetFloat("volume", Mathf.Log10(volume) * 20);
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    public void Resume()
    {
        FindObjectOfType<Pause>().UnPause();
    }
}
