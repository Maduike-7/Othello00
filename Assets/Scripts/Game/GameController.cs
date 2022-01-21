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

    [Space]

    [Tooltip("Weighted chances that the CPU will make a certain move.\nLeft = worse move; Right = better move\nTop = higher chance; Bottom = lower chance")]
    [SerializeField] AnimationCurve[] cpuDifficultyCurves = new AnimationCurve[System.Enum.GetNames(typeof(UserSettings.CPUDifficulty)).Length];

    #endregion

    #region Actions

    public event System.Action DiscPlaceAction;
    public event System.Action<float> DiscFlipAction;
    public event System.Action<int, int> ScoreUpdateAction;
    public event System.Action<bool, int> TurnPassAction;
    public event System.Action GameOverAction;

    #endregion

    #region Game properties

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

    const int BoardSize = 8;

    readonly GameObject[,] gameBoard = new GameObject[BoardSize, BoardSize];
    GameState currentGameState;

    readonly GameObject[,] hintDiscs = new GameObject[BoardSize, BoardSize];

    //used for CPU to prioritize placing discs on these coordinates on harder difficulty settings (to-do: remove)
    List<(int, int)> corners = new List<(int, int)>(4);
    List<(int, int)> edges = new List<(int, int)>((BoardSize - 1) * 4);

    const float FlipAnimationDelay = 0.1f;
    float FlipAnimationDuration => userSettings.animationsOn ? 0.5f : 0f;

    public (int black, int white) discCount = (2, 2);

    bool inputEnabled = true;
    bool playerTurn = true;

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
        currentGameState = new GameState();

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                //get game board elements
                gameBoard[row, col] = discParent.GetChild(row * BoardSize + col).gameObject;
                hintDiscs[row, col] = hintDiscParent.transform.GetChild(row * BoardSize + col).gameObject;

                //get edge and corner coordinates
                if (row == 0 || col == 0 || row == BoardSize - 1 || col == BoardSize - 1)
                {
                    if ((row == 0 || row == BoardSize - 1) && (col == 0 || col == BoardSize - 1))
                    {
                        corners.Add((row, col));
                    }
                    else
                    {
                        edges.Add((row, col));
                    }
                }
            }
        }
    }

    void Start()
    {
        ShowHints();
    }

    void Update()
    {
        if (inputEnabled && playerTurn)
        {
            GetMouseInput();
        }
    }

    void GetMouseInput()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            //if raycast hit game board...
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetType() == typeof(BoxCollider))
            {
                //get row-column coordinates from [0, BoardSize - 1] based on (hit.point.xy / hit.collider.bounds.extents.xy)
                (int row, int col) selectedCoordinate;
                selectedCoordinate.row = Mathf.FloorToInt(hit.point.y / hit.collider.bounds.extents.y * (BoardSize / 2) + (BoardSize / 2));
                selectedCoordinate.col = Mathf.FloorToInt(hit.point.x / hit.collider.bounds.extents.x * (BoardSize / 2) + (BoardSize / 2));

                //if disc at selected coordinate is inactive, and if selectedCoordinate exists in validSpaces[], make move
                if (!gameBoard[selectedCoordinate.row, selectedCoordinate.col].activeInHierarchy && currentGameState.validSpaces.Any(item => item.coordinate == selectedCoordinate))
                {
                    StartCoroutine(MakeMove(selectedCoordinate, FindValidDirections(selectedCoordinate)));
                    HideHints();
                }
            }
        }
    }

    List<(Vector2Int, int)> FindValidDirections((int row, int col) coordinate)
    {
        List<(Vector2Int direction, int flipCount)> validDirections = new List<(Vector2Int direction, int flipCount)>();

        //call CheckInDirection for all 8 directions in checkDirections[], and store result as (valid check direction, amount to flip) in validDirections[]
        for (int i = 0; i < checkDirections.Length; i++)
        {
            int nextRow = coordinate.row + checkDirections[i].y;
            int nextCol = coordinate.col + checkDirections[i].x;

            if (nextRow >= 0 && nextRow < BoardSize && nextCol >= 0 && nextCol < BoardSize)
            {
                (bool isValid, int flipCount) = CheckInDirection(coordinate, checkDirections[i]);

                if (isValid)
                {
                    validDirections.Add((checkDirections[i], flipCount));
                }
            }
        }

        return validDirections;
    }

    (bool isValid, int flipCount) CheckInDirection((int row, int col) coordinate, Vector2Int direction)
    {
        int currentColourValue = playerTurn ? (int)CellType.Black : (int)CellType.White;
        int oppColourValue = playerTurn ? (int)CellType.White : (int)CellType.Black;
        int flipCount = 0;

        int nextRow = coordinate.row + direction.y;
        int nextCol = coordinate.col + direction.x;

        while (currentGameState.board[nextRow, nextCol] == oppColourValue)
        {
            nextRow += direction.y;
            nextCol += direction.x;
            flipCount++;

            if (nextRow >= 0 && nextRow < BoardSize && nextCol >= 0 && nextCol < BoardSize)
            {
                if (currentGameState.board[nextRow, nextCol] == currentColourValue)
                {
                    return (true, flipCount);
                }
            }
            else break;
        }

        return (false, 0);
    }

    IEnumerator MakeMove((int row, int col) coordinate, List<(Vector2Int direction, int flipCount)> validDirections)
    {
        //update board display and internal board state
        gameBoard[coordinate.row, coordinate.col].SetActive(true);
        currentGameState.board[coordinate.row, coordinate.col] = playerTurn ? (int)CellType.Black : (int)CellType.White;

        //increment disc count based on whose turn it is
        if (playerTurn)
        {
            discCount.black++;
        }
        else
        {
            discCount.white++;
        }

        //play sound
        if (userSettings.soundOn)
        {
            DiscPlaceAction?.Invoke();
        }

        //call FlipInDirection() for all directions in validDirections
        for (int i = 0; i < validDirections.Count; i++)
        {
            FlipInDirection(coordinate, validDirections[i].direction, validDirections[i].flipCount);
        }

        //disable input until flip animation finishes
        inputEnabled = false;

        //wait for seconds based on greatest flipCount in validDirections
        yield return WaitForSeconds(FlipAnimationDuration + FlipAnimationDelay * validDirections.Max(i => i.flipCount));
        inputEnabled = true;

        ClearConsole();
        print("current game board:");
        PrintGameState(currentGameState);
        PassTurn(0);
    }

    //flip all flippable discs in specified direction and 
    void FlipInDirection((int row, int col) coordinate, Vector2Int direction, int flipLength)
    {
        for (int i = 1; i <= flipLength; i++)
        {
            //set flip axis such that it looks like discs are flipping outward from position of placed disc
            Vector3 directionV3 = new Vector3(direction.x, direction.y);
            Vector3 flipAxis = Vector3.Cross(directionV3, playerTurn ? Vector3.forward : Vector3.back);

            //set flip delay based on flipLength such that it looks like discs are flipping one after another, instead of all at once
            float flipDelay = i * FlipAnimationDelay;

            int nextRow = coordinate.row + (direction.y * i);
            int nextCol = coordinate.col + (direction.x * i);

            //update
            gameBoard[nextRow, nextCol].GetComponent<Disc>().FlipUponAxis(flipAxis, FlipAnimationDuration, flipDelay);
            currentGameState.board[nextRow, nextCol] = playerTurn ? (int)CellType.Black : (int)CellType.White;

            //play disc flip sfx
            DiscFlipAction?.Invoke(flipDelay + FlipAnimationDuration);

            //increment/decrement disc counts accordingly based on whose turn it is
            if (playerTurn)
            {
                discCount.black++;
                discCount.white--;
            }
            else
            {
                discCount.black--;
                discCount.white++;
            }
        }
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
            playerTurn = !playerTurn;

            //check all coordinates of inactive discs to see if a move can be made there, given whose turn it is
            currentGameState.validSpaces.Clear();

            currentGameState.childGameStates.Clear();
            FindChildGameStates(currentGameState);

            //if at least 1 valid move exists...
            if (currentGameState.validSpaces.Count > 0)
            {
                if (playerTurn)
                {
                    ShowHints();
                }
                else
                {
                    StartCoroutine(RunCPU());
                }

                TurnPassAction?.Invoke(playerTurn, turnsPassed);
            }
            //if a valid move doesn't exist, increment turnsPassed counter; call this function again
            else
            {
                turnsPassed++;
                PassTurn(turnsPassed);
            }
        }
        //game is over when turn has been passed twice without a move being made
        //(checking if board is full is not good enough because gameover states exist where the board isn't completely filled and neither player can make a move)
        else
        {
            GameOverAction?.Invoke();
        }

        ScoreUpdateAction?.Invoke(discCount.black, discCount.white);
    }

    void FindChildGameStates(GameState state)
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                if (currentGameState.board[row, col] == 0)
                {
                    List<(Vector2Int direction, int flipCount)> validDirections = FindValidDirections((row, col));

                    //if placing a disc at [row, col] makes for valid move, add to validSpaces[], with total number of discs flipped
                    if (validDirections.Count > 0)
                    {
                        int totalFlipCount = validDirections.Sum(i => i.flipCount);
                        state.validSpaces.Add(((row, col), totalFlipCount));

                        int[,] stateCopy = currentGameState.board.Clone() as int[,];

                        stateCopy[row, col] = playerTurn ? (int)CellType.Black : (int)CellType.White;

                        for (int i = 0; i < validDirections.Count; i++)
                        {
                            for (int j = 1; j <= validDirections[i].flipCount; j++)
                            {
                                int nextRow = row + (j * validDirections[i].direction.y);
                                int nextCol = col + (j * validDirections[i].direction.x);

                                stateCopy[nextRow, nextCol] = playerTurn ? (int)CellType.Black : (int)CellType.White;
                            }
                        }

                        print("new future game state found.");

                        GameState newChildState = new GameState(stateCopy);
                        PrintGameState(newChildState);

                        state.childGameStates.Add(newChildState);
                    }
                }
            }
        }
    }

    IEnumerator RunCPU()
    {
        //order validSpaces[] by whether or not item exists in corners[], then by whether or not item exists in edges[], then by item's total number of discs to flip
        currentGameState.validSpaces = currentGameState.validSpaces.OrderBy(i => corners.Contains(i.coordinate))
                          .ThenBy(i => edges.Contains(i.coordinate))
                          .ThenBy(i => i.flipCount).ToList();

        currentGameState.validSpaces.ForEach(v => print(v));

        (int row, int col) selectedCoordinate = FindCPUMove();

        //more possible valid spaces = longer wait time (for added realism)
        float cpuDelay = 0.5f + (currentGameState.validSpaces.Count / 8f);
        yield return WaitForSeconds(cpuDelay);

        yield return MakeMove(selectedCoordinate, FindValidDirections(selectedCoordinate));
    }

    //get coordinates of CPU's next move, based on <cpuDifficulty>
    (int row, int col) FindCPUMove()
    {
        int cpuDifficulty = (int)userSettings.cpuDifficulty;
        List<float> moveSelectionWeights = new List<float>(currentGameState.validSpaces.Count);

        for (int i = 0; i < currentGameState.validSpaces.Count; i++)
        {
            moveSelectionWeights.Add(cpuDifficultyCurves[cpuDifficulty].Evaluate((i + 1f) / (currentGameState.validSpaces.Count + 1f)));
        }

        return currentGameState.validSpaces[GetRandomWeightedIndex(moveSelectionWeights)].coordinate;
    }

    int GetRandomWeightedIndex(List<float> weights)
    {
        //get sum of weights
        float weightSum = weights.Sum();

        //loop through all weights; see if each one is selected
        for (int i = 0; i < weights.Count; i++)
        {
            //randomize between 0 and sum of weights; if random number is less than current weight being checked, return index of that number
            if (Random.Range(0, weightSum) < weights[i])
            {
                return i;
            }

            //otherwise remove current weight from sum of weights; repeat
            weightSum -= weights[i];
        }

        //return last index if all previous items weren't selected
        return weights.Count - 1;
    }

    void ShowHints()
    {
        if (!userSettings.hintsOn) { return; }

        HideHints();

        //if there are any coordinates that the player can place a disc on
        if (currentGameState.validSpaces.Any())
        {
            //display hint disc for each valid space
            currentGameState.validSpaces.ForEach(i =>
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
        for (int i = currentGameState.board.GetLength(0) - 1; i >= 0; i--)
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
    public readonly int[,] board = new int[8, 8];

    public List<((int row, int col) coordinate, int flipCount)> validSpaces = new List<((int, int), int)>();
    public List<GameState> childGameStates = new List<GameState>();

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

        validSpaces.Add(((2, 4), 1));
        validSpaces.Add(((3, 5), 1));
        validSpaces.Add(((4, 2), 1));
        validSpaces.Add(((5, 3), 1));
    }

    public GameState(int[,] _board)
    {
        board = _board;
    }
}