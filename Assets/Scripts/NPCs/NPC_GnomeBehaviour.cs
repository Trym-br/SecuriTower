using UnityEngine;

public class NPC_GnomeBehaviour : MonoBehaviour, IInteractable
{
    public int timesInteractedWith;
    private Material outlineMaterial;
    
    [Header("InkJSONs")] 
    public TextAsset[] InkJSONs;
    

    public void Interact()
    {
        if (!DialogueManager.instance.dialogueIsPlaying)
        {
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
