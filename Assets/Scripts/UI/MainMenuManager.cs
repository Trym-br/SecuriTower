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
        mainMenuHolder.SetActive(true);
        animator = GetComponent<Animator>();
        musicStageChanger.TitleScreenMusic();
        Debug.Log("changed music to title screen music");
    }

    public void StartIntroCutscene()
    {
        animator.Play("startIntroCutscene");
        introCutsceneIsPlaying = true;
        musicStageChanger.IntroCutsceneMusic();
        Debug.Log("changed music to introcutscene music");
    }

    public void EndIntroCutscene()
    {
        animator.Play("endIntroCutscene");
        introCutsceneIsPlaying = false;
        musicStageChanger.MainStageMusic();
        Debug.Log("changed music to mainstagemusic");
    }

    public void DisableMainMenu()
    {
        mainMenuHolder.SetActive(false);
        animator.Play("default");
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