using System.Collections.Generic;
using UnityEngine;

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

        GetValidMoves(this, IsPlayerTurn);
    }

    public GameState(int[,] board)
    {
        this.board = board;
    }

    public List<(Vector2Int, int)> GetFlipDirections(GameState state, bool playerTurn, (int row, int col) coordinate)
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
                while (state.board[nextRow, nextCol] == oppColourValue)
                {
                    flipCount++;

                    //keep incrementing check coordinate by check direction
                    nextRow += checkDirections[i].y;
                    nextCol += checkDirections[i].x;

                    //check to see if there is eventually another disc of the same colour
                    if (nextRow >= 0 && nextRow < BoardSize && nextCol >= 0 && nextCol < BoardSize)
                    {
                        //if there is another disc of the same colour ("sandwich" confirmed), add to flipDirections
                        if (state.board[nextRow, nextCol] == currentColourValue)
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

    public void ApplyMove((int row, int col) coordinate)
    {
        if (board[coordinate.row, coordinate.col] != (int)CellType.Empty) return;

        board[coordinate.row, coordinate.col] = IsPlayerTurn ? (int)CellType.Black : (int)CellType.White;

        List<(Vector2Int direction, int flipCount)> flipDirections = GetFlipDirections(this, IsPlayerTurn, coordinate);

        for (int i = 0; i < flipDirections.Count; i++)
        {
            Vector2Int flipDirection = flipDirections[i].direction;

            for (int j = 1; j <= flipDirections[i].flipCount; j++)
            {
                int nextRow = coordinate.row + (flipDirection.y * j);
                int nextCol = coordinate.col + (flipDirection.x * j);

                board[nextRow, nextCol] = IsPlayerTurn ? (int)CellType.Black : (int)CellType.White;
            }
        }
    }

    GameState TryMove(GameState state, bool playerTurn, (int row, int col) coordinate, List<(Vector2Int direction, int flipCount)> flipDirections)
    {
        if (state.board[coordinate.row, coordinate.col] != (int)CellType.Empty) return this;

        int[,] boardCopy = state.board.Clone() as int[,];

        boardCopy[coordinate.row, coordinate.col] = playerTurn ? (int)CellType.Black : (int)CellType.White;

        for (int i = 0; i < flipDirections.Count; i++)
        {
            Vector2Int flipDirection = flipDirections[i].direction;

            for (int j = 1; j <= flipDirections[i].flipCount; j++)
            {
                int nextRow = coordinate.row + (flipDirection.y * j);
                int nextCol = coordinate.col + (flipDirection.x * j);

                boardCopy[nextRow, nextCol] = boardCopy[nextRow, nextCol] == (int)CellType.Black ? (int)CellType.White : (int)CellType.Black;
            }
        }

        return new GameState(boardCopy);
    }

    public void GetValidMoves(GameState state, bool playerTurn)
    {
        state.validMoves.Clear();

        for (int row = 0; row < state.board.GetLength(0); row++)
        {
            for (int col = 0; col < state.board.GetLength(1); col++)
            {
                //for all empty cells, check if placing a disc that cell makes for a valid move
                if (state.board[row, col] == 0)
                {
                    List<(Vector2Int direction, int flipCount)> flipDirections = GetFlipDirections(state, playerTurn, (row, col));

                    if (flipDirections.Count > 0)
                    {
                        var boardCopy = TryMove(state, playerTurn, (row, col), flipDirections);

                        state.validMoves.Add(((row, col), state.Evaluation));

                        print("new future game state found.");
                        PrintGameState(boardCopy);
                    }
                }
            }
        }
    }

    public void GetCPUMove(int difficulty)
    {
        var bestMove = FindBestMove(difficulty);
    }

    ((int, int), int) FindBestMove(int difficulty)
    {
        int minEvaluation = int.MaxValue;
        (int row, int col) bestMove = validMoves[0].coordinate;

        int evaluation = Minimax(this, 2, Mathf.NegativeInfinity, Mathf.Infinity, false);
        print($"static evaluation is {evaluation}");

        return (bestMove, minEvaluation);
    }

    public int Minimax(GameState state, int depth, float alpha, float beta, bool maximizingPlayer)
    {
        if (depth == 0 || IsGameOver)
        {
            return state.Evaluation;
        }

        if (state.validMoves.Count == 0)
        {
            GetValidMoves(state, maximizingPlayer);
        }

        if (maximizingPlayer)
        {
            int maxEvaluation = int.MinValue;

            foreach (var move in state.validMoves)
            {
                var flipDirections = GetFlipDirections(state, !IsPlayerTurn, move.coordinate);
                var newState = TryMove(state, !IsPlayerTurn, move.coordinate, flipDirections);

                int evaluation = Minimax(newState, depth - 1, alpha, beta, false);

                maxEvaluation = Mathf.Max(maxEvaluation, evaluation);
                alpha = Mathf.Max(alpha, evaluation);

                if (beta <= alpha) break;
            }

            return maxEvaluation;
        }
        else
        {
            int minEvaluation = int.MaxValue;
            foreach (var move in state.validMoves)
            {
                var flipDirections = GetFlipDirections(state, IsPlayerTurn, move.coordinate);
                var newState = TryMove(state, IsPlayerTurn, move.coordinate, flipDirections);

                int evaluation = Minimax(newState, depth - 1, alpha, beta, true);

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
    void PrintGameState(GameState state)
    {
        print("__________________");
        for (int i = state.board.GetLength(0) - 1; i >= 0; i--)
        {
            string output = "";

            for (int j = 0; j < state.board.GetLength(1); j++)
            {
                if (state.board[i, j] == 0)
                {
                    output += "_| ";
                }
                else if (state.board[i, j] == 1)
                {
                    output += "●| ";
                }
                else if (state.board[i, j] == -1)
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