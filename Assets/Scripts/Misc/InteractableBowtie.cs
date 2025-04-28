using UnityEngine;

public class InteractableBowtie : MonoBehaviour, IInteractable {
    [Tooltip(
        "These dialogues will be played in order globally for all bowties. NOTE: MAKE SURE YOU EDIT THIS ON THE PREFAB!!")]
    public TextAsset[] inOrderDialogueInkJSONArray;

    static int dialogueIndex = 0;

    bool wasInteractedWith = false;

    void Update() {
        if (wasInteractedWith && !DialogueManager.instance.dialogueIsPlaying) {
            Destroy(this.gameObject);
            FMODController.PlaySoundFrom(FMODController.Sound.SFX_BowRemove, gameObject);
        }
    }

    public void Interact() {
        wasInteractedWith = true;

        if (DialogueManager.instance == null || inOrderDialogueInkJSONArray.Length <= dialogueIndex) {
            return;
        }

        if (inOrderDialogueInkJSONArray == null || inOrderDialogueInkJSONArray.Length == 0) {
            Debug.LogError($"Trying to play dialogue from '{this.name}' on interact, but there's no dialogue to play.");
            return;
        }

        if ((inOrderDialogueInkJSONArray[dialogueIndex]) != null) {
            DialogueManager.instance.EnterDialogueMode(inOrderDialogueInkJSONArray[dialogueIndex]);
        }
        
        dialogueIndex += 1;

    }
}