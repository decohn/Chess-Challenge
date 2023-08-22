using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    private int maxDepth = 4;

    public Move Think(Board board, Timer timer)
    {
        // Get all legal moves. Select the move with the best score for the
        // current player:
        Move bestMove = new Move();
        double bestEval = -999;

        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            double moveEval = -NegaMax(board, 1);
            board.UndoMove(move);

            if (moveEval > bestEval)
            {
                bestMove = move;
                bestEval = moveEval;
            }
        }

        return bestMove;
    }

    public double NegaMax(Board b, int depth)
    {
        // If we've reached the maximum search depth, return the heuristic
        // evaluation of the position.
        if (depth == maxDepth)
        {
            return Evaluate(b);
        }
        else
        {
            double bestEval = -999;

            foreach (Move move in b.GetLegalMoves())
            {
                b.MakeMove(move);
                bestEval = Math.Max(bestEval, -NegaMax(b, depth + 1));
                b.UndoMove(move);
            }

            return bestEval;
        }
    }

    public double Evaluate(Board b)
    {
        // Return a value in pawns from the perspective of the player to move
        if (b.IsInCheckmate())
        {
            return -500;
        }

        if (b.IsInStalemate() || b.IsInsufficientMaterial() || b.IsFiftyMoveDraw())
        {
            return 0;
        }

        double evaluation = 0;       
        PieceList[] pieceLists = b.GetAllPieceLists();

        // Compute a material score using Hans Berliner's system, from White's
        // perspective. Order: {P, N, B, R, Q, K}
        double[] pieceValues = {1, 3.2, 3.33, 5.1, 8.8, 0};

        for (int i = 0; i < 12; i++)
        {
            evaluation += pieceValues[i % 6] * (i < 6 ? 1 : -1) * pieceLists[i].Count;
        }

        // Convert the score from White's perspective to that of the player to move
        return evaluation * (b.IsWhiteToMove ? 1 : -1);
    }
}