using UnityEngine;

public class Generic_NPC : MonoBehaviour, IInteractable {
    
    private Animator animator;
    private Material material;
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
        material = GetComponent<Renderer>().material;

        instance = this;
    }

    void Start() {
        if (walks) {
            if (animator != null) animator.Play("walk_left");
            currentPoint = point_a.transform;
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
                if (animator != null) animator.Play("walk_right");
            }
        }
        else if (currentPoint == point_b.transform) {
            transform.position =
                Vector3.MoveTowards(transform.position, point_b.transform.position, walkSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, point_b.transform.position) < 0.1f) {
                currentPoint = point_a.transform;
                if (animator != null) animator.Play("walk_left");
            }
        }
        
    }
    void OnTriggerStay2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        if (DialogueManager.instance.dialogueIsPlaying) {
            material.SetFloat("_Alpha", 0.0f);
        }
        else {
            material.SetFloat("_Alpha", 1.0f);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        material.SetFloat("_Alpha", 0.0f);
    }

    public void EndedDialogue() {
        if (walks) {
            if (animator != null) animator.Play("walk_left");
            currentPoint = point_a.transform;
        }
    }

    public void Interact()
    {
        if (!DialogueManager.instance.dialogueIsPlaying)
        {
            if (animator != null) animator.Play("idle");
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
