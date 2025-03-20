using UnityEngine;

public class Dialogue_Trigger_NPC : MonoBehaviour {
    private InputActions input;

    public int timesInteractedWith;
    public bool playerInRange;
    public GameObject visualCue;

    [Header("Ink JSONs")] 
    public TextAsset[] InkJSONs;

    void Start()
    {
        input = GetComponent<InputActions>();
        visualCue.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && input.interactBegin && !DialogueManager.instance.dialogueIsPlaying)
        {
            if (timesInteractedWith < InkJSONs.Length)
            {
                DialogueManager.instance.EnterDialogueMode(InkJSONs[timesInteractedWith]);
                timesInteractedWith++;
            }
            else if (timesInteractedWith == InkJSONs.Length)
            {
                DialogueManager.instance.EnterDialogueMode(InkJSONs[^1]);
            }
            else
            {
                Debug.LogError("Error related to Ink JSON files attached to the NPC.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        visualCue.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        visualCue.SetActive(false);
    }
}