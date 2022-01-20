using UnityEngine;

public static class CPUController
{
    public static float Minimax(GameState currentGameState, int depth, float alpha, float beta, bool maximizingPlayer)
    {
        if (depth == 0)
        {
            return currentGameState.Evaluation;
        }

        if (maximizingPlayer)
        {
            float maxEvaluation = Mathf.NegativeInfinity;

            foreach (var child in currentGameState.childGameStates)
            {
                float evaluation = Minimax(child, depth - 1, alpha, beta, false);

                maxEvaluation = Mathf.Max(maxEvaluation, evaluation);
                alpha = Mathf.Max(alpha, evaluation);

                if (beta <= alpha) break;
            }

            return maxEvaluation;
        }
        else
        {
            float minEvaluation = Mathf.Infinity;

            foreach (var child in currentGameState.childGameStates)
            {
                float evaluation = Minimax(child, depth - 1, alpha, beta, true);

                minEvaluation = Mathf.Min(minEvaluation, evaluation);
                beta = Mathf.Min(beta, evaluation);

                if (beta <= alpha) break;
            }

            return minEvaluation;
        }
    }
}