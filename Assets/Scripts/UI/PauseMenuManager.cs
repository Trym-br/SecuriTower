using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour {
    public static PauseMenuManager instance;

    [Header("UI Elements")] public GameObject pauseMenuHolder;
    public Button continueButton;
    public Button settingsButton;

    void Start() {
        pauseMenuHolder.SetActive(false);

        instance = this;
    }

    public void ActivatePauseMenu(bool fromSettings) {
        if (fromSettings) {
            settingsButton.Select();
        }
        else {
            continueButton.Select();
        }
        pauseMenuHolder.SetActive(true);
        PlayerController.instance.inMenu = true;
        Cursor.visible = true;
    }

    public void DeactivatePauseMenu() {
        PlayerController.instance.inMenu = false;
        pauseMenuHolder.SetActive(false);
        Cursor.visible = false;
    }
}