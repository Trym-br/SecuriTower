using UnityEngine;
using Random = UnityEngine.Random;

// Dogshit name :)
public class InteractDialogueObject : MonoBehaviour, IInteractable {
	[Tooltip("The object chooses a dialogue from this array at random.")]
	public TextAsset[] dialogueInkJSONArray;

	public void Interact() {
		if (DialogueManager.instance == null) return;

		if (dialogueInkJSONArray == null || dialogueInkJSONArray.Length == 0) {
			Debug.LogError($"Trying to play dialogue from '{this.name}' on interact, but there's no dialogue to play.");
			return;
		}

		DialogueManager.instance.EnterDialogueMode(dialogueInkJSONArray[Random.Range(0, dialogueInkJSONArray.Length)]);
	}
}
