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
    
    public void TogglePauseMenu() // not used currently
    {
        if (!pauseMenuHolder.activeInHierarchy)
        {
            pauseMenuHolder.SetActive(true);
        }
        else if (pauseMenuHolder.activeInHierarchy)
        {
            pauseMenuHolder.SetActive(false);
            continueButton.Select();
        }
    }

    public void ActivatePauseMenu()
    {
        pauseMenuHolder.SetActive(true);
        continueButton.Select();
        PlayerController.instance.inMenu = true;

    }

    public void DeactivatePauseMenu()
    {
        PlayerController.instance.inMenu = false;
        pauseMenuHolder.SetActive(false);
    }
}