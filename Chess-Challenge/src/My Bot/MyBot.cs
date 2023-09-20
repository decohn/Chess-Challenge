using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    private int MATE = 99999;
    private int MAX_DEPTH = 3;
    // Order: {P, N, B, R, Q, K}. Hans Berliner's system.
    private int[] PIECE_VALUES = {100, 320, 333, 510, 880, 0};

    public Move Think(Board board, Timer timer)
    {
        // Print current position for debugging
        Console.WriteLine(board.CreateDiagram(true, false, false));

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
        int bestEval = -MATE - 100;
        foreach (Move move in legalMoves)
        {
            board.MakeMove(move);
            int moveEval = -SearchEvaluate(board, -MATE, MATE, MAX_DEPTH);
            board.UndoMove(move);

            if (moveEval > bestEval)
            {
                bestMove = move;
                bestEval = moveEval;
            }

            // Print relevant information for debugging:
            Console.WriteLine(move + "(" + (moveEval / 100) + ")");
            Console.WriteLine("Elapsed time: " + timer.MillisecondsElapsedThisTurn);
        }

        Console.WriteLine("Depth: " + MAX_DEPTH);
        Console.WriteLine("Elapsed time: " + timer.MillisecondsElapsedThisTurn);
        Console.WriteLine("Best " + bestMove + "(" + (bestEval / 100) + ")");
        return bestMove;
    }

    public int HeuristicMovePriority(Move move, Board b) {
        // Prioritize captures and moving pieces that are attacked. Deprioritize
        // moving pieces to squares that are attacked. Lower number -> higher
        // priority
        int priority = move.IsPromotion ? -880 : 0;
        if (move.IsCapture)
        {
            priority -= PIECE_VALUES[(int)move.CapturePieceType - 1]; 
        }
        if (b.SquareIsAttackedByOpponent(move.TargetSquare))
        {
            priority += PIECE_VALUES[(int)move.MovePieceType - 1];
        }
        if (b.SquareIsAttackedByOpponent(move.StartSquare))
        {
            priority -= PIECE_VALUES[(int)move.MovePieceType - 1];
        }

        return priority;
    }

    public int SearchEvaluate(Board b, int alpha, int beta, int depth)
    {
        // If we've reached the maximum search depth, or if there are no legal
        // moves, return the heuristic evaluation of the position.
        if (depth == 0)
        {
            return HeuristicEvaluate(b, 0);
        }

        Move[] legalMoves = b.GetLegalMoves();

        if (legalMoves.Length == 0)
        {
            return HeuristicEvaluate(b, depth);
        }

        foreach (Move move in legalMoves)
        {
            b.MakeMove(move);
            int score = -SearchEvaluate(b, -beta, -alpha, depth - 1);
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

    public int HeuristicEvaluate(Board b, int depthLeft)
    {
        // Return a value in pawns from the perspective of the player to move
        if (b.IsInCheckmate())
        {
            return -MATE + (100 * depthLeft);
        }

        if (b.IsInStalemate() || b.IsInsufficientMaterial() || b.IsFiftyMoveDraw())
        {
            return 0;
        }

        int evaluation = 0;       
        PieceList[] pieceLists = b.GetAllPieceLists();

        // Compute a material score from White's perspective.
        for (int i = 0; i < 5; i++)
        {
            evaluation += PIECE_VALUES[i] * (pieceLists[i].Count - pieceLists[i + 6].Count);
        }
    
        // Convert the score from White's perspective to that of the player to move
        return evaluation * (b.IsWhiteToMove ? 1 : -1);
    }
}