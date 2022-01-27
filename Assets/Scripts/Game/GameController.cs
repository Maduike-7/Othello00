using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CoroutineHelper;

public class GameController : MonoBehaviour
{
    #region Editor fields

    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] AudioController audioController;
    [SerializeField] Transform discParent;
    [SerializeField] GameObject hintDiscParent;
    Camera mainCam;

    #endregion

    #region Game properties

    public GameState CurrentGameState = new GameState();

    const int BoardSize = GameState.BoardSize;
    readonly GameObject[,] gameBoard = new GameObject[BoardSize, BoardSize];
    readonly GameObject[,] hintDiscs = new GameObject[BoardSize, BoardSize];

    const float FlipAnimationDelay = 0.1f;
    float FlipAnimationDuration => userSettings.animationsOn ? 0.5f : 0f;

    bool inputEnabled = true;

    #endregion

    #region Actions

    public event System.Action DiscPlaceAction;
    public event System.Action<float> DiscFlipAction;
    public event System.Action<int, int> ScoreUpdateAction;
    public event System.Action<bool, int> TurnPassAction;
    public event System.Action GameOverAction;

    #endregion

    void Awake()
    {
        mainCam = Camera.main;

        GameOverAction += OnGameOver;
        FindObjectOfType<PauseHandler>().GamePauseAction += OnGamePaused;

        InitGame();
    }

    void InitGame()
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                //get game board elements
                gameBoard[row, col] = discParent.GetChild(row * BoardSize + col).gameObject;
                hintDiscs[row, col] = hintDiscParent.transform.GetChild(row * BoardSize + col).gameObject;
            }
        }
    }

    void Start()
    {
        ShowHints();
    }

    void Update()
    {
        if (inputEnabled && CurrentGameState.IsPlayerTurn)
        {
            GetMouseInput();
        }
    }

    void GetMouseInput()
    {
        //0 = L. mouse button
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            //if raycast hit game board...
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetType() == typeof(BoxCollider))
            {
                //get row-column coordinates based on where user clicked on game board
                (int row, int col) selectedCoordinate;
                selectedCoordinate.row = Mathf.FloorToInt(hit.point.y / hit.collider.bounds.extents.y * (BoardSize / 2) + (BoardSize / 2));
                selectedCoordinate.col = Mathf.FloorToInt(hit.point.x / hit.collider.bounds.extents.x * (BoardSize / 2) + (BoardSize / 2));

                //make move if selected coordinate exists in list of valid moves
                if (CurrentGameState.validMoves.Any(item => (item.coordinate.row, item.coordinate.col) == selectedCoordinate))
                {
                    List<(Vector2Int direction, int flipCount)> flipDirections = CurrentGameState.GetFlipDirections(CurrentGameState.IsPlayerTurn, selectedCoordinate);

                    CurrentGameState.ApplyMove(CurrentGameState.IsPlayerTurn, selectedCoordinate, flipDirections);
                    StartCoroutine(DisplayMove(selectedCoordinate, flipDirections));
                    HideHints();
                }
            }
        }
    }

    //show the move happening on the game board
    IEnumerator DisplayMove((int row, int col) coordinate, List<(Vector2Int direction, int flipCount)> flipDirections)
    {
        //disable input until flip animation finishes
        inputEnabled = false;

        gameBoard[coordinate.row, coordinate.col].SetActive(true);

        if (userSettings.soundOn)
        {
            DiscPlaceAction?.Invoke();
        }

        //flip all flippable discs for all directions in flipDirections
        for (int i = 0; i < flipDirections.Count; i++)
        {
            Vector2Int flipDirection = flipDirections[i].direction;

            for (int j = 1; j <= flipDirections[i].flipCount; j++)
            {
                //set flip axis such that it looks like discs are flipping outward from position of placed disc
                Vector3 directionV3 = new Vector3(flipDirection.x, flipDirection.y);
                Vector3 flipAxis = Vector3.Cross(directionV3, CurrentGameState.IsPlayerTurn ? Vector3.forward : Vector3.back);

                //set flip delay to achieve a cascading flip animation
                float flipDelay = j * FlipAnimationDelay;

                int nextRow = coordinate.row + (flipDirection.y * j);
                int nextCol = coordinate.col + (flipDirection.x * j);

                gameBoard[nextRow, nextCol].GetComponent<Disc>().FlipUponAxis(flipAxis, FlipAnimationDuration, flipDelay);
                DiscFlipAction?.Invoke(flipDelay + FlipAnimationDuration);
            }
        }

        //wait for seconds based on greatest flipCount in flipDirections, before re-enabling input
        yield return WaitForSeconds(FlipAnimationDuration + FlipAnimationDelay * flipDirections.Max(i => i.flipCount));
        inputEnabled = true;

        //debug
        ClearConsole();
        print("current game board:");
        PrintGameState(CurrentGameState);

        PassTurn(0);
    }

    //handle events that happen between turns
    void PassTurn(int turnsPassed)
    {
        if (turnsPassed < 2)
        {
            //flip all inactive discs
            foreach (var disc in gameBoard)
            {
                if (!disc.activeInHierarchy)
                {
                    disc.GetComponent<Disc>().FlipUponAxis(Vector3.right);
                }
            }

            //pass the turn over
            CurrentGameState.PassTurn();

            //check all coordinates of inactive discs to see if a move can be made there, given whose turn it is
            CurrentGameState.validMoves.Clear();
            CurrentGameState.GetValidMoves(CurrentGameState.IsPlayerTurn);

            //if at least 1 valid move exists...
            if (CurrentGameState.validMoves.Count > 0)
            {
                if (CurrentGameState.IsPlayerTurn)
                {
                    ShowHints();
                }
                else
                {
                    CurrentGameState.GetCPUMove();
                }

                TurnPassAction?.Invoke(CurrentGameState.IsPlayerTurn, turnsPassed);
            }
            //if a valid move doesn't exist, increment turnsPassed counter and pass the turn again
            else
            {
                turnsPassed++;
                PassTurn(turnsPassed);
            }
        }
        //game is over when turn has been passed twice without a move being made.
        //(checking if board is full is not good enough because gameover states exist where
        //the board isn't completely filled and neither player can make a move)
        else
        {
            GameOverAction?.Invoke();
        }

        (int black, int white) = CurrentGameState.DiscCount;
        ScoreUpdateAction?.Invoke(black, white);
    }

    void ShowHints()
    {
        if (!userSettings.hintsOn) { return; }

        HideHints();

        //if there are any coordinates that the player can place a disc on
        if (CurrentGameState.validMoves.Any())
        {
            //display hint disc for each valid space
            CurrentGameState.validMoves.ForEach(i =>
            {
                hintDiscs[i.coordinate.row, i.coordinate.col].SetActive(true);
            });
        }
    }

    void HideHints()
    {
        for (int i = 0; i < hintDiscs.GetLength(0); i++)
        {
            for (int j = 0; j < hintDiscs.GetLength(1); j++)
            {
                hintDiscs[i, j].SetActive(false);
            }
        }
    }

    void OnGamePaused(bool state)
    {
        enabled = !state;
    }

    void OnGameOver()
    {
        enabled = false;
    }

    //debug
    #region Console Logs
    void PrintGameState(GameState gameState)
    {
        print("_________________");
        for (int i = gameState.board.GetLength(0) - 1; i >= 0; i--)
        {
            string output = "";

            for (int j = 0; j < gameState.board.GetLength(1); j++)
            {
                if (gameState.board[i, j] == 0)
                {
                    output += "_| ";
                }
                else if (gameState.board[i, j] == 1)
                {
                    output += "●| ";
                }
                else if (gameState.board[i, j] == -1)
                {
                    output += "○| ";
                }

                //does same as above but only supported in v2020.2+ smh
                /*
                output += (CellType)gameState.board[i, j] switch
                { 
                    CellType.Empty => "□, ",
                    CellType.Black => "●, ",
                    CellType.White => "○, ",
                    _ => "",
                };
                */
            }

            print(output);
        }
    }

    void ClearConsole()
    {
        System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.SceneView)).GetType("UnityEditor.LogEntries").GetMethod("Clear").Invoke(new object(), null);
    }
    #endregion
}

public class GameState
{
    enum CellType
    {
        Empty = 0,
        Black = 1,
        White = -1
    }

    readonly Vector2Int[] checkDirections = new Vector2Int[8]
    {
        new Vector2Int( 0, 1),
        new Vector2Int( 1, 1),
        new Vector2Int( 1, 0),
        new Vector2Int( 1,-1),
        new Vector2Int( 0,-1),
        new Vector2Int(-1,-1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1)
    };

    public const int BoardSize = 8;
    public readonly int[,] board = new int[BoardSize, BoardSize];

    public List<((int row, int col) coordinate, int evaluation)> validMoves = new List<((int, int), int)>(BoardSize);

    public bool IsPlayerTurn { get; private set; } = true;

    public (int black, int white) DiscCount
    {
        get
        {
            int black = 0;
            int white = 0;

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == 1) black++;
                    if (board[i, j] == -1) white++;
                }
            }

            return (black, white);
        }
    }

    //to-do: impl. properly
    public bool IsGameOver;

    //to-do: impl. properly
    public int Evaluation
    {
        get
        {
            int e = 0;

            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    e += board[i, j];
                }
            }

            return e;
        }
    }

    public GameState()
    {
        board[3, 3] = 1;
        board[3, 4] = -1;
        board[4, 3] = -1;
        board[4, 4] = 1;

        GetValidMoves(IsPlayerTurn);
    }

    public List<(Vector2Int, int)> GetFlipDirections(bool playerTurn, (int row, int col) coordinate)
    {
        List<(Vector2Int direction, int flipCount)> flipDirections = new List<(Vector2Int, int)>();

        int currentColourValue = playerTurn ? (int)CellType.Black : (int)CellType.White;
        int oppColourValue = playerTurn ? (int)CellType.White : (int)CellType.Black;

        //check for discs to flip for all 8 directions in checkDirections[]
        for (int i = 0; i < checkDirections.Length; i++)
        {
            //continue incrementing check coordinate by check direction
            int nextRow = coordinate.row + checkDirections[i].y;
            int nextCol = coordinate.col + checkDirections[i].x;

            //make sure next coordinate to check is not out of bounds
            if (nextRow >= 0 && nextRow < BoardSize && nextCol >= 0 && nextCol < BoardSize)
            {
                int flipCount = 0;

                //while next coordinate has a disc of the opposite colour
                while (board[nextRow, nextCol] == oppColourValue)
                {
                    flipCount++;

                    //keep incrementing check coordinate by check direction
                    nextRow += checkDirections[i].y;
                    nextCol += checkDirections[i].x;

                    //check to see if there is eventually another disc of the same colour
                    if (nextRow >= 0 && nextRow < BoardSize && nextCol >= 0 && nextCol < BoardSize)
                    {
                        //if there is another disc of the same colour ("sandwich" confirmed), add to flipDirections
                        if (board[nextRow, nextCol] == currentColourValue)
                        {
                            flipDirections.Add((checkDirections[i], flipCount));
                        }
                    }
                    else break;
                }
            }
        }

        return flipDirections;
    }

    void ToggleDisc((int row, int col) coordinate)
    {
        if (board[coordinate.row, coordinate.col] == (int)CellType.Empty) return;

        board[coordinate.row, coordinate.col] = board[coordinate.row, coordinate.col] == (int)CellType.Black ? (int)CellType.White : (int)CellType.Black;
    }

    public void ApplyMove(bool playerTurn, (int row, int col) coordinate, List<(Vector2Int direction, int flipCount)> flipDirections)
    {
        if (board[coordinate.row, coordinate.col] != (int)CellType.Empty) return;

        board[coordinate.row, coordinate.col] = playerTurn ? (int)CellType.Black : (int)CellType.White;

        for (int i = 0; i < flipDirections.Count; i++)
        {
            Vector2Int flipDirection = flipDirections[i].direction;

            for (int j = 1; j <= flipDirections[i].flipCount; j++)
            {
                int nextRow = coordinate.row + (flipDirection.y * j);
                int nextCol = coordinate.col + (flipDirection.x * j);

                ToggleDisc((nextRow, nextCol));
            }
        }
    }

    public void UndoApplyMove((int row, int col) coordinate, List<(Vector2Int direction, int flipCount)> flipDirections)
    {
        if (board[coordinate.row, coordinate.col] == (int)CellType.Empty) return;

        board[coordinate.row, coordinate.col] = (int)CellType.Empty;

        for (int i = 0; i < flipDirections.Count; i++)
        {
            Vector2Int flipDirection = flipDirections[i].direction;

            for (int j = 1; j <= flipDirections[i].flipCount; j++)
            {
                int nextRow = coordinate.row + (flipDirection.y * j);
                int nextCol = coordinate.col + (flipDirection.x * j);

                ToggleDisc((nextRow, nextCol));
            }
        }
    }

    public void GetValidMoves(bool playerTurn)
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            for (int col = 0; col < board.GetLength(1); col++)
            {
                //for all empty cells, check if placing a disc that cell makes for a valid move
                if (board[row, col] == 0)
                {
                    List<(Vector2Int direction, int flipCount)> flipDirections = GetFlipDirections(playerTurn, (row, col));

                    if (flipDirections.Count > 0)
                    {
                        ApplyMove(playerTurn, (row, col), flipDirections);

                        validMoves.Add(((row, col), Evaluation));

                        print("new future game state found.");
                        PrintGameState();

                        UndoApplyMove((row, col), flipDirections);
                    }
                }
            }
        }
    }

    public ((int, int), int) GetCPUMove()
    {
        int minEvaluation = int.MaxValue;
        (int row, int col) bestMove = validMoves[0].coordinate;

        foreach (var move in validMoves)
        {
            minEvaluation = Mathf.Min(minEvaluation, move.evaluation);
        }

        return ((bestMove), minEvaluation);
    }

    
    public int Minimax((int row, int col) coordinate, int depth, float alpha, float beta, bool maximizingPlayer)
    {
        if (depth == 0 || IsGameOver)
        {
            return Evaluation;
        }

        if (maximizingPlayer)
        {
            int maxEvaluation = int.MinValue;

            foreach (var move in validMoves)
            {
                int evaluation = Minimax(move.coordinate, depth - 1, alpha, beta, false);

                maxEvaluation = Mathf.Max(maxEvaluation, evaluation);
                alpha = Mathf.Max(alpha, evaluation);

                if (beta <= alpha) break;
            }

            return maxEvaluation;
        }
        else
        {
            int minEvaluation = int.MaxValue;

            foreach (var move in validMoves)
            {
                int evaluation = Minimax(move.coordinate, depth - 1, alpha, beta, true);

                minEvaluation = Mathf.Min(minEvaluation, evaluation);
                beta = Mathf.Min(beta, evaluation);

                if (beta <= alpha) break;
            }

            return minEvaluation;
        }
    }
    

    public void PassTurn()
    {
        IsPlayerTurn = !IsPlayerTurn;
    }

    //debug
    #region Console Logs
    void PrintGameState()
    {
        print("_________________");
        for (int i = board.GetLength(0) - 1; i >= 0; i--)
        {
            string output = "";

            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == 0)
                {
                    output += "_| ";
                }
                else if (board[i, j] == 1)
                {
                    output += "●| ";
                }
                else if (board[i, j] == -1)
                {
                    output += "○| ";
                }
            }

            print(output);
        }
    }

    void print(object msg)
    {
        Debug.Log(msg);
    }
    #endregion
}