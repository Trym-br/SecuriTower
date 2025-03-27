using UnityEngine;
using UnityEngine.UI;
public class SettingsMenuManager : MonoBehaviour {
    [Header("UI Elements")] 
    public GameObject settingsMenuHolder;
    public GameObject pauseMenu;
    public Slider masterSlider;

    private bool fromPauseMenu;

    void Start()
    {
        settingsMenuHolder.SetActive(false);
    }

    public void ActivateSettingsMenu(bool parameterFromPauseMenu)
    {
        if (parameterFromPauseMenu)
        {
            fromPauseMenu = true;
            pauseMenu.SetActive(false);
        }

        settingsMenuHolder.SetActive(true);
        masterSlider.Select();
    }

    public void DeactivateSettingsMenu()
    {
        if (fromPauseMenu)
        {
            pauseMenu.SetActive(true);
            fromPauseMenu = false;
        }
        settingsMenuHolder.SetActive(false);
    }
}