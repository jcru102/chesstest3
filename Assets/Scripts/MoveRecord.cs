using UnityEngine;

[System.Serializable]
public class MoveRecord
{
    public Vector2Int fromSquare;
    public Vector2Int toSquare;
    public Piece movedPiece;
    public Piece capturedPiece;
    public Vector2Int capturedPieceSquare; // For en passant
    public bool wasFirstMove; // To restore hasMoved state
    public TeamColour teamThatMoved;

    // Special move flags
    public bool wasCastling;
    public Vector2Int rookFromSquare;
    public Vector2Int rookToSquare;
    public Piece castledRook;

    // En passant tracking
    public Pawn previousEnPassantPawn;

    // Promotion tracking
    public bool wasPromotion;
    public System.Type originalPieceType;

    public MoveRecord(Vector2Int from, Vector2Int to, Piece piece, Piece captured,
                      Vector2Int capturedSquare, bool firstMove, TeamColour team)
    {
        fromSquare = from;
        toSquare = to;
        movedPiece = piece;
        capturedPiece = captured;
        capturedPieceSquare = capturedSquare;
        wasFirstMove = firstMove;
        teamThatMoved = team;
        wasCastling = false;
        wasPromotion = false;
        previousEnPassantPawn = Pawn.lastPawnThatMovedTwoSquares;
    }
}
