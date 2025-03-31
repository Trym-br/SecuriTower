using UnityEngine;

public class NPC_DoorBehaviour : MonoBehaviour, IInteractable {
    
    public static NPC_DoorBehaviour instance;
    
    [Header("Ink JSON")]
    public TextAsset InkJSON;

    [Header("Bools")] 
    public bool doorIsLocked = true;

    private Animator animator;
    

    void Awake()
    {
        instance = this;
        animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (!DialogueManager.instance.dialogueIsPlaying && doorIsLocked)
        {
            DialogueManager.instance.EnterDialogueMode(InkJSON);
        }
        else if (!doorIsLocked) {
            animator.Play("open");
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            FMODController.PlaySoundFrom(FMODController.Sound.SFX_DoorOpen, gameObject);
        }
    }
}
