using UnityEngine;
using UnityEngine.UI;

public class OpeningCutsceneManager : MonoBehaviour {
    
    public static OpeningCutsceneManager instance;
    
    [Header("Game Objects")] 
    public GameObject cutsceneImage;
    public GameObject darkenEffect;
    public Sprite[] images;

    [Header("Dialogue")] public TextAsset InkJSON;

    [Header("Stuff")] public float timeBetweenImages;
    public int amountOfFrames;
    public int playDialogueAtFrame;
    public float playDialogueAfterSeconds;


    [Header("Booleans")] public bool waitingForNextFrame;
    public bool waitingForDialogue;
    public bool cutsceneDone;

    private Animator animator;


    private float timeDings;
    private int frameIndex = 0;
    private float timeDingsDialogue;

    void Awake()
    {
        cutsceneImage.SetActive(false);
        animator = GetComponent<Animator>();
        instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCutScene();
        }
    }

    public void StartCutScene()
    {
        cutsceneImage.SetActive(true);
        cutsceneImage.GetComponent<Image>().sprite = images[frameIndex];
        waitingForNextFrame = true;
        timeDings = Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (waitingForNextFrame)
        {
            if (timeDings < timeBetweenImages)
            {
                timeDings += Time.deltaTime;
            }

            if (timeDings > timeBetweenImages)
            {
                frameIndex++;
                cutsceneImage.GetComponent<Image>().sprite = images[frameIndex];

                if (frameIndex + 1 == amountOfFrames)
                {
                    waitingForNextFrame = false;
                }

                if (playDialogueAtFrame == frameIndex + 1)
                {
                    waitingForDialogue = true;
                    timeDingsDialogue = Time.deltaTime;
                    playDialogueAfterSeconds = timeDingsDialogue + playDialogueAfterSeconds;
                    Debug.Log("Waiting for dialogue now");
                }
            }
        }

        if (waitingForDialogue)
        {
            if (timeDingsDialogue < playDialogueAfterSeconds)
            {
                timeDingsDialogue += Time.deltaTime;
            }
            else if (timeDingsDialogue >= playDialogueAfterSeconds)
            {
                waitingForDialogue = false;
                PlayDialogue();
            }
        }
        
    }

    public void EndCutscene()
    {
        animator.Play("endCutscene");
    }

    public void EndCutsceneForReal()
    {
        cutsceneImage.SetActive(false);
    }
    
    public void PlayDialogue()
    {
        Debug.Log("PlayDialogue() called");
        DialogueManager.instance.EnterDialogueMode(InkJSON);
    }
}