using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChessUIManager : MonoBehaviour
{
    [SerializeField] private GameObject UIParent;
    [SerializeField] private Text resultText;

    public void HideUI()
    {
        UIParent.SetActive(false);
    }

    public void OnGameFinished(string winner)
    {
        UIParent.SetActive(true);
        resultText.text = string.Format("{0} won", winner);
    }

    public void OnStalemate()
    {
        UIParent.SetActive(true);
        resultText.text = "Stalemate";
    }

    [SerializeField] private Button takebackButton;

    private void Start()
    {
        takebackButton.onClick.AddListener(() => {
            FindFirstObjectByType<ChessGameController>().RequestTakeback();
        });
    }




}
