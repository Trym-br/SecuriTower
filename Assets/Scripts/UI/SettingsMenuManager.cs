using UnityEngine;

public class SettingsMenuManager : MonoBehaviour {
    [Header("UI Elements")] 
    public GameObject settingsMenuHolder;
    public GameObject pauseMenu;

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