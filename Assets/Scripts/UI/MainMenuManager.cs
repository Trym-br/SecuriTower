using UnityEngine;
using static AugustBase.All;

public class MainMenuManager : MonoBehaviour {
    
    public static MainMenuManager instance;

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
    }

    void Start()
    {
        mainMenuHolder.SetActive(true);
        animator = GetComponent<Animator>();
    }

    public void StartIntroCutscene()
    {
        animator.Play("startIntroCutscene");
        introCutsceneIsPlaying = true;
    }

    public void EndIntroCutscene()
    {
        animator.Play("endIntroCutscene");
        introCutsceneIsPlaying = false;
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