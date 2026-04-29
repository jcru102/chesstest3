using UnityEngine;

[System.Serializable]
public class ChessMove
{
    public Vector2Int from;
    public Vector2Int to;
    public string promotion;

    public ChessMove(Vector2Int from, Vector2Int to, string promotion = "")
    {
        this.from = from;
        this.to = to;
        this.promotion = promotion;
    }
}