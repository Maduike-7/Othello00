using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CoroutineHelper;

public class GameController : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] AudioController audioController;
    [SerializeField] Transform discParent;
    [SerializeField] GameObject hintDiscParent;
    Camera mainCam;

    [Space]

    [Tooltip("Weighted chances that the CPU will make a certain move.\nLeft = worse move; Right = better move\nTop = higher chance; Bottom = lower chance")]
    [SerializeField] AnimationCurve[] cpuDifficultyCurves = new AnimationCurve[System.Enum.GetNames(typeof(UserSettings.CPUDifficulty)).Length];

    public event System.Action DiscPlaceAction;
    public event System.Action<float> DiscFlipAction;
    public event System.Action<int, int> ScoreUpdateAction;
    public event System.Action<bool, int> TurnPassAction;
    public event System.Action GameOverAction;

    const int BoardSize = 8;

    readonly GameObject[,] gameBoard = new GameObject[BoardSize, BoardSize];
    GameState currentGameState = new GameState();

    readonly GameObject[,] hintDiscs = new GameObject[BoardSize, BoardSize];

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

    //list to hold (direction in which >0 discs would be flipped, number of discs to flip)
    //List<(Vector2Int direction, int flipCount)> validDirections = new List<(Vector2Int, int)>(8);

    //list to hold (row-column coordinate of board if a disc placed there would result in a valid move, total number of discs that would flip if a disc was placed there)
    List<((int row, int col) coordinate, int totalFlipCount)> validSpaces = new List<((int, int), int)>();

    //used for CPU to prioritize placing discs on these coordinates on harder difficulty settings
    List<(int, int)> corners = new List<(int, int)>(4);
    List<(int, int)> edges = new List<(int, int)>((BoardSize - 1) * 4);

    const float FlipAnimationDelay = 0.1f;
    const float FlipAnimationDuration = 0.5f;

    float flipAnimationDuration => userSettings.animationsOn ? FlipAnimationDuration : 0f;

    int BlackDiscLayer => LayerMask.NameToLayer("Black Disc");
    int WhiteDiscLayer => LayerMask.NameToLayer("White Disc");
    public (int black, int white) discCount = (2, 2);

    bool inputEnabled = true;
    bool playerTurn = true;

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

        //initialize valid move coordinates
        validSpaces.Add(((2, 4), 1));
        validSpaces.Add(((3, 5), 1));
        validSpaces.Add(((4, 2), 1));
        validSpaces.Add(((5, 3), 1));
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
                (int row, int col) selectedCoordinate = InverseTransformPoint(hit);

                //if disc at selected coordinate is inactive, and if selectedCoordinate exists in validSpaces[], make move
                if (!gameBoard[selectedCoordinate.row, selectedCoordinate.col].activeInHierarchy && validSpaces.Any(item => item.coordinate == selectedCoordinate))
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
            if (coordinate.row + checkDirections[i].y >= 0 && coordinate.row + checkDirections[i].y < BoardSize &&
                coordinate.col + checkDirections[i].x >= 0 && coordinate.col + checkDirections[i].x < BoardSize)
            {
                var (isValid, flipCount) = CheckInDirection(coordinate, checkDirections[i]);
                if (isValid)
                {
                    validDirections.Add((checkDirections[i], flipCount));
                }
            }
        }
        //print($"{coordinate}: {validDirections.Count}");

        return validDirections;
    }

    //raycast from <coordinate's world space> in <direction> to check for discs depending on whose turn it is
    (bool isValid, int flipCount) CheckInDirection((int row, int col) coordinate, Vector2Int direction)
    {
        int currentColourValue = playerTurn ? (int)CellType.Black : (int)CellType.White;
        int oppColourValue = playerTurn ? (int)CellType.White : (int)CellType.Black;
        int flipCount = 0;

        int nextRow = coordinate.row + direction.y;
        int nextCol = coordinate.col + direction.x;

        if (nextRow >= 0 && nextRow < BoardSize && nextCol >= 0 && nextCol < BoardSize)
        {
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
        }

        return (false, 0);
    }

    IEnumerator MakeMove((int row, int col) coordinate, List<(Vector2Int direction, int flipCount)> validDirections)
    {
        gameBoard[coordinate.row, coordinate.col].SetActive(true);
        currentGameState.board[coordinate.row, coordinate.col] = gameBoard[coordinate.row, coordinate.col].layer == BlackDiscLayer ? 1 : -1;

        if (playerTurn)
        {
            discCount.black++;
        }
        else
        {
            discCount.white++;
        }

        if (userSettings.soundOn)
        {
            DiscPlaceAction?.Invoke();
        }

        //call FlipInDirection() for all items in validDirections
        for (int i = 0; i < validDirections.Count; i++)
        {
            FlipInDirection(coordinate, validDirections[i].direction, validDirections[i].flipCount);
        }

        //disable input until flip animation finishes
        inputEnabled = false;

        //wait for seconds based on highest flipCount in validDirections
        yield return WaitForSeconds(flipAnimationDuration + FlipAnimationDelay * validDirections.Max(i => i.flipCount));
        inputEnabled = true;

        print("current game board:");
        PrintGameState(currentGameState);
        PassTurn(0);
    }

    //flip all flippable discs in specified direction and update internal board state
    void FlipInDirection((int row, int col) coordinate, Vector2Int direction, int flipLength)
    {
        for (int i = 1; i <= flipLength; i++)
        {
            //set flip axis such that it looks like discs are flipping outward from position of placed disc
            Vector3 directionV3 = new Vector3(direction.x, direction.y);
            Vector3 flipAxis = Vector3.Cross(directionV3, gameBoard[coordinate.row, coordinate.col].layer == BlackDiscLayer ? Vector3.forward : Vector3.back);

            //set flip delay based on flipLength such that it looks like discs are flipping one after another, instead of all at once
            float flipDelay = i * FlipAnimationDelay;

            int row = coordinate.row + (direction.y * i);
            int col = coordinate.col + (direction.x * i);

            gameBoard[row, col].GetComponent<Disc>().FlipUponAxis(flipAxis, flipAnimationDuration, flipDelay);
            currentGameState.board[row, col] = gameBoard[row, col].layer == BlackDiscLayer ? 1 : -1;

            //play disc flip sfx
            DiscFlipAction?.Invoke(flipDelay + flipAnimationDuration);

            //increment/decrement disc counts accordingly (player always plays as black)
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
            validSpaces.Clear();

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (!gameBoard[row, col].activeInHierarchy)
                    {
                        (int, int) coordinate = (row, col);
                        List<(Vector2Int direction, int flipCount)> validDirections = FindValidDirections(coordinate);

                        //if placing a disc at [row, col] makes for valid move, add to validSpaces[], with total number of discs flipped
                        if (validDirections.Count > 0)
                        {
                            int totalFlipCount = validDirections.Sum(i => i.flipCount);
                            validSpaces.Add((coordinate, totalFlipCount));

                            currentGameState.childGameStates.Add(CreateChildGameState(coordinate, validDirections));
                        }
                    }
                }
            }

            //if at least 1 valid move exists...
            if (validSpaces.Count > 0)
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

    //to-do
    GameState CreateChildGameState((int row, int col) coordinate, List<(Vector2Int direction, int flipCount)> validDirections)
    {
        int[,] gameStateCopy = currentGameState.board.Clone() as int[,];

        gameStateCopy[coordinate.row, coordinate.col] = playerTurn ? (int)CellType.Black : (int)CellType.White;

        for (int i = 0; i < validDirections.Count; i++)
        {
            for (int j = 1; j <= validDirections[i].flipCount; j++)
            {
                (int r, int c) = (coordinate.row + (j * validDirections[i].direction.y), coordinate.col + (j * validDirections[i].direction.x));
                gameStateCopy[r, c] = playerTurn ? (int)CellType.Black : (int)CellType.White;
            }
        }

        print("new future game state found.");

        GameState childGameState = new GameState(gameStateCopy);
        PrintGameState(childGameState);

        return childGameState;
    }

    IEnumerator RunCPU()
    {
        //order validSpaces[] by whether or not item exists in corners[], then by whether or not item exists in edges[], then by item's total number of discs to flip
        validSpaces = validSpaces.OrderBy(i => corners.Contains(i.coordinate))
                          .ThenBy(i => edges.Contains(i.coordinate))
                          .ThenBy(i => i.totalFlipCount).ToList();

        validSpaces.ForEach(v => print(v));

        (int row, int col) selectedCoordinate = FindCPUMove();

        //more possible valid spaces = longer wait time (for added realism)
        float cpuDelay = 0.5f + (validSpaces.Count / 8f);
        yield return WaitForSeconds(cpuDelay);

        StartCoroutine(MakeMove(selectedCoordinate, FindValidDirections(selectedCoordinate)));
    }

    //get coordinates of CPU's next move, based on <cpuDifficulty>
    (int row, int col) FindCPUMove()
    {
        int cpuDifficulty = (int)userSettings.cpuDifficulty;
        List<float> moveSelectionWeights = new List<float>(validSpaces.Count);

        for (int i = 0; i < validSpaces.Count; i++)
        {
            moveSelectionWeights.Add(cpuDifficultyCurves[cpuDifficulty].Evaluate((i + 1f) / (validSpaces.Count + 1f)));
        }

        return validSpaces[GetRandomWeightedIndex(moveSelectionWeights)].coordinate;
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
        if (validSpaces.Any())
        {
            //display hint disc for each valid space
            validSpaces.ForEach(i =>
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

    //get row-column coordinates from [0, BoardSize - 1] based on (hit.point.xy / hit.collider.bounds.extents.xy)
    //bottom-left: (0, 0); top-right: (BoardSize - 1, BoardSize - 1)
    (int row, int col) InverseTransformPoint(RaycastHit hit)
    {
        int row = Mathf.FloorToInt(hit.point.y / hit.collider.bounds.extents.y * (BoardSize / 2) + (BoardSize / 2));
        int col = Mathf.FloorToInt(hit.point.x / hit.collider.bounds.extents.x * (BoardSize / 2) + (BoardSize / 2));
        return (row, col);
    }

    //temp
    void PrintGameState(GameState gameState)
    {
        //System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.SceneView)).GetType("UnityEditor.LogEntries").GetMethod("Clear").Invoke(new object(), null);
        for (int i = currentGameState.board.GetLength(0) - 1; i >= 0; i--)
        {
            string output = "";

            for (int j = 0; j < gameState.board.GetLength(1); j++)
            {
                if (gameState.board[i, j] == 0)
                {
                    output += "□, ";
                }
                else if (gameState.board[i, j] == 1)
                {
                    output += "●, ";
                }
                else if (gameState.board[i, j] == -1)
                {
                    output += "○, ";
                }
            }

            print(output);
        }
    }
}

public class GameState
{
    public readonly int[,] board = new int[8, 8];
    public List<GameState> childGameStates = new List<GameState>();

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
    }

    public GameState(int[,] _board)
    {
        board = _board;
    }
}