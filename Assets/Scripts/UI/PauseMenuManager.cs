using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour {
    [Header("UI Elements")] 
    public GameObject pauseMenuHolder;
    public Button continueButton;

    void Start()
    {
        pauseMenuHolder.SetActive(false);
    }

    public void ActivatePauseMenu()
    {
        pauseMenuHolder.SetActive(true);
        PlayerController.instance.inMenu = true;

    }

    public void DeactivatePauseMenu()
    {
        PlayerController.instance.inMenu = false;
        pauseMenuHolder.SetActive(false);
    }
}