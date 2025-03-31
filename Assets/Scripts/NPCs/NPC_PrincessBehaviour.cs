using UnityEngine;

public class NPC_PrincessBehaviour : MonoBehaviour {
    public static NPC_PrincessBehaviour instance;
    public Animator animator;

    [Header("Ink JSON")] public TextAsset InkJSON;

    void Awake() {
        instance = this;
    }

    void Start() {
        animator.Play("idle");
    }

    public void YeetPrincess() {
        animator.Play("yeetPrincess");
        Debug.Log("Yeeting princess");
    }

    public void ShatterGlassSFX() {
        FMODController.PlaySound(FMODController.Sound.SFX_GlassShatter);
    }

    public void FinalDialogue() {
        DialogueManager.instance.EnterDialogueMode(InkJSON);
    }
}