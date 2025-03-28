using UnityEngine;
using static AugustBase.All;

public class MainMenuManager : MonoBehaviour {
    
    public static MainMenuManager instance;
    private MusicStageChanger musicStageChanger;
    
    [Header("Music Stage Changer")]
    public GameObject musicStageChangerObject;

    [Header("UI Elements")] 
    public GameObject mainMenuHolder;

    [Header("InkJSON file")] 
    public TextAsset InkJSON;

    private Animator animator;
    
    [Header("Booleans")]
    public bool introCutsceneIsPlaying;

    void Awake()
    {
        instance = this;
        musicStageChanger = musicStageChangerObject.GetComponent<MusicStageChanger>();
    }

    void Start()
    {
        PlayerController.instance.inMenu = true;
        mainMenuHolder.SetActive(true);
        animator = GetComponent<Animator>();
        musicStageChanger.TitleScreenMusic();
    }

    public void StartIntroCutscene()
    {
        
        animator.Play("startIntroCutscene");
        introCutsceneIsPlaying = true;
        musicStageChanger.IntroCutsceneMusic();
    }

    public void EndIntroCutscene()
    {
        animator.Play("endIntroCutscene");
        introCutsceneIsPlaying = false;
        musicStageChanger.MainStageMusic();
        Debug.Log("changed music to mainstagemusic");
        PlayerController.instance.inMenu = false;
    }

    public void DisableMainMenu()
    {
        mainMenuHolder.SetActive(false);
        animator.Play("default");
        PlayerController.instance.inMenu = false;
    }

    public void StartIntroDialogue()
    {
        DialogueManager.instance.EnterDialogueMode(InkJSON);
    }

    public void QuitGame()
    {
        StopProgram();
    }
}