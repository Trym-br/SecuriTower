using UnityEngine;

public class NPC_StaffBehavior : MonoBehaviour, IInteractable {
    
    public static NPC_StaffBehavior instance;
    
    [Header("InkJSON")] 
    public TextAsset InkJSON;

    void Awake()
    {
        instance = this;
    }

    public void Interact()
    {
        if (!DialogueManager.instance.dialogueIsPlaying)
        {
            DialogueManager.instance.EnterDialogueMode(InkJSON);
        }
    }

    public void CollectStaff()
    {
        NPC_DoorBehaviour.instance.doorIsLocked = false;
        FMODController.PlaySoundFrom(FMODController.Sound.SFX_StaffPickup, gameObject);
        gameObject.SetActive(false);
    }
}