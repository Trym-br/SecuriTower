using UnityEngine;

public class MainMenuManager : MonoBehaviour {

    public static MainMenuManager instance;
    
    [Header("UI Elements")] 
    public GameObject mainMenuHolder;

    [Header("InkJSON file")] 
    public TextAsset InkJSON;

    private Animator animator;

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
    }

    public void EndIntroCutscene()
    {
        animator.Play("endIntroCutscene");
    }

    public void DisableMainMenu()
    {
        mainMenuHolder.SetActive(false);
    }

    public void StartIntroDialogue()
    {
        DialogueManager.instance.EnterDialogueMode(InkJSON);
    }
}
