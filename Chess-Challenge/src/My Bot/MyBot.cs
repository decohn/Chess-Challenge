using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    private int MATE = 999;
    private int MAX_DEPTH = 3;

    public Move Think(Board board, Timer timer)
    {
        // Get all legal moves. Use a heuristic to prioritize certain ones.
        Move[] legalMoves = board.GetLegalMoves();
        int numLegalMoves = legalMoves.Length;
        int[] movePriorities = new int[numLegalMoves];

        for (int i = 0; i < numLegalMoves; i++)
        {
            movePriorities[i] = HeuristicMovePriority(legalMoves[i], board);
        }

        Array.Sort(movePriorities, legalMoves);

        // Select the move with the best score for the current player:
        Move bestMove = new Move();
        double bestEval = -MATE - 1;
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            double moveEval = -SearchEvaluate(board, -MATE, MATE, MAX_DEPTH);
            board.UndoMove(move);

            if (moveEval > bestEval)
            {
                bestMove = move;
                bestEval = moveEval;
            }
        }

        return bestMove;
    }

    public int HeuristicMovePriority(Move move, Board b) {
        // Prioritize captures and moving pieces that are attacked. Deprioritize
        // moving pieces to squares that are attacked. Lower number -> higher
        // priority
        int priority = move.IsPromotion ? -6 : 0;
        if (move.IsCapture)
        {
            priority -= (int)move.CapturePieceType; 
        }
        if (b.SquareIsAttackedByOpponent(move.TargetSquare))
        {
            priority += (int)move.MovePieceType;
        }
        if (b.SquareIsAttackedByOpponent(move.StartSquare))
        {
            priority -= (int)move.MovePieceType;
        }

        return priority;
    }

    public double SearchEvaluate(Board b, double alpha, double beta, int depth)
    {
        // If we've reached the maximum search depth, or if there are no legal
        // moves, return the heuristic evaluation of the position.
        if (depth == 0)
        {
            return HeuristicEvaluate(b, 0);
        }
        else
        {
            Move[] legalMoves = b.GetLegalMoves();

            if (legalMoves.Length == 0)
            {
                return HeuristicEvaluate(b, depth);
            }

            foreach (Move move in legalMoves)
            {
                b.MakeMove(move);
                double score = -SearchEvaluate(b, -beta, -alpha, depth - 1);
                b.UndoMove(move);

                if (score >= beta)
                {
                    return beta;
                }

                if (score > alpha)
                {
                    alpha = score;
                }
            }

            return alpha;
        }
    }

    public double HeuristicEvaluate(Board b, int depthLeft)
    {
        // Return a value in pawns from the perspective of the player to move
        if (b.IsInCheckmate())
        {
            return -MATE + depthLeft;
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