using System;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour {
    // This script will:
    // Store the Ink JSON file
    // Keep track of player distance
    // Trigger dialogue manager to play

    private InputActions input;
    public static DialogueManager instance;
    private Story _currentStory;

    // Story
    private Story currentStory;

    //Tags
    private const string speakerTag = "speaker";
    private const string leftPortraitTag = "leftPortrait";
    private const string rightPortraitTag = "rightPortrait";
    private const string layoutTag = "layout";
    private const string wordSpeedTag = "wordSpeed";
    private const string audioTag = "audio";
    private const string endCutsceneTag = "endCutscene";
    private const string yeetPrincessTag = "yeetPrincess";
    private const string startCreditsTag = "startCredits";

    //Typing effect
    private float showNextCharacterAt;
    private string line;

    [Header("Booleans")] public bool dialogueIsPlaying;
    public bool typing;
    private bool endCutsceneAfterDialogue;
    private bool yeetPrincessAfterDialogue;
    private bool startCreditsAfterDialogue;

    [Header("Word Speed")] public float wordSpeed;

    [Header("Dialogue UI")] public GameObject dialogueHolder;
    public GameObject continueSymbol;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerName;

    [Header("Animators")] public Animator leftPortraitAnimator;
    public Animator rightPortraitAnimator;
    public Animator layoutAnimator;
    public Animator panelAnimator;
    private Animator dialogueAnimator;

    private void Awake() {
        input = GetComponent<InputActions>();

        if (instance != null) {
            Debug.Log("Found more than one dialogue manager in the scene!");
        }

        instance = this;
    }

    private void Start() {
        dialogueAnimator = GetComponent<Animator>();
        dialogueHolder.SetActive(false);
    }

    #region Handle and Display Dialogue

    void Update() {
        if (!dialogueIsPlaying) {
            return;
        }

        if (typing) {
            Typing();
        }

        if (input.submitBegin && dialogueIsPlaying) {
            if (PauseMenuManager.instance.pauseMenuIsActive) {
                return;
            }
            if (typing) {
                SkipDialogue();
                Debug.Log("Skipped dialogue!");
            }
            else if (!typing) {
                ContinueStory();
            }
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON) {
        Debug.Log("entering dialogue mode");
        if (dialogueIsPlaying) return;

        currentStory = new Story(inkJSON.text);
        dialogueHolder.SetActive(true);
        continueSymbol.SetActive(false);
        dialogueIsPlaying = true;
        PlayerController.instance.inMenu = true;

        //functions
        currentStory.BindExternalFunction("collectStaff", (string hi) => {
            Debug.Log("staff collected");
            NPC_StaffBehavior.instance.CollectStaff();
        });
        currentStory.BindExternalFunction("yeetPrincess",
            (string hi) => { NPC_PrincessBehaviour.instance.YeetPrincess(); });

        dialogueAnimator.Play("start_initiator");
        //reset tags so they don't carry over from previous story
        speakerName.text = "???";
        wordSpeed = 0.03f;

        ContinueStory();
    }

    public void StartDialogueAnimation() {
        dialogueAnimator.Play("start");
    }

    public void ContinueStory() {
        if (currentStory.canContinue) {
            currentStory.Continue();
            HandleTags(currentStory.currentTags);
            showNextCharacterAt = Time.time + wordSpeed;
            line = currentStory.currentText;
            dialogueText.text = line;
            dialogueText.maxVisibleCharacters = 0;

            typing = true;
        }

        else if (!currentStory.canContinue) {
            dialogueAnimator.Play("end");
        }
    }

    private void Typing() {
        if (Time.time > showNextCharacterAt) {
            showNextCharacterAt = Time.time + wordSpeed;

            if (dialogueText.maxVisibleCharacters < line.Length) {
                typing = true;
                continueSymbol.SetActive(false);
                dialogueText.maxVisibleCharacters++;
            }
            else if (dialogueText.maxVisibleCharacters == line.Length) {
                typing = false;
                continueSymbol.SetActive(true);
            }
        }
    }

    public void SkipDialogue() {
        if (!typing) {
            ContinueStory();
        }

        typing = false;
        continueSymbol.SetActive(true);
        dialogueText.maxVisibleCharacters = line.Length;
    }

    public void EndDialogue() {
        dialogueHolder.SetActive(false);
        dialogueIsPlaying = false;
        PlayerController.instance.inMenu = false;
        
        if (Generic_NPC.instance != null) Generic_NPC.instance.EndedDialogue();
        if (endCutsceneAfterDialogue) {
            Debug.Log("Ending cutscene.");
            MainMenuManager.instance.EndIntroCutscene();
            endCutsceneAfterDialogue = false;
        }

        if (yeetPrincessAfterDialogue) {
            NPC_PrincessBehaviour.instance.YeetPrincess();
            yeetPrincessAfterDialogue = false;
        }

        if (startCreditsAfterDialogue) {
            MainMenuManager.instance.StartCredits();
            startCreditsAfterDialogue = false;
        }
    }

    #endregion

    #region Tags

    private void HandleTags(List<string> currentTags) {
        // loop gjennom alle tags og håndter dem
        foreach (string tag in currentTags) {
            string[] splitTag = tag.Split(';'); 
            if (splitTag.Length != 2) {
                Debug.LogError("Tag could not be parsed properly: " + tag);
            }

            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            switch (tagKey) {
                case speakerTag:
                    speakerName.text = tagValue;
                    //panelAnimator.Play($"{tagValue}_panel");
                    break;
                case leftPortraitTag:
                    leftPortraitAnimator.Play(tagValue);
                    break;
                case rightPortraitTag:
                    rightPortraitAnimator.Play(tagValue);
                    break;
                case layoutTag:
                    layoutAnimator.Play(tagValue);
                    break;
                case wordSpeedTag:
                    try {
                        wordSpeed = float.Parse(tagValue);
                    }
                    catch (FormatException) {
                        string newTagValue = tagValue.Replace('.', ',');
                        try {
                            wordSpeed = float.Parse(newTagValue);
                            Debug.Log(newTagValue);
                        }
                        catch (FormatException) {
                            float defaultValue = 0.03f;
                            Debug.LogError($"Could not parse '{tagValue}' as a float for word speed! " +
                                           $"Using a default value of {defaultValue} instead.");
                            wordSpeed = defaultValue;
                        }
                    }

                    break;

                case audioTag:
                    FMODController.PlayVoiceLineAudio(tagValue);
                    Debug.Log(tagValue);
                    break;

                case endCutsceneTag:
                    endCutsceneAfterDialogue = true;
                    break;

                case yeetPrincessTag:
                    yeetPrincessAfterDialogue = true;
                    break;

                case startCreditsTag:
                    startCreditsAfterDialogue = true;
                    break;

                default:
                    Debug.LogWarning("Tag came in but is not currently being handled: " + tag);
                    break;
            }
        }
    }

    #endregion
}