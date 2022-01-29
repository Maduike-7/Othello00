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

    [Space]

    [SerializeField] Transform discParent;
    [SerializeField] GameObject hintDiscParent;
    Camera mainCam;

    #endregion

    #region Game properties

    public GameState CurrentGameState;

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

    //get game board elements
    void InitGame()
    {
        CurrentGameState = new GameState();

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
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
                    HideHints();
                    StartCoroutine(DisplayMove(selectedCoordinate));
                    CurrentGameState.ApplyMove(selectedCoordinate);
                }
            }
        }
    }

    //show the move happening on the game board
    IEnumerator DisplayMove((int row, int col) coordinate)
    {
        //disable input until flip animation finishes
        inputEnabled = false;

        gameBoard[coordinate.row, coordinate.col].SetActive(true);

        if (userSettings.soundOn)
        {
            DiscPlaceAction?.Invoke();
        }

        List<(Vector2Int direction, int flipCount)> flipDirections = CurrentGameState.GetFlipDirections(CurrentGameState, CurrentGameState.IsPlayerTurn, coordinate);

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
        //print("current game board:");
        //PrintGameState(CurrentGameState);

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

            CurrentGameState.GetValidMoves(CurrentGameState, CurrentGameState.IsPlayerTurn);

            if (CurrentGameState.validMoves.Count > 0)
            {
                if (CurrentGameState.IsPlayerTurn)
                {
                    ShowHints();
                }
                else
                {
                    StartCoroutine(RunCPU());
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

    IEnumerator RunCPU()
    {
        (int, int) selectedCoordinate = CurrentGameState.GetCPUMove((int)userSettings.cpuDifficulty);

        float delay = 0.5f + (CurrentGameState.validMoves.Count / 8f);
        yield return WaitForSeconds(delay);

        StartCoroutine(DisplayMove(selectedCoordinate));
        CurrentGameState.ApplyMove(selectedCoordinate);
    }

    void ShowHints()
    {
        if (!userSettings.hintsOn) { return; }

        HideHints();

        //display hint disc at location of each valid move
        if (CurrentGameState.validMoves.Any())
        {
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