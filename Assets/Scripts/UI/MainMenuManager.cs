using UnityEngine;
using static AugustBase.All;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    public static MainMenuManager instance;
    private MusicStageChanger musicStageChanger;

    [Header("First time playing")] public bool firstTime = true;

    [Header("Music Stage Changer")] public GameObject musicStageChangerObject;

    [Header("UI Elements")] public GameObject mainMenuHolder;
    public GameObject credits;
    public GameObject blackOverlay;
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public GameObject princess;

    [Header("InkJSON file")] public TextAsset InkJSON;

    private Animator animator;

    [Header("Booleans")] public bool introCutsceneIsPlaying;

    void Awake() {
        instance = this;
        musicStageChanger = musicStageChangerObject.GetComponent<MusicStageChanger>();
    }

    void Start() {
        mainMenuHolder.SetActive(true);
        credits.SetActive(false);
        blackOverlay.SetActive(false);
        animator = GetComponent<Animator>();
        musicStageChanger.TitleScreenMusic();
        PlayerController.instance.inMenu = true;
        Debug.Log("STARTING MAIN MENU" + PlayerController.instance.inMenu);
        FMODController.instance.pauseAudio = false;
    }

    public void StartGame() {
        if (firstTime) {
            StartIntroCutscene();
        }
        else if (!firstTime) {
            EndIntroCutscene();
        }
    }

    public void StartIntroCutscene() {
        firstTime = false;
        animator.Play("startIntroCutscene");
        introCutsceneIsPlaying = true;
        musicStageChanger.IntroCutsceneMusic();
        credits.SetActive(false);
        FMODController.instance.pauseAudio = false;
    }

    public void EndIntroCutscene() {
        credits.SetActive(false);
        animator.Play("endIntroCutscene");
        introCutsceneIsPlaying = false;
        musicStageChanger.MainStageMusic();
        Debug.Log("changed music to mainstagemusic");
        PlayerController.instance.inMenu = false;
    }

    public void EnableMainMenu() {
        if (!mainMenuHolder.activeInHierarchy) {
            animator.Play("default");
            mainMenuHolder.SetActive(true);
        }
        EnableButtons();
        PlayerController.instance.inMenu = true;
        FMODController.instance.currentMusicStage = FMODController.MusicStage.TitleScreen;
        FMODController.instance.pauseAudio = true;
        startButton.Select();
        Debug.Log("opened main menu");
    }

    public void DisableMainMenu() {
        mainMenuHolder.SetActive(false);
        animator.Play("default");
        PlayerController.instance.inMenu = false;
        FMODController.instance.pauseAudio = false;
    }

    public void DisableButtons() {
        startButton.enabled = false;
        settingsButton.enabled = false;
        quitButton.enabled = false;
    }

    public void EnableButtons() {
        startButton.enabled = true;
        settingsButton.enabled = true;
        quitButton.enabled = true;
    }

    public void StartIntroDialogue() {
        DialogueManager.instance.EnterDialogueMode(InkJSON);
    }

    public void QuitGame() {
        StopProgram();
    }

    public void StartCredits() {
        FMODController.instance.currentMusicStage = FMODController.MusicStage.Credits;
        blackOverlay.SetActive(true);
        PlayerController.instance.inMenu = true;
        animator.Play("startCreditsNew");
    }

    public void StartCreditsAdditionalDings() {
        mainMenuHolder.SetActive(true);
        credits.SetActive(true);
    }

    public void CreditsEnd() {
        blackOverlay.SetActive(false);
        EnableButtons();
        startButton.Select();
        FMODController.instance.currentMusicStage = FMODController.MusicStage.TitleScreen;
        LevelResetController.instance.ResetAllLevels();
        princess.SetActive(false);
    }
}