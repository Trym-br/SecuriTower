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

    public void TogglePauseMenu()
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

    }

    public void DeactivatePauseMenu()
    {
        pauseMenuHolder.SetActive(false);
        
    }
}