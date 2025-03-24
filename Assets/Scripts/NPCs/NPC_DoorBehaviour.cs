using UnityEngine;

public class NPC_DoorBehaviour : MonoBehaviour, IInteractable {
    
    public static NPC_DoorBehaviour instance;
    
    [Header("Ink JSON")]
    public TextAsset InkJSON;

    [Header("Bools")] 
    public bool doorIsLocked = true;
    
    [Header("Sprite when opened")]
    public Sprite openSprite;
    

    void Awake()
    {
        instance = this;
    }

    public void Interact()
    {
        if (!DialogueManager.instance.dialogueIsPlaying && doorIsLocked)
        {
            DialogueManager.instance.EnterDialogueMode(InkJSON);
        }
        else if (!doorIsLocked)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = openSprite;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
