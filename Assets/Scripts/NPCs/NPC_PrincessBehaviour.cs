using UnityEngine;

public class NPC_PrincessBehaviour : MonoBehaviour {
    public static NPC_PrincessBehaviour instance;
    public Animator animator;
    

    [Header("Ink JSON")] public TextAsset InkJSON;

    void Awake() {
        instance = this;
        windowBoxCollider.enabled = false;
        windowOutline.enabled = false;
        windowInteractableDialogue.enabled = false;
    }

    void Start() {
        animator.Play("idle");
        
    }

    public void YeetPrincess() {
        FMODController.instance.currentMusicStage = FMODController.MusicStage.WaitingSpace;
        FMODController.instance.currentLevel = FMODController.Level.IntroLevel;
        animator.Play("yeetPrincess");
        PlayerController.instance.inMenu = true;

    }

    public BoxCollider2D windowBoxCollider;
    public InteractableOutline windowOutline;
    public InteractDialogueObject windowInteractableDialogue;

    // NOTE: This function name is misleading. This is the function that gets called when the window is shattered.
    // It also handles some other shit. Read the thing.
    
    public void ShatterGlassSFX() {
        FMODController.PlaySound(FMODController.Sound.SFX_GlassShatter);
        windowBoxCollider.enabled = true;
        windowOutline.enabled = true;
        windowInteractableDialogue.enabled = true;

    }

    public void FinalDialogue() {
        DialogueManager.instance.EnterDialogueMode(InkJSON);
    }
}