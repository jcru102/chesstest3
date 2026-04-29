using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject botDifficultyCanvas;

    [Header("Buttons")]
    [SerializeField] private Button playLocalButton;
    [SerializeField] private Button playWithBotButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button backButton;

    private void Start()
    {
        playLocalButton.onClick.AddListener(PlayLocal);
        playWithBotButton.onClick.AddListener(ShowBotDifficulty);
        quitButton.onClick.AddListener(Quit);
        easyButton.onClick.AddListener(PlayBotEasy);
        hardButton.onClick.AddListener(PlayBotHard);
        backButton.onClick.AddListener(GoBack);
    }

    public void PlayLocal()
    {
        PlayerPrefs.SetInt("PlayWithAI", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("OneBoardPVP");
    }

    public void ShowBotDifficulty()
    {
        mainMenuCanvas.SetActive(false);
        botDifficultyCanvas.SetActive(true);
    }

    public void PlayBotEasy()
    {
        PlayerPrefs.SetInt("PlayWithAI", 1);
        PlayerPrefs.SetInt("AIDifficulty", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("APIBoard");
    }

    public void PlayBotHard()
    {
        PlayerPrefs.SetInt("PlayWithAI", 1);
        PlayerPrefs.SetInt("AIDifficulty", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("APIBoard");
    }

    public void GoBack()
    {
        botDifficultyCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
