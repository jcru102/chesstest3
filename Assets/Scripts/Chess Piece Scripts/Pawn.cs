using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Pawn : Piece
{
    public static Pawn lastPawnThatMovedTwoSquares;
    private bool justMovedTwoSquares = false;

    private List<Vector2Int> enPassantMoves = new List<Vector2Int>();

    public override List<Vector2Int> SelectAvailableSquares()
    {
        availableMoves.Clear();
        enPassantMoves.Clear(); 

        Vector2Int direction = team == TeamColour.White ? Vector2Int.up : Vector2Int.down;
        float range = hasMoved ? 1 : 2;

        //movement
        for (int i = 1; i <= range; i++)
        {
            Vector2Int nextCoords = occupiedSquare + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinatesAreOnBoard(nextCoords))
                break;
            if (piece == null)
                TryToAddMove(nextCoords);
            else
                break; //stop when u hit a piece
        }

        //diagonals
        Vector2Int[] takeDirections = new Vector2Int[] {
            new Vector2Int(1, direction.y),
            new Vector2Int(-1, direction.y)
        };

        //enpassant
        if (lastPawnThatMovedTwoSquares != null && lastPawnThatMovedTwoSquares != this)
        {
            Vector2Int enemyPos = lastPawnThatMovedTwoSquares.occupiedSquare;
            if (enemyPos.y == occupiedSquare.y && Mathf.Abs(enemyPos.x - occupiedSquare.x) == 1)
            {
                Vector2Int enPassantMove = enemyPos + direction;
                if (board.CheckIfCoordinatesAreOnBoard(enPassantMove))
                {
                    TryToAddMove(enPassantMove);
                    enPassantMoves.Add(enPassantMove);
                }
            }
        }

        for (int i = 0; i < takeDirections.Length; i++)
        {
            Vector2Int nextCoords = occupiedSquare + takeDirections[i];

            if (!board.CheckIfCoordinatesAreOnBoard(nextCoords))
                continue;

            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (piece != null && !piece.IsFromSameTeam(this))
                TryToAddMove(nextCoords);
        }

        return availableMoves;
    }

    
    public bool IsEnPassantMove(Vector2Int coords)
    {
        return enPassantMoves.Contains(coords);
    }

    public override void MovePiece(Vector2Int coords)
    {
        
        bool isEnPassantCapture = false;
        if (lastPawnThatMovedTwoSquares != null)
        {
            Vector2Int enemyPos = lastPawnThatMovedTwoSquares.occupiedSquare;
            Vector2Int direction = team == TeamColour.White ? Vector2Int.up : Vector2Int.down;

            
            if (coords.x == enemyPos.x &&
                coords.y == enemyPos.y + direction.y &&
                enemyPos.y == occupiedSquare.y &&
                Mathf.Abs(enemyPos.x - occupiedSquare.x) == 1)
            {
                isEnPassantCapture = true;
                board.SetPieceOnBoard(enemyPos, null);
                board.RemovePiece(lastPawnThatMovedTwoSquares);
            }
        }

        int moveDistance = Mathf.Abs(coords.y - occupiedSquare.y);

        
        base.MovePiece(coords);

        if (moveDistance == 2)
        {
            lastPawnThatMovedTwoSquares = this;
        }
        else
        {
            lastPawnThatMovedTwoSquares = null;
        }

       
        CheckPromotion();
    }

    private void CheckPromotion()
    {
        int endOfBoardYCoord = team == TeamColour.White ? Board.BOARD_SIZE - 1 : 0;
        if (occupiedSquare.y == endOfBoardYCoord)
            board.PromotePiece(this);
    }
}
