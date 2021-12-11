using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Globals;
using static CoroutineHelper;

public class GameController : MonoBehaviour
{
    Camera mainCam;
    [SerializeField] AudioController audioController;
    [SerializeField] Transform discParent;
    [SerializeField] GameObject hintDisc;

    [Space]

    [Tooltip("Weighted chances that the CPU will make a certain move.\nLeft = worse move; Right = better move\nTop = higher chance; Bottom = lower chance")]
    [SerializeField] AnimationCurve[] cpuDifficultyCurves = new AnimationCurve[MaxCPUDifficulty + 1];

    const int BoardSize = 8;
    readonly GameObject[,] gameBoard = new GameObject[BoardSize, BoardSize];

    Vector3Int[] checkDirections = new Vector3Int[8]
    {
        new Vector3Int( 0, 1, 0),
        new Vector3Int( 1, 1, 0),
        new Vector3Int( 1, 0, 0),
        new Vector3Int( 1,-1, 0),
        new Vector3Int( 0,-1, 0),
        new Vector3Int(-1,-1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(-1, 1, 0)
    };

    //list to hold (direction in which >0 discs would be flipped, number of discs to flip)
    List<(Vector3Int direction, int flipCount)> validDirections = new List<(Vector3Int, int)>(8);

    //list to hold (row-column coordinate of board if a disc placed there would result in a valid move, total number of discs that would flip if a disc was placed there)
    List<((int row, int col) coordinate, int totalFlipCount)> validSpaces = new List<((int, int), int)>();

    //used for CPU to prioritize placing discs on these coordinates on harder difficulty settings
    List<(int, int)> corners = new List<(int, int)>(4);
    List<(int, int)> edges = new List<(int, int)>((BoardSize - 1) * 4);

    public event System.Action DiscPlaceAction;
    public event System.Action<float> DiscFlipAction;
    public event System.Action ScoreUpdateAction;
    public event System.Action GameOverAction;

    void Awake()
    {
        mainCam = Camera.main;
        GameOverAction += OnGameOver;

        InitGameBoard();
    }

    void InitGameBoard()
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                //set elements in gameBoard
                gameBoard[row, col] = discParent.GetChild(row * BoardSize + col).gameObject;

                //set edge and corner coordinates
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
        ResetGameState();
    }

    void ResetGameState()
    {
        inputEnabled = true;
        playerTurn = true;
        whiteDiscCount = 2;
        blackDiscCount = 2;

        validSpaces.Add(((2, 4), 1));
        validSpaces.Add(((3, 5), 1));
        validSpaces.Add(((4, 2), 1));
        validSpaces.Add(((5, 3), 1));
    }

    void Update()
    {
        if (inputEnabled && playerTurn)
        {
            GetMouseInput();

            if (hintsEnabled)
            {
                UpdateHints();
            }
        }
    }

    void GetMouseInput()
    {
        //0 = L. mouse button released
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

            //if raycast hit game board...
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetType() == typeof(MeshCollider))
            {
                (int row, int col) selectedCoordinate = InverseTransformPoint(hit);

                //if disc at selected coordinate is inactive, and if selectedCoordinate exists in validSpaces[], make move
                if (!gameBoard[selectedCoordinate.row, selectedCoordinate.col].activeInHierarchy && validSpaces.Any(item => item.coordinate == selectedCoordinate))
                {
                    FindValidDirections(selectedCoordinate);
                    StartCoroutine(MakeMove(selectedCoordinate));
                }
            }
        }
    }

    void FindValidDirections((int row, int col) coordinate)
    {
        validDirections.Clear();

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
    }

    //raycast from <coordinate's world space> in <direction> to check for discs depending on whose turn it is
    (bool isValid, int flipCount) CheckInDirection((int row, int col) coordinate, Vector3 direction)
    {
        Vector3 rayOrigin = gameBoard[coordinate.row, coordinate.col].transform.position;
        float rayDistance = direction.normalized.magnitude;
        int currentColourLayer = playerTurn ? blackDiscLayer : whiteDiscLayer;
        int flipCount = 0;

        //continuously raycast to check for a disc of opposite colour
        while (Physics.Raycast(rayOrigin, direction, out RaycastHit oppositeColourDisc, rayDistance, ~(1 << currentColourLayer)))
        {
            //update rayOrigin to transform.position of disc that was hit
            rayOrigin = oppositeColourDisc.transform.position;
            flipCount++;

            //if raycast hits another disc...
            if (Physics.Raycast(rayOrigin, direction, out RaycastHit sameColourDisc, rayDistance))
            {
                //check what colour it is; if same colour, 'sandwich' confirmed, return (true, <how many times this loop iterated>); otherwise continue checking
                if (sameColourDisc.transform.gameObject.layer == currentColourLayer)
                {
                    return (true, flipCount);
                }
            }
            //otherwise break and return false (no disc of same colour to 'close the sandwich')
            else
            {
                break;
            }
        }

        return (false, 0);
    }

    IEnumerator MakeMove((int row, int col) coordinate)
    {
        gameBoard[coordinate.row, coordinate.col].SetActive(true);

        if (soundEnabled)
        {
            DiscPlaceAction?.Invoke();
        }

        //call FlipInDirection() for all items in validDirections[]
        for (int i = 0; i < validDirections.Count; i++)
        {
            FlipInDirection(coordinate, validDirections[i].direction, validDirections[i].flipCount);
        }

        //disable input until flip animation finishes
        inputEnabled = false;

        //wait for seconds based on highest flipCount in validDirections
        yield return WaitForSeconds(FlipAnimationDuration + FlipAnimationDelay * validDirections.Max(i => i.flipCount));
        inputEnabled = true;

        int turnsPassed = 0;
        PassTurn(ref turnsPassed);
    }

    //start Disc.FlipUponAxis() coroutine for all discs that should be flipped
    void FlipInDirection((int row, int col) coordinate, Vector3Int direction, int flipLength)
    {
        for (int i = 1; i <= flipLength; i++)
        {
            //set flip axis such that it looks like discs are flipping outward from position of placed disc
            Vector3 flipAxis = Vector3.Cross(direction, gameBoard[coordinate.row, coordinate.col].layer == blackDiscLayer ? Vector3.forward : Vector3.back);

            //set flip delay based on flipLength such that it looks like discs are flipping one after another, instead of all at once
            float flipDelay = i * FlipAnimationDelay;

            gameBoard[coordinate.row + (direction.y * i), coordinate.col + (direction.x * i)].GetComponent<Disc>().FlipUponAxis(flipAxis, flipDelay);

            //play disc flip sfx
            DiscFlipAction?.Invoke(flipDelay + FlipAnimationDuration);

            //increment/decrement disc counts accordingly
            //(player always plays as black)
            if (playerTurn)
            {
                blackDiscCount++;
                whiteDiscCount--;
            }
            else
            {
                blackDiscCount--;
                whiteDiscCount++;
            }
        }
    }

    //handle events that happen between turns
    void PassTurn(ref int _turnsPassed)
    {
        int turnsPassed = _turnsPassed;

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

            //pass the turn over; look for validSpaces given <playerTurn> in new board state
            playerTurn = !playerTurn;
            FindValidSpaces(playerTurn);

            //if at least 1 valid move exists...
            if (validSpaces.Count > 0)
            {
                turnsPassed = 0;

                if (!playerTurn)
                {
                    hintDisc.SetActive(false);
                    StartCoroutine(RunCPU());
                }
            }
            //if a valid move doesn't exist, increment turnsPassed counter; call this function again
            else
            {
                turnsPassed++;
                PassTurn(ref turnsPassed);
            }
        }
        //game is over when turn has been passed twice without a move being made
        //(checking if board is full is not good enough because gameover states exist where the board isn't completely filled and neither player can make a move)
        else
        {
            GameOverAction?.Invoke();
        }

        ScoreUpdateAction?.Invoke();
    }

    //check all coordinates of inactive discs to see if a move can be made there, given whose turn it is
    //(true = player's turn = black; false = CPU's turn = white)
    void FindValidSpaces(bool playerTurn)
    {
        validSpaces.Clear();

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                if (!gameBoard[row, col].activeInHierarchy)
                {
                    FindValidDirections((row, col));

                    //if placing a disc at [row, col] makes for valid move, add to validSpaces[], with total number of discs flipped
                    if (validDirections.Count > 0)
                    {
                        int totalFlipCount = validDirections.Sum(i => i.flipCount);
                        validSpaces.Add(((row, col), totalFlipCount));
                    }
                }
            }
        }
    }

    IEnumerator RunCPU()
    {
        //order validSpaces[] by whether or not item exists in corners[], then by whether or not item exists in edges[], then by item's total number of discs to flip
        validSpaces = validSpaces.OrderBy(i => corners.Contains(i.coordinate))
                          .ThenBy(i => edges.Contains(i.coordinate))
                          .ThenBy(i => i.totalFlipCount).ToList();

        (int row, int col) selectedCoordinate = FindCPUMove();

        //more possible valid spaces = longer wait time (to make it seem a bit more R E A L I S T I C)
        float cpuDelay = validSpaces.Count / 8f + 1;
        yield return WaitForSeconds(cpuDelay);

        FindValidDirections(selectedCoordinate);
        StartCoroutine(MakeMove(selectedCoordinate));
    }

    //get coordinates of CPU's next move, based on <cpuDifficulty>
    (int row, int col) FindCPUMove()
    {
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

    void UpdateHints()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetType() == typeof(MeshCollider))
        {
            //get board coordinate at where player moused over
            (int row, int col) coordinate = InverseTransformPoint(hit);

            //if placing a disc at coordinate would result in a valid move
            if (validSpaces.Any(i => i.coordinate == coordinate))
            {
                //show hintDisc, and position it at coordinate's transform.position
                hintDisc.transform.localPosition = gameBoard[coordinate.row, coordinate.col].transform.position;
                hintDisc.SetActive(true);
            }
            //otherwise hide hintDisc
            else
            {
                hintDisc.SetActive(false);
            }
        }
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
}