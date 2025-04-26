using UnityEngine;

public class UI_Dings : MonoBehaviour {
    private InputActions input;

    private MainMenuManager mainMenuManager;
    private PauseMenuManager pauseMenuManager;
    private SettingsMenuManager settingsMenuManager;

    [Header("Menus")] 
    public GameObject mainMenu;
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    
    void Awake() {
        input = GetComponent<InputActions>();
        mainMenuManager = mainMenu.GetComponent<MainMenuManager>();
        pauseMenuManager = pauseMenu.GetComponent<PauseMenuManager>();
        settingsMenuManager = settingsMenu.GetComponent<SettingsMenuManager>();
    }

    void Start() {
        Cursor.visible = false;
    }

    void Update() {
        if (input.cancelBegin) {
            if (DialogueManager.instance.dialogueIsPlaying) {
                // do nothing   
                pauseMenuManager.ActivatePauseMenu(false);
            }
            else if (mainMenuManager.introCutsceneIsPlaying) {
                mainMenuManager.EndIntroCutscene();
            }
            else if (settingsMenuManager.settingsMenuHolder.activeInHierarchy) {
                settingsMenuManager.DeactivateSettingsMenu();
            }
            else if (pauseMenuManager.pauseMenuHolder.activeInHierarchy) {
                pauseMenuManager.DeactivatePauseMenu();
            }
            else if (!mainMenuManager.mainMenuHolder.activeInHierarchy) {
                pauseMenuManager.ActivatePauseMenu(false);
            }
        }
    }
}