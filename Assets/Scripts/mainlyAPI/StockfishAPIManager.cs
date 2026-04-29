using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class StockfishAPIManager : MonoBehaviour
{
    public enum Difficulty { Easy, Hard }

    [Header("AI Settings")]
    [SerializeField] private TeamColour aiTeam = TeamColour.Black;
    [SerializeField] private bool enableAI = true;

    [Header("Request Throttling")]
    [SerializeField] private float minDelay = 1f;
    [SerializeField] private float maxDelay = 3f;

    private Difficulty difficulty = Difficulty.Easy;
    private int currentDepth = 5;
    private MoveConverter moveConverter;
    private Board board;
    private ChessGameController gameController;
    private FENGenerator fenGenerator;
    private Coroutine depthFluctuationCoroutine;

    private void Awake()
    {
        moveConverter = GetComponent<MoveConverter>();
        board = FindFirstObjectByType<Board>();
        gameController = FindFirstObjectByType<ChessGameController>();
        fenGenerator = GetComponent<FENGenerator>();

        if (fenGenerator == null)
            fenGenerator = gameObject.AddComponent<FENGenerator>();
    }

    private void Start()
    {
        int playWithAI = PlayerPrefs.GetInt("PlayWithAI", 0);

        if (playWithAI == 1)
        {
            enableAI = true;
            int difficultyValue = PlayerPrefs.GetInt("AIDifficulty", 0);
            difficulty = difficultyValue == 0 ? Difficulty.Easy : Difficulty.Hard;
            SetDifficulty(difficulty);
        }
        else
        {
            enableAI = false;
        }
    }

    private void SetDifficulty(Difficulty newDifficulty)
    {
        difficulty = newDifficulty;

        if (depthFluctuationCoroutine != null)
        {
            StopCoroutine(depthFluctuationCoroutine);
            depthFluctuationCoroutine = null;
        }

        if (difficulty == Difficulty.Easy)
        {
            currentDepth = Random.Range(5, 7);
            depthFluctuationCoroutine = StartCoroutine(FluctuateDepthEasy());
            Debug.Log("AI Difficulty set to EASY - Depth fluctuates between 5-6");
        }
        else if (difficulty == Difficulty.Hard)
        {
            currentDepth = Random.Range(10, 16);
            depthFluctuationCoroutine = StartCoroutine(FluctuateDepthHard());
            Debug.Log("AI Difficulty set to HARD - Depth fluctuates between 10-15");
        }
    }

    private IEnumerator FluctuateDepthEasy()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            currentDepth = Random.Range(5, 7);
            Debug.Log("Easy AI depth changed to: " + currentDepth);
        }
    }

    private IEnumerator FluctuateDepthHard()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            currentDepth = Random.Range(10, 16);
            Debug.Log("Hard AI depth changed to: " + currentDepth);
        }
    }

    public bool IsAITurn(TeamColour currentTeam)
    {
        return enableAI && currentTeam == aiTeam;
    }

    public void RequestAIMove(string fen)
    {
        StartCoroutine(GetBestMoveFromAPI(fen));
    }

    private IEnumerator GetBestMoveFromAPI(string fen)
    {
        float randomDelay = Random.Range(minDelay, maxDelay);
        Debug.Log($"AI thinking for {randomDelay:F1} seconds at depth {currentDepth}...");
        yield return new WaitForSeconds(randomDelay);

        string url = $"https://stockfish.online/api/s/v2.php?fen={fen}&depth={currentDepth}&mode=bestmove";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API ERROR: " + request.error);
                yield break;
            }

            string raw = request.downloadHandler.text;
            Debug.Log("RAW API RESPONSE: " + raw);

            string moveStr = ExtractBestMove(raw);

            if (string.IsNullOrEmpty(moveStr))
            {
                Debug.LogError("Could not parse move from API response!");
                yield break;
            }

            Debug.Log("PARSED MOVE: " + moveStr);
            ExecuteAIMove(moveStr);
        }
    }

    private string ExtractBestMove(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return "";

        raw = raw.ToLower();

        if (raw.Contains("bestmove"))
        {
            int index = raw.IndexOf("bestmove");

            for (int i = index; i < raw.Length - 3; i++)
            {
                if (char.IsLetter(raw[i]) && char.IsDigit(raw[i + 1]) &&
                    char.IsLetter(raw[i + 2]) && char.IsDigit(raw[i + 3]))
                {
                    return raw.Substring(i, 4);
                }
            }
        }

        for (int i = 0; i < raw.Length - 3; i++)
        {
            if (char.IsLetter(raw[i]) && char.IsDigit(raw[i + 1]) &&
                char.IsLetter(raw[i + 2]) && char.IsDigit(raw[i + 3]))
            {
                return raw.Substring(i, 4);
            }
        }

        return "";
    }

    private void ExecuteAIMove(string moveString)
    {
        ChessMove move = moveConverter.ParseAlgebraicMove(moveString);

        if (move == null)
        {
            Debug.LogError("Move parsing failed!");
            return;
        }

        Piece piece = board.GetPieceOnSquare(move.from);

        if (piece == null)
        {
            Debug.LogError($"No piece at {move.from}");
            return;
        }

        if (piece.team != aiTeam)
        {
            Debug.LogError($"Wrong team piece at {move.from}");
            return;
        }

        Piece capturedPiece = board.GetPieceOnSquare(move.to);
        bool wasFirstMove = !piece.hasMoved;

        MoveRecord record = new MoveRecord(
            move.from,
            move.to,
            piece,
            capturedPiece,
            move.to,
            wasFirstMove,
            aiTeam
        );

        if (capturedPiece != null && !piece.IsFromSameTeam(capturedPiece))
        {
            capturedPiece.gameObject.SetActive(false);
            board.RemovePiece(capturedPiece);
        }

        board.UpdateBoardOnPieceMove(move.to, move.from, piece, null);
        piece.MovePiece(move.to);

        gameController.RecordMove(record);

        gameController.EndTurn();
    }

    private void OnDestroy()
    {
        if (depthFluctuationCoroutine != null)
        {
            StopCoroutine(depthFluctuationCoroutine);
        }
    }
}
