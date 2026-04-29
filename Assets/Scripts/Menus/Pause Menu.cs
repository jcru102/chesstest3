using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using NUnit.Framework.Constraints;

public class PauseMenu : MonoBehaviour
{
    public static bool Paused = false;
    public GameObject PauseMenuCanvas;

    void Start()
    {
        Time.timeScale = 1f;
        // Make sure cursor starts locked for first-person gameplay
       // Cursor.lockState = CursorLockMode.Locked;
       // Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Paused)
            {
                Play();
            }
            else
            {
                Stop();
            }
        }
    }

    void Stop()
    {
        PauseMenuCanvas.SetActive(true);
        Time.timeScale = 0f;
        Paused = true;

        // Unlock cursor and make it visible for menu interaction
       // Cursor.lockState = CursorLockMode.None;
       // Cursor.visible = true;
    }

    public void Play()
    {
        PauseMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        Paused = false;

        // Lock cursor back for first-person gameplay
      //  Cursor.lockState = CursorLockMode.Locked;
      //  Cursor.visible = false;
    }

    public void MainMenuButton()
    {
        // Reset time scale before changing scenes
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}
