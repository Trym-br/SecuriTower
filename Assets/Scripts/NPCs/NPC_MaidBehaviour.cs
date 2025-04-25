using Ink.Parsed;
using UnityEngine;

public class NPC_MaidBehaviour : MonoBehaviour, IInteractable {
    private Animator animator;
    private Transform currentPoint;
    private Material outlineMaterial;

    public static NPC_MaidBehaviour instance;
    public int timesInteractedWith;

    [Header("Movement")] 
    public GameObject point_a;
    public GameObject point_b;
    public float walkSpeed;
    private Vector3 point;

    [Header("InkJSONs")] public TextAsset[] InkJSONs;

    void Awake() {
        animator = GetComponent<Animator>();
        instance = this;
        outlineMaterial = GetComponent<Renderer>().material;
    }

    void Start() {
        animator.Play("walk_left");
        currentPoint = point_a.transform;
    }

    void Update() {
		Debug.LogError($"{this.name}: THIS SCRIPT SHOULD NOT BE ATTACHED TO ANYTHING!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1");
		return;

#if false
        if (DialogueManager.instance.dialogueIsPlaying) return;
        if (currentPoint == point_a.transform) {
            transform.position =
                Vector3.MoveTowards(transform.position, point_a.transform.position, walkSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, point_a.transform.position) < 0.1f) {
                currentPoint = point_b.transform;
                animator.Play("walk_right");
            }
        }
        else if (currentPoint == point_b.transform) {
            transform.position =
                Vector3.MoveTowards(transform.position, point_b.transform.position, walkSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, point_b.transform.position) < 0.1f) {
                currentPoint = point_a.transform;
                animator.Play("walk_left");
            }
        }
#endif
    }

    void OnTriggerStay2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        if (DialogueManager.instance.dialogueIsPlaying) {
            outlineMaterial.SetFloat("_Alpha", 0.0f);
        }
        else {
            outlineMaterial.SetFloat("_Alpha", 1.0f);
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;
        outlineMaterial.SetFloat("_Alpha", 0.0f);
    }

	/*
    public void EndedMaidDialogue() {
        currentPoint = point_a.transform;
        animator.Play("walk_left");
    }
	*/

    public void Interact() {
        if (!DialogueManager.instance.dialogueIsPlaying) {
            animator.Play("idle");
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
