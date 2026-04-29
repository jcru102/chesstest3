using System.Text;
using UnityEngine;

public class FENGenerator : MonoBehaviour
{
    private Board board;

    private void Awake()
    {
        board = FindFirstObjectByType<Board>();
    }

    public string GenerateFEN(TeamColour activePlayer)
    {
        StringBuilder fen = new StringBuilder();

        //piece placement
        for (int rank = 7; rank >= 0; rank--)
        {
            int emptySquares = 0;
            for (int file = 0; file < 8; file++)
            {
                Piece piece = board.GetPieceOnSquare(new Vector2Int(file, rank));
                if (piece == null) emptySquares++;
                else
                {
                    if (emptySquares > 0) { fen.Append(emptySquares); emptySquares = 0; }
                    fen.Append(GetPieceFENChar(piece));
                }
            }
            if (emptySquares > 0) fen.Append(emptySquares);
            if (rank > 0) fen.Append('/');
        }

        fen.Append(' ');
        fen.Append(activePlayer == TeamColour.White ? 'w' : 'b');
        fen.Append(' ');
        fen.Append(GetCastlingFEN());
        fen.Append(' ');
        fen.Append(GetEnPassantFEN());
        fen.Append(" 0 1");

        return fen.ToString();
    }

    private char GetPieceFENChar(Piece piece)
    {
        char c = piece switch
        {
            Pawn => 'p',
            Rook => 'r',
            Knight => 'n',
            Bishop => 'b',
            Queen => 'q',
            King => 'k',
            _ => ' '
        };
        return piece.team == TeamColour.White ? char.ToUpper(c) : c;
    }

    private string GetCastlingFEN()
    {
        StringBuilder castling = new StringBuilder();
        King whiteKing = FindKing(TeamColour.White);
        King blackKing = FindKing(TeamColour.Black);

        
        if (whiteKing != null && !whiteKing.hasMoved)
        {
            if (board.GetPieceOnSquare(new Vector2Int(7, 0)) is Rook wRk && !wRk.hasMoved) castling.Append('K');
            if (board.GetPieceOnSquare(new Vector2Int(0, 0)) is Rook wRq && !wRq.hasMoved) castling.Append('Q');
        }
        
        if (blackKing != null && !blackKing.hasMoved)
        {
            if (board.GetPieceOnSquare(new Vector2Int(7, 7)) is Rook bRk && !bRk.hasMoved) castling.Append('k');
            if (board.GetPieceOnSquare(new Vector2Int(0, 7)) is Rook bRq && !bRq.hasMoved) castling.Append('q');
        }

        return castling.Length > 0 ? castling.ToString() : "-";
    }

    private string GetEnPassantFEN()
    {
        if (Pawn.lastPawnThatMovedTwoSquares != null)
        {
            Vector2Int pos = Pawn.lastPawnThatMovedTwoSquares.occupiedSquare;
            Vector2Int enPassantSquare = Pawn.lastPawnThatMovedTwoSquares.team == TeamColour.White
                ? new Vector2Int(pos.x, pos.y - 1)
                : new Vector2Int(pos.x, pos.y + 1);
            return CoordinateToAlgebraic(enPassantSquare);
        }
        return "-";
    }

    private King FindKing(TeamColour team)
    {
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                if (board.GetPieceOnSquare(new Vector2Int(x, y)) is King k && k.team == team)
                    return k;
        return null;
    }

    private string CoordinateToAlgebraic(Vector2Int coords)
    {
        char file = (char)('a' + coords.x);
        char rank = (char)('1' + coords.y);
        return $"{file}{rank}";
    }
}