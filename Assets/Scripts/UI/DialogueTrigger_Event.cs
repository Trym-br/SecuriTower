using UnityEngine;

public class DialogueTrigger_Event : MonoBehaviour {
    
    public bool firstTimeEntered = true;

    [Header("Ink JSON")] 
    public TextAsset inkJSON;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (firstTimeEntered)
        {
            DialogueManager.instance.EnterDialogueMode(inkJSON);
            firstTimeEntered = false;
        }
    }
}
