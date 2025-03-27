using UnityEngine;

public class PauseMenuManager : MonoBehaviour {
    [Header("UI Elements")] 
    public GameObject pauseMenuHolder;

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
        }
    }

    public void ActivatePauseMenu()
    {
        pauseMenuHolder.SetActive(true);
    }

    public void DeactivatePauseMenu()
    {
        pauseMenuHolder.SetActive(false);
    }
}