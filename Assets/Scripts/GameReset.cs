using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartGame : MonoBehaviour
{
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartCurrentScene();
        }
    }

    public void RestartCurrentScene()
    {

        string currentSceneName = SceneManager.GetActiveScene().name;


        SceneManager.LoadScene(currentSceneName);
    }
}