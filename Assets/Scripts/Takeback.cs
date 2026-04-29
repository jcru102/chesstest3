using System.Collections.Generic;
using UnityEngine;

public class Takeback : MonoBehaviour
{
    private Stack<MoveRecord> moveHistory;
    private ChessGameController gameController;
    private Board board;
    private PieceCreator pieceCreator;

    [SerializeField] private ChessPlayer whitePlayer;
    [SerializeField] private ChessPlayer blackPlayer;

    private void Awake()
    {
        moveHistory = new Stack<MoveRecord>();
        gameController = GetComponent<ChessGameController>();
        board = FindFirstObjectByType<Board>();
        pieceCreator = GetComponent<PieceCreator>();
    }

    public void SetPlayers(ChessPlayer white, ChessPlayer black)
    {
        whitePlayer = white;
        blackPlayer = black;
    }

    public void RecordMove(MoveRecord record)
    {
        moveHistory.Push(record);
        Debug.Log($"Move recorded. History depth: {moveHistory.Count}");
    }

    public bool CanTakeback()
    {
        return moveHistory.Count > 0;
    }

    public void UndoLastMove()
    {
        if (!CanTakeback())
        {
            Debug.LogWarning("No moves to undo!");
            return;
        }

        MoveRecord lastMove = moveHistory.Pop();
        Debug.Log($"Undoing move from {lastMove.fromSquare} to {lastMove.toSquare}");

        Pawn.lastPawnThatMovedTwoSquares = lastMove.previousEnPassantPawn;

        if (lastMove.wasCastling)
        {
            UndoCastling(lastMove);
            return;
        }

        if (lastMove.wasPromotion)
        {
            UndoPromotion(lastMove);
            return;
        }

        UndoStandardMove(lastMove);
    }

    private void UndoStandardMove(MoveRecord record)
    {
        Piece piece = record.movedPiece;

        board.UpdateBoardOnPieceMove(record.fromSquare, record.toSquare, piece, null);
        piece.occupiedSquare = record.fromSquare;
        piece.hasMoved = !record.wasFirstMove;

        Vector3 originalPosition = board.CalculatePositionFromCoords(record.fromSquare);
        piece.transform.position = originalPosition;

        if (record.capturedPiece != null)
        {
            Debug.Log($"Attempting to restore: {record.capturedPiece.name}, Active: {record.capturedPiece.gameObject.activeSelf}");
            RestoreCapturedPiece(record);
        }
    }

    private void RestoreCapturedPiece(MoveRecord record)
    {
        if (record.capturedPiece == null)
        {
            Debug.LogWarning("No captured piece to restore");
            return;
        }

        Piece captured = record.capturedPiece;

        captured.gameObject.SetActive(true);
        captured.occupiedSquare = record.capturedPieceSquare;

        board.SetPieceOnBoard(record.capturedPieceSquare, captured);

        ChessPlayer owner = captured.team == TeamColour.White ? whitePlayer : blackPlayer;
        if (owner == null)
        {
            Debug.LogError("Owner player is null!");
            return;
        }

        if (!owner.activePieces.Contains(captured))
        {
            owner.AddPiece(captured);
        }

        Vector3 capturedPosition = board.CalculatePositionFromCoords(record.capturedPieceSquare);
        captured.transform.position = capturedPosition;

        Debug.Log($"Restored {captured.GetType().Name} at {record.capturedPieceSquare}");
    }

    private void UndoCastling(MoveRecord record)
    {
        Piece king = record.movedPiece;
        board.UpdateBoardOnPieceMove(record.fromSquare, record.toSquare, king, null);
        king.occupiedSquare = record.fromSquare;
        king.hasMoved = false;
        king.transform.position = board.CalculatePositionFromCoords(record.fromSquare);

        Piece rook = record.castledRook;
        board.UpdateBoardOnPieceMove(record.rookFromSquare, record.rookToSquare, rook, null);
        rook.occupiedSquare = record.rookFromSquare;
        rook.hasMoved = false;
        rook.transform.position = board.CalculatePositionFromCoords(record.rookFromSquare);

        Debug.Log("Castling undone");
    }

    private void UndoPromotion(MoveRecord record)
    {
        Piece promotedPiece = board.GetPieceOnSquare(record.toSquare);
        ChessPlayer owner = promotedPiece.team == TeamColour.White ? whitePlayer : blackPlayer;
        owner.RemovePiece(promotedPiece);
        Destroy(promotedPiece.gameObject);

        Pawn newPawn = pieceCreator.CreatePiece(record.originalPieceType).GetComponent<Pawn>();
        newPawn.SetData(record.fromSquare, record.teamThatMoved, board);
        Material teamMaterial = pieceCreator.GetTeamMaterial(record.teamThatMoved);
        newPawn.SetMaterial(teamMaterial);
        newPawn.hasMoved = !record.wasFirstMove;

        board.SetPieceOnBoard(record.fromSquare, newPawn);
        owner.AddPiece(newPawn);

        if (record.capturedPiece != null)
        {
            RestoreCapturedPiece(record);
        }

        Debug.Log("Promotion undone");
    }

    public int GetMoveCount()
    {
        return moveHistory.Count;
    }

    public void ClearHistory()
    {
        moveHistory.Clear();
    }
}
