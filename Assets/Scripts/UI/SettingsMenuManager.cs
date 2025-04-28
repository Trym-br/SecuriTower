using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuManager : MonoBehaviour {
    [Header("UI Elements")] public GameObject settingsMenuHolder;
    public GameObject pauseMenuHolder;
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;
    public Slider voSlider;
    public GameObject mainMenuButtons;

    public bool fromPauseMenu;

    void Start() {
        settingsMenuHolder.SetActive(false);
    }

    public void ActivateSettingsMenu(bool parameterFromPauseMenu) {
        if (parameterFromPauseMenu) {
            fromPauseMenu = true;
            pauseMenuHolder.SetActive(false);
        }
        else if (!fromPauseMenu) {
            mainMenuButtons.SetActive(false);
        }
        PlayerController.instance.inMenu = true;
        settingsMenuHolder.SetActive(true);
        masterSlider.Select();
    }

    public void DeactivateSettingsMenu() {
        if (fromPauseMenu) {
            PauseMenuManager.instance.ActivatePauseMenu(true);
            fromPauseMenu = false;
        }
        else {
            MainMenuManager.instance.EnableMainMenu();
            mainMenuButtons.SetActive(true);
        }
        settingsMenuHolder.SetActive(false);
    }

    public void SetMasterVolume() {
        if (FMODController.instance == null) return;
        FMODController.instance.SetVolume(FMODController.VolumeSlider.Master, masterSlider.value);
    }

    public void SetSFXVolume() {
        if (FMODController.instance == null) return;
        FMODController.instance.SetVolume(FMODController.VolumeSlider.SoundEffects, sfxSlider.value);
    }

    public void SetMusicVolume() {
        if (FMODController.instance == null) return;
        FMODController.instance.SetVolume(FMODController.VolumeSlider.Music, musicSlider.value);
    }

    public void SetVOVolume() {
        if (FMODController.instance == null) return;
        FMODController.instance.SetVolume(FMODController.VolumeSlider.VoiceLines, voSlider.value);
    }
}