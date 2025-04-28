using UnityEngine;

public class NPC_DoorBehaviour : MonoBehaviour, IInteractable {
    public static NPC_DoorBehaviour instance;

    [Header("Ink JSON")]
    public TextAsset InkJSON;

    [Header("Bools")] 
    public bool doorIsLocked = true;

    private Animator animator;

	bool isOpen = false;

    void Awake() {
        instance = this;
        animator = GetComponent<Animator>();

		isOpen = false;
    }

	BoxCollider2D boxCollider;
	void FixedUpdate() {
		if (animator != null) {
			animator.SetBool("Open", isOpen);
		}

		if (boxCollider == null) {
			boxCollider = GetComponent<BoxCollider2D>();
		}

		boxCollider.enabled = !isOpen;
	}

    public void Interact()
    {
        if (!DialogueManager.instance.dialogueIsPlaying && doorIsLocked)
        {
            DialogueManager.instance.EnterDialogueMode(InkJSON);
        }
        else if (!doorIsLocked) {
            //animator.Play("open");
            //gameObject.GetComponent<BoxCollider2D>().enabled = false;
			isOpen = true;
            FMODController.PlaySoundFrom(FMODController.Sound.SFX_DoorOpen, gameObject);
        }
    }
}
