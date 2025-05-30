using UnityEngine;

public class Generic_NPC : MonoBehaviour, IInteractable {
    
    private Animator animator;
    private Transform currentPoint;
    
    public int timesInteractedWith;
    
    public static Generic_NPC instance;

    [Header("Which NPC is this?")] 
    public bool maid;
    public bool gnome;
    public bool golem;
    public bool dragon;
    public bool princess;

    [Header("Does this NPC walk around?")] 
    public bool walks;
    
    [Header("InkJSONs")] 
    public TextAsset[] InkJSONs;
    
    [Header("Movement")] 
    public GameObject point_a;
    public GameObject point_b;
    public float walkSpeed;
    private Vector3 point;

    void Awake() {
        TryGetComponent<Animator>(out animator);

        instance = this;
    }

    void Start() {
        if (walks) {
            if (animator != null) animator.Play("walk_left");
            currentPoint = point_a.transform;
        }
        else {
            if (animator != null) animator.Play("idle");
        }
    }

    void Update() {
        if (!walks) return;

        if (DialogueManager.instance.dialogueIsPlaying) return;

        if (currentPoint == point_a.transform) {
            transform.position =
                Vector3.MoveTowards(transform.position, point_a.transform.position, walkSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, point_a.transform.position) < 0.1f) {
                currentPoint = point_b.transform;
				currentWalkAnimation = "walk_right";
                if (animator != null) animator.Play(currentWalkAnimation);
            }
        } else if (currentPoint == point_b.transform) {
            transform.position =
                Vector3.MoveTowards(transform.position, point_b.transform.position, walkSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, point_b.transform.position) < 0.1f) {
                currentPoint = point_a.transform;
				currentWalkAnimation = "walk_left";
                if (animator != null) animator.Play(currentWalkAnimation);
            }
        }
        
    }

	string currentWalkAnimation = "walk_right";
    public void EndedDialogue() {
		if (walks && animator != null) {
			animator.Play(currentWalkAnimation);
		}

        if (gnome) {
            animator.Play("idle");
        }
    }

    public void Interact()
    {
        if (!DialogueManager.instance.dialogueIsPlaying)
        {
            if (animator != null && !gnome) animator.Play("idle");
            if (gnome) animator.Play("dialogue_idle");
            if (timesInteractedWith < InkJSONs.Length)
            {
                DialogueManager.instance.EnterDialogueMode(InkJSONs[timesInteractedWith]);
                timesInteractedWith++;
            }
            else if (timesInteractedWith == InkJSONs.Length)
            {
                DialogueManager.instance.EnterDialogueMode(InkJSONs[InkJSONs.Length - 1]);
            }
            else
            {
                Debug.LogError("Error related to Ink JSON files attached to the NPC.");
            }
        }
    }
}
