using UnityEngine;
using UnityEngine.UI;

public class ResignButton : MonoBehaviour
{
    [SerializeField] private Button resignButton;
    [SerializeField] private ChessGameController gameController;
    [SerializeField] private ChessUIManager uiManager;

    private void Start()
    {
        if (resignButton != null)
            resignButton.onClick.AddListener(ResignGame);
    }

    private void ResignGame()
    {
        if (!gameController.IsGameInProgress())
            return;

        
        TeamColour resigningPlayer = gameController.GetActivePlayerTeam();
        TeamColour winner = resigningPlayer == TeamColour.White ? TeamColour.Black : TeamColour.White;

        
        uiManager.OnGameFinished(winner.ToString());
    }

    private void OnDestroy()
    {
        if (resignButton != null)
            resignButton.onClick.RemoveListener(ResignGame);
    }
}
