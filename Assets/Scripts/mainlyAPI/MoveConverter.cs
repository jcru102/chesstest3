using UnityEngine;

public class MoveConverter : MonoBehaviour
{
    public ChessMove ParseAlgebraicMove(string moveString)
    {
        if (string.IsNullOrEmpty(moveString) || moveString.Length < 4)
        {
            Debug.LogError($"Invalid move string: {moveString}");
            return null;
        }

        Vector2Int from = AlgebraicToCoordinate(moveString.Substring(0, 2));
        Vector2Int to = AlgebraicToCoordinate(moveString.Substring(2, 2));

        string promotion = "";
        if (moveString.Length > 4)
        {
            promotion = moveString.Substring(4, 1);
        }

        return new ChessMove(from, to, promotion);
    }

    public Vector2Int AlgebraicToCoordinate(string algebraic)
    {
        if (algebraic.Length != 2)
        {
            Debug.LogError($"Invalid algebraic notation: {algebraic}");
            return new Vector2Int(-1, -1);
        }

        int file = algebraic[0] - 'a';
        int rank = algebraic[1] - '1';

        return new Vector2Int(file, rank);
    }

    public string CoordinateToAlgebraic(Vector2Int coords)
    {
        char file = (char)('a' + coords.x);
        char rank = (char)('1' + coords.y);
        return $"{file}{rank}";
    }
}