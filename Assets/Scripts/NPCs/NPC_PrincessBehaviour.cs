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
        FMODController.instance.currentMusicStage = FMODController.MusicStage.WaitingSpace;
        FMODController.instance.currentLevel = FMODController.Level.IntroLevel;
        animator.Play("yeetPrincess");
        
    }

    public void ShatterGlassSFX() {
        FMODController.PlaySound(FMODController.Sound.SFX_GlassShatter);
    }

    public void FinalDialogue() {
        DialogueManager.instance.EnterDialogueMode(InkJSON);
    }
}