using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour {
    public static PauseMenuManager instance;
    public bool pauseMenuIsActive;

    [Header("UI Elements")]
    public GameObject pauseMenuHolder;
    public Button continueButton;
    public Button settingsButton;
    public Button mainMenuButton;

    void Start() {
        pauseMenuHolder.SetActive(false);

        instance = this;
    }

    public void ActivatePauseMenu(bool fromSettings) {
        mainMenuButton.interactable = true;
        if (fromSettings) {
            settingsButton.Select();
        }
        else {
            continueButton.Select();
        }
        pauseMenuHolder.SetActive(true);
        pauseMenuIsActive = true;
        PlayerController.instance.inMenu = true;
        if (DialogueManager.instance.dialogueIsPlaying) {
            mainMenuButton.interactable = false;
        }
    }

    public void DeactivatePauseMenu() {
        PlayerController.instance.inMenu = false;
        pauseMenuHolder.SetActive(false);
        pauseMenuIsActive = false;
    }
}