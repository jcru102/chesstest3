using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(SquareSelectorCreator))]
public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 8;
    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float SquareSize;

    private Piece[,] grid;
    private Piece selectedPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;

    private void Awake()
    {
        squareSelector = GetComponent<SquareSelectorCreator>();
        CreateGrid();
    }

    public void SetDependencies(ChessGameController chessController)
    {
        this.chessController = chessController;
    }

    private void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    internal Vector3 CalculatePositionFromCoords(Vector2Int coords)
    {
        return bottomLeftSquareTransform.position + new Vector3(coords.x * SquareSize, 0f, coords.y * SquareSize);
    }

    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(inputPosition.x / SquareSize) + BOARD_SIZE / 2;
        int y = Mathf.FloorToInt(inputPosition.z / SquareSize) + BOARD_SIZE / 2;
        return new Vector2Int(x, y);
    }

    internal bool HasPiece(Piece piece)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (grid[i, j] == piece)
                    return true;
            }
        }
        return false;
    }

    public void RemovePiece(Piece piece)
    {
        chessController.OnPieceRemoved(piece);
    }

    public void OnSquareSelected(Vector3 inputPosition)
    {
        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
                DeselectPiece();
            else if (piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team))
                SelectPiece(piece);
            else if (selectedPiece.CanMoveTo(coords))
                OnSelectedPieceMoved(coords, selectedPiece);
        }
        else
        {
            if (piece != null && chessController.IsTeamTurnActive(piece.team))
                SelectPiece(piece);
        }
    }

    public void OnSelectedPieceMoved(Vector2Int coords, Piece piece)
    {
        Vector2Int originalSquare = piece.occupiedSquare;
        bool wasFirstMove = !piece.hasMoved;

        Piece capturedPiece = GetPieceOnSquare(coords);
        Vector2Int capturedSquare = coords;

        if (piece is Pawn pawn && capturedPiece == null)
        {
            if (pawn.IsEnPassantMove(coords))
            {
                Vector2Int enPassantCaptureSquare = new Vector2Int(coords.x, piece.occupiedSquare.y);
                capturedPiece = GetPieceOnSquare(enPassantCaptureSquare);
                capturedSquare = enPassantCaptureSquare;
            }
        }

        bool isCastling = false;
        Vector2Int rookFrom = Vector2Int.zero;
        Vector2Int rookTo = Vector2Int.zero;
        Piece rook = null;

        if (piece is King king && Mathf.Abs(coords.x - originalSquare.x) == 2)
        {
            isCastling = true;

            if (coords.x > originalSquare.x)
            {
                rookFrom = new Vector2Int(7, originalSquare.y);
                rookTo = new Vector2Int(5, originalSquare.y);
            }
            else
            {
                rookFrom = new Vector2Int(0, originalSquare.y);
                rookTo = new Vector2Int(3, originalSquare.y);
            }

            rook = GetPieceOnSquare(rookFrom);
        }

        MoveRecord record = new MoveRecord(
            originalSquare,
            coords,
            piece,
            capturedPiece,
            capturedSquare,
            wasFirstMove,
            piece.team
        );

        if (isCastling && rook != null)
        {
            record.wasCastling = true;
            record.rookFromSquare = rookFrom;
            record.rookToSquare = rookTo;
            record.castledRook = rook;
        }

        if (capturedPiece != null)
        {
            grid[capturedSquare.x, capturedSquare.y] = null;
            capturedPiece.gameObject.SetActive(false);

            ChessPlayer opponent = capturedPiece.team == TeamColour.White ?
                chessController.GetWhitePlayer() : chessController.GetBlackPlayer();
            opponent.RemovePiece(capturedPiece);
        }

        UpdateBoardOnPieceMove(coords, originalSquare, piece, null);
        selectedPiece.MovePiece(coords);

        chessController.RecordMove(record);

        DeselectPiece();
        EndTurn();
    }

    private void EndTurn()
    {
        chessController.EndTurn();
    }

    public void UpdateBoardOnPieceMove(Vector2Int newcoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece)
    {
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newcoords.x, newcoords.y] = newPiece;
    }

    private void SelectPiece(Piece piece)
    {
        chessController.RemoveMovesEnablingAttackOnPieceOfTypes<King>(piece);
        selectedPiece = piece;
        List<Vector2Int> selection = selectedPiece.availableMoves;
        ShowSelectionSquares(selection);
    }

    private void ShowSelectionSquares(List<Vector2Int> selection)
    {
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for (int i = 0; i < selection.Count; i++)
        {
            Vector3 position = CalculatePositionFromCoords(selection[i]);
            bool isSquareFree = GetPieceOnSquare(selection[i]) == null;

            if (selectedPiece is Pawn pawn && isSquareFree)
            {
                if (pawn.IsEnPassantMove(selection[i]))
                {
                    isSquareFree = false;
                }
            }

            squaresData.Add(position, isSquareFree);
        }
        squareSelector.ShowSelection(squaresData);
    }

    private void DeselectPiece()
    {
        selectedPiece = null;
        squareSelector.ClearSelection();
    }

    public Piece GetPieceOnSquare(Vector2Int coords)
    {
        if (CheckIfCoordinatesAreOnBoard(coords))
            return grid[coords.x, coords.y];
        return null;
    }

    public bool CheckIfCoordinatesAreOnBoard(Vector2Int coords)
    {
        if (coords.x < 0 || coords.y < 0 || coords.x >= BOARD_SIZE || coords.y >= BOARD_SIZE)
            return false;
        return true;
    }

    public void SetPieceOnBoard(Vector2Int coords, Piece piece)
    {
        if (CheckIfCoordinatesAreOnBoard(coords))
            grid[coords.x, coords.y] = piece;
    }

    public void OnGameRestarted()
    {
        selectedPiece = null;
        CreateGrid();
    }

    public void PromotePiece(Piece piece)
    {
        Vector2Int square = piece.occupiedSquare;
        TeamColour team = piece.team;
        System.Type originalType = piece.GetType();

        MoveRecord promotionRecord = new MoveRecord(
            square,
            square,
            piece,
            null,
            square,
            false,
            team
        );
        promotionRecord.wasPromotion = true;
        promotionRecord.originalPieceType = originalType;

        grid[square.x, square.y] = null;
        chessController.OnPieceRemoved(piece);
        chessController.CreatePieceAndInitialize(square, team, typeof(Queen));

        chessController.RecordMove(promotionRecord);
    }
}
