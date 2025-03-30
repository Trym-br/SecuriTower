using UnityEngine;

public class NPC_MaidBehaviour : MonoBehaviour, IInteractable {
    private Animator animator;
    private Transform currentPoint;
    public static NPC_MaidBehaviour instance;
    public int timesInteractedWith;

    [Header("Movement")] 
    public GameObject point_a;
    public GameObject point_b;
    public float walkSpeed;
    private Vector3 point;

    [Header("InkJSONs")]
    public TextAsset[] InkJSONs;

    void Awake() {
        animator = GetComponent<Animator>();
        instance = this;
    }

    void Start() {
        animator.Play("maid_walk_left");
        currentPoint = point_a.transform;
    }

    void Update() {
        if (DialogueManager.instance.dialogueIsPlaying) return;
        if (currentPoint == point_a.transform) {
            transform.position =
                Vector3.MoveTowards(transform.position, point_a.transform.position, walkSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, point_a.transform.position) < 0.1f) {
                currentPoint = point_b.transform;
                animator.Play("maid_walk_right");
            }
        }
        else if (currentPoint == point_b.transform) {
            transform.position =
                Vector3.MoveTowards(transform.position, point_b.transform.position, walkSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, point_b.transform.position) < 0.1f) {
                currentPoint = point_a.transform;
                animator.Play("maid_walk_left");
            }
        }
    }

    public void EndedMaidDialogue() {
        currentPoint = point_a.transform;
        animator.Play("maid_walk_left");
    }

    public void Interact() {
        if (!DialogueManager.instance.dialogueIsPlaying) {
            animator.Play("maid_idle");
            if (timesInteractedWith < InkJSONs.Length) {
                DialogueManager.instance.EnterDialogueMode(InkJSONs[timesInteractedWith]);
                timesInteractedWith++;
            }
            else if (timesInteractedWith == InkJSONs.Length) {
                DialogueManager.instance.EnterDialogueMode(InkJSONs[InkJSONs.Length - 1]);
            }
            else {
                Debug.LogError("Error related to Ink JSON files attached to the NPC.");
            }
        }
    }
}