using UnityEngine;

public class NPC_PrincessBehaviour : MonoBehaviour
{
    public static NPC_PrincessBehaviour instance;
    private Animator animator;

    [Header("Ink JSON")] 
    public TextAsset InkJSON;

    void Awake()
    {
        instance = this;
        animator = GetComponent<Animator>();
    }

    public void YeetPrincess()
    {
        animator.Play("yeetPrincess");
    }

    public void FinalDialogue()
    {
        DialogueManager.instance.EnterDialogueMode(InkJSON);
    }
}
