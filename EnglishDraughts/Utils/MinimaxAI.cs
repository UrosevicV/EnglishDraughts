using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace EnglishDraughts.Utils
{
    public class MinimaxAi
    {
        private const int NumberOfFields = 8;
        private Move bestMoveSoFar = null;
        private const string Draught = "D";

        public async Task<Move> FindBestMoveWithTimeoutAsync(int[,] board, Button[,] buttons, int player, int depth, int milliseconds)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(milliseconds);

                try
                {
                    await Task.Run(() => { FindBestMoveInternal(board, buttons, player, depth, cts.Token); },
                        cts.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }

            return bestMoveSoFar;
        }

        private void FindBestMoveInternal(int[,] board, Button[,] buttons, int player, int depth, CancellationToken token)
        {
            object lockObj = new object();
            int bestScore = int.MinValue;
            bestMoveSoFar = null;

            var moves = GenerateAllMoves(board, buttons, player);

            Parallel.ForEach(moves, new ParallelOptions { CancellationToken = token }, move =>
            {
                if (token.IsCancellationRequested) return;

                int[,] simulated = MakeMove(CloneBoard(board), move);
                int score = Minimax(simulated, buttons, depth - 1, false, GetOpponent(player), token);

                lock (lockObj)
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMoveSoFar = move;
                    }
                }
            });
        }

        // Implementation of Minimax algorithm
        private int Minimax(int[,] board, Button[,] buttons, int depth, bool isMaximizingPlayer, int currentPlayer, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return int.MinValue;

            if (depth == 0 || IsGameOver(board, buttons))
                return Evaluate(board, buttons, currentPlayer);

            var moves = GenerateAllMoves(board, buttons, currentPlayer);

            // Max branch - finding the best ending
            if (isMaximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in moves)
                {
                    if (token.IsCancellationRequested)
                        return int.MinValue;

                    int[,] simulated = MakeMove(CloneBoard(board), move);
                    int eval = Minimax(simulated, buttons, depth - 1, false, GetOpponent(currentPlayer), token);
                    maxEval = Math.Max(maxEval, eval);
                }
                return maxEval;
            }
            // Min branch - finding the worst ending
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in moves)
                {
                    if (token.IsCancellationRequested)
                        return int.MinValue;

                    int[,] simulated = MakeMove(CloneBoard(board), move);
                    int eval = Minimax(simulated, buttons, depth - 1, true, GetOpponent(currentPlayer), token);
                    minEval = Math.Min(minEval, eval);
                }
                return minEval;
            }
        }

        // Make move on copy of the board
        public int[,] MakeMove(int[,] board, Move move)
        {
            int[,] newBoard = CloneBoard(board);
            newBoard[move.ToI, move.ToJ] = newBoard[move.FromI, move.FromJ];
            newBoard[move.FromI, move.FromJ] = 0;

            if (move.IsJump)
            {
                int midI = (move.FromI + move.ToI) / 2;
                int midJ = (move.FromJ + move.ToJ) / 2;
                newBoard[midI, midJ] = 0;
            }

            return newBoard;
        }

        // Calculate the score
        public int Evaluate(int[,] board, Button[,] buttons, int player)
        {
            int score = 0;
            int opponent = GetOpponent(player);

            for (int i = 0; i < NumberOfFields; i++)
            {
                for (int j = 0; j < NumberOfFields; j++)
                {
                    int piece = board[i, j];
                    if (piece == player)
                        score += buttons[i, j].Text == Draught ? 3 : 1;
                    else if (piece == opponent)
                        score -= buttons[i, j].Text == Draught ? 3 : 1;
                }
            }

            return score;
        }

        // Make copy of the board
        public int[,] CloneBoard(int[,] board)
        {
            int[,] newBoard = new int[NumberOfFields, NumberOfFields];
            Array.Copy(board, newBoard, board.Length);
            return newBoard;
        }

        private int GetOpponent(int player) => player == 1 ? 2 : 1;
        private bool IsInside(int i, int j) => i >= 0 && i < NumberOfFields && j >= 0 && j < NumberOfFields;
        private bool IsDarkSquare(int i, int j) => (i + j) % 2 != 0;

        // Check if it is the end of the game
        private bool IsGameOver(int[,] board, Button[,] buttons)
        {
            bool player1HasFigures = false;
            bool player2HasFigures = false;

            for (int i = 0; i < NumberOfFields; i++)
            {
                for (int j = 0; j < NumberOfFields; j++)
                {
                    if (board[i, j] == 1)
                        player1HasFigures = true;
                    else if (board[i, j] == 2)
                        player2HasFigures = true;
                }
            }

            if (!player1HasFigures || !player2HasFigures || PlayerHasLegalMoves(board, buttons, 1) || PlayerHasLegalMoves(board, buttons, 2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Check if there is any legal move for the player
        public bool PlayerHasLegalMoves(int[,] board, Button[,] buttons, int player)
        {
            for (int i = 0; i < NumberOfFields; i++)
            {
                for (int j = 0; j < NumberOfFields; j++)
                {
                    if (board[i, j] == player)
                    {
                        bool isDraught = buttons[i, j].Text == Draught;

                        // All 4 diagonals
                        int[] dirX = { -1, -1, 1, 1 };
                        int[] dirY = { -1, 1, -1, 1 };

                        for (int d = 0; d < 4; d++)
                        {
                            int ni = i + dirX[d];
                            int nj = j + dirY[d];

                            if (ni >= 0 && ni < NumberOfFields && nj >= 0 && nj < NumberOfFields)
                            {
                                if (board[ni, nj] == 0)
                                {
                                    // Simple figures
                                    if (!isDraught)
                                    {
                                        if ((player == 1 && dirX[d] < 0) || (player == 2 && dirX[d] > 0))
                                            continue;
                                    }
                                    return true;
                                }

                                // Jump check
                                int jumpI = i + 2 * dirX[d];
                                int jumpJ = j + 2 * dirY[d];

                                if (jumpI >= 0 && jumpI < NumberOfFields && jumpJ >= 0 && jumpJ < NumberOfFields)
                                {
                                    if (board[ni, nj] != 0 && board[ni, nj] != player && board[jumpI, jumpJ] == 0)
                                        return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        // Get list of all legal moves
        public List<Move> GenerateAllMoves(int[,] board, Button[,] buttons, int player)
        {
            var moves = new List<Move>();
            int direction = player == 1 ? 1 : -1;

            // First check eating
            for (int i = 0; i < NumberOfFields; i++)
            {
                for (int j = 0; j < NumberOfFields; j++)
                {
                    if (board[i, j] == player)
                    {
                        bool isDraught = buttons[i, j].Text == Draught;
                        var jumpChain = GetNormalMultiJumps(board, i, j, player, isDraught);
                        if (jumpChain.Count > 0)
                            moves.AddRange(jumpChain);
                    }
                }
            }

            // Eating steps must be played
            if (moves.Count > 0)
                return moves;

            // If there is no eating moves, add simple
            for (int i = 0; i < NumberOfFields; i++)
            {
                for (int j = 0; j < NumberOfFields; j++)
                {
                    if (board[i, j] == player)
                    {
                        bool isDraught = buttons[i, j].Text == Draught;
                        foreach (var (di, dj) in isDraught
                                     ? new (int, int)[] { (-1, -1), (-1, 1), (1, -1), (1, 1) }
                                     : new (int, int)[] { (direction, -1), (direction, 1) })
                        {
                            int ni = i + di;
                            int nj = j + dj;

                            if (IsInside(ni, nj) && board[ni, nj] == 0 && IsDarkSquare(ni, nj))
                            {
                                moves.Add(new Move(i, j, ni, nj, false));
                            }
                        }
                    }
                }
            }

            return moves;
        }

        // Find jump moves
        public List<Move> GetNormalMultiJumps(int[,] board, int i, int j, int player, bool allowBackward = false)
        {
            var result = new List<Move>();
            ExploreJumps(board, i, j, player, new List<Move>(), result, new bool[NumberOfFields, NumberOfFields], allowBackward);
            return result;
        }

        // Find jump moves
        private void ExploreJumps(int[,] board, int i, int j, int player, List<Move> path, List<Move> result,
            bool[,] visited, bool allowBackward)
        {
            bool extended = false;
            int direction = player == 1 ? 1 : -1;
            int opponent = GetOpponent(player);

            var directions = allowBackward
                ? new (int, int)[] { (-1, -1), (-1, 1), (1, -1), (1, 1) }
                : new (int, int)[] { (direction, -1), (direction, 1) };

            foreach (var (di, dj) in directions)
            {
                int mi = i + di;
                int mj = j + dj;
                int li = i + 2 * di;
                int lj = j + 2 * dj;

                if (IsInside(mi, mj) && IsInside(li, lj) &&
                    board[mi, mj] == opponent && board[li, lj] == 0 && !visited[mi, mj])
                {
                    var newBoard = CloneBoard(board);
                    newBoard[i, j] = 0;
                    newBoard[mi, mj] = 0;
                    newBoard[li, lj] = player;

                    var newPath = new List<Move>(path) { new Move(i, j, li, lj, true) };
                    var newVisited = (bool[,])visited.Clone();
                    newVisited[mi, mj] = true;

                    ExploreJumps(newBoard, li, lj, player, newPath, result, newVisited, allowBackward);
                    extended = true;
                }
            }

            if (!extended && path.Count > 0)
            {
                foreach (var m in path)
                    result.Add(m);
            }
        }
    }
}