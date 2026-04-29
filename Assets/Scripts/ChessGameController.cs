using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PieceCreator))]
public class ChessGameController : MonoBehaviour
{
    private enum GameState { Init, Play, Finished }

    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;
    [SerializeField] private ChessUIManager uiManager;
    [Header("AI Settings")]
    [SerializeField] private StockfishAPIManager aiManager;

    private PieceCreator pieceCreator;
    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer activePlayer;
    private GameState state;
    private Takeback takebackSystem;

    private void Awake()
    {
        pieceCreator = GetComponent<PieceCreator>();

        takebackSystem = GetComponent<Takeback>();
        if (takebackSystem == null)
        {
            takebackSystem = gameObject.AddComponent<Takeback>();
            Debug.Log("Takeback component added automatically");
        }

        CreatePlayers();
    }

    private void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColour.White, board);
        blackPlayer = new ChessPlayer(TeamColour.Black, board);

        if (takebackSystem != null)
        {
            takebackSystem.SetPlayers(whitePlayer, blackPlayer);
            Debug.Log("Players assigned to Takeback system");
        }
    }

    public TeamColour GetActivePlayerTeam()
    {
        return activePlayer.team;
    }

    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        uiManager.HideUI();
        SetGameState(GameState.Init);
        board.SetDependencies(this);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
        SetGameState(GameState.Play);
    }

    private void SetGameState(GameState newState)
    {
        state = newState;
    }

    public bool IsGameInProgress()
    {
        return state == GameState.Play;
    }

    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColour team = layout.GetSquareTeamColourAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);

            Type type = Type.GetType(typeName);
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }

    public void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColour team, Type type)
    {
        Piece newPiece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        newPiece.SetData(squareCoords, team, board);

        Material teamMaterial = pieceCreator.GetTeamMaterial(team);
        newPiece.SetMaterial(teamMaterial);

        board.SetPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = team == TeamColour.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }

    private void GenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.GenerateAllPossibleMoves();
    }

    public bool IsTeamTurnActive(TeamColour team)
    {
        return activePlayer.team == team;
    }

    public void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));

        if (CheckIfGameIsFinished())
        {
            EndGame();
            return;
        }

        ChangeActiveTeam();

        if (aiManager != null && aiManager.IsAITurn(activePlayer.team))
        {
            FENGenerator fenGen = aiManager.GetComponent<FENGenerator>();
            if (fenGen != null)
            {
                string currentFEN = fenGen.GenerateFEN(activePlayer.team);
                aiManager.RequestAIMove(currentFEN);
            }
            else
            {
                Debug.LogError("FENGenerator component missing on AI Manager!");
            }
        }
    }

    private bool CheckIfGameIsFinished()
    {
        ChessPlayer opponent = GetOpponentToPlayer(activePlayer);
        Piece[] kingAttacks = activePlayer.GetPiecesAttackingOppositePieceOfType<King>();

        if (kingAttacks.Length > 0)
        {
            Piece attackedKing = opponent.GetPiecesOfType<King>().FirstOrDefault();
            if (attackedKing != null)
            {
                opponent.RemoveMovesEnablingAttackOnPiece<King>(activePlayer, attackedKing);
                if (attackedKing.availableMoves.Count == 0 && !opponent.CanHidePieceFromAttack<King>(activePlayer))
                    return true;
            }
        }
        else if (CheckForStalemate(opponent))
        {
            return true;
        }

        return false;
    }

    private bool CheckForStalemate(ChessPlayer player)
    {
        Piece king = player.GetPiecesOfType<King>().FirstOrDefault();
        if (king != null)
            player.RemoveMovesEnablingAttackOnPiece<King>(GetOpponentToPlayer(player), king);

        foreach (Piece piece in player.activePieces)
        {
            if (piece.availableMoves.Count > 0)
                return false;
        }
        return true;
    }

    private void EndGame()
    {
        ChessPlayer opponent = GetOpponentToPlayer(activePlayer);
        Piece[] kingAttacks = activePlayer.GetPiecesAttackingOppositePieceOfType<King>();

        if (kingAttacks.Length > 0)
        {
            uiManager.OnGameFinished(activePlayer.team.ToString());
        }
        else
        {
            uiManager.OnStalemate();
        }

        SetGameState(GameState.Finished);
    }

    private void ChangeActiveTeam()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    public void RemoveMovesEnablingAttackOnPieceOfTypes<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveMovesEnablingAttackOnPiece<T>(GetOpponentToPlayer(activePlayer), piece);
    }

    public void OnPieceRemoved(Piece piece)
    {
        ChessPlayer owner = piece.team == TeamColour.White ? whitePlayer : blackPlayer;
        owner.RemovePiece(piece);
        piece.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestarted();
        whitePlayer.OnGameRestarted();
        blackPlayer.OnGameRestarted();

        if (takebackSystem != null)
        {
            takebackSystem.ClearHistory();
        }

        StartNewGame();
    }

    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(p => Destroy(p.gameObject));
        blackPlayer.activePieces.ForEach(p => Destroy(p.gameObject));
    }

    public void RequestTakeback()
    {
        if (takebackSystem == null)
        {
            Debug.LogError("Takeback system is NULL!");
            return;
        }

        if (takebackSystem.CanTakeback())
        {
            takebackSystem.UndoLastMove();

            if (takebackSystem.CanTakeback())
            {
                takebackSystem.UndoLastMove();
            }

            GenerateAllPossiblePlayerMoves(whitePlayer);
            GenerateAllPossiblePlayerMoves(blackPlayer);

            Debug.Log("Takeback completed");
        }
        else
        {
            Debug.LogWarning("Cannot takeback - no moves in history");
        }
    }

    public bool CanTakeback()
    {
        return takebackSystem != null && takebackSystem.CanTakeback();
    }

    public ChessPlayer GetWhitePlayer()
    {
        return whitePlayer;
    }

    public ChessPlayer GetBlackPlayer()
    {
        return blackPlayer;
    }

    public void RecordMove(MoveRecord record)
    {
        if (takebackSystem == null)
        {
            Debug.LogError("Takeback system is NULL!");
            return;
        }

        takebackSystem.RecordMove(record);
        Debug.Log("Move recorded: " + record.fromSquare + " to " + record.toSquare + " | History: " + takebackSystem.GetMoveCount() + " moves");
    }
}
