using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnglishDraughts.Utils;

namespace EnglishDraughts
{
    public partial class Form1 : Form
    {
        private const int numberOfFields = 8;
        private const int cellSize = 50;

        private int currentPlayer;
        private int humanPlayer;
        private int botPlayer;
        int botThinkingTime = 3; // default

        List<Button> simpleSteps = new List<Button>();

        Button prevButton;
        Button pressedButton;
        Button hintButton;
        Button resetButton;
        Label thinkingLabel;
        Label currentPlayerLabel;
        bool isContinue = false;

        int countEatSteps = 0;
        bool isMoving;
        bool isEatStep = false;

        int[,] board = new int[numberOfFields, numberOfFields];

        Button[,] buttons = new Button[numberOfFields, numberOfFields];

        private string assetsPath = Path.Combine(Application.StartupPath, "Assets");
        private Image whiteFigure => LoadImage("white.png");
        private Image blackFigure => LoadImage("black.png");
        private Image whiteDraught => LoadImage("white_draught.png");
        private Image blackDraught => LoadImage("black_draught.png");

        public Form1()
        {
            InitializeComponent();

            try
            {
                this.Icon = new Icon("Assets/crown.ico");
                this.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                this.Text = "EnglishDraughts";
            }
            catch { }

            SetupGameDialog dialog = new SetupGameDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                humanPlayer = dialog.SelectedPlayer;
                botPlayer = humanPlayer == 1 ? 2 : 1;
                botThinkingTime = dialog.BotThinkTimeSeconds * 1000;
            }
            else
            {
                Application.Exit();
                return;
            }

            Init();

            this.Shown += (s, e) =>
            {
                if (currentPlayer == botPlayer)
                {
                    BotTurn();
                }
            };
        }

        // Initialized the game
        public void Init()
        {

            currentPlayer = 2; // black
            isMoving = false;
            prevButton = null;

            board = new int[numberOfFields, numberOfFields]
            {
                { 0, 1, 0, 1, 0, 1, 0, 1 },
                { 1, 0, 1, 0, 1, 0, 1, 0 },
                { 0, 1, 0, 1, 0, 1, 0, 1 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 2, 0, 2, 0, 2, 0, 2, 0 },
                { 0, 2, 0, 2, 0, 2, 0, 2 },
                { 2, 0, 2, 0, 2, 0, 2, 0 }
            };

            CreateMap();
        }

        #region GUI elements

        // Create GUI
        public void CreateMap()
        {
            this.Width = (numberOfFields + 5) * cellSize;
            this.Height = (numberOfFields + 2) * cellSize;

            AddAdditionalElements();

            for (int i = 0; i < numberOfFields; i++)
            {
                for (int j = 0; j < numberOfFields; j++)
                {
                    AddField(i, j);
                }
            }
        }

        // Add labels and hint button
        public void AddAdditionalElements()
        {
            // Add letters A–H
            for (int j = 0; j < numberOfFields; j++)
            {
                Label bottomLabel = new Label();
                bottomLabel.Text = ((char)('A' + j)).ToString();
                bottomLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                bottomLabel.Size = new Size(cellSize, cellSize);
                bottomLabel.Location = new Point(j * cellSize, numberOfFields * cellSize);
                bottomLabel.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(bottomLabel);
            }

            // Add numbers 1-8
            for (int i = 0; i < numberOfFields; i++)
            {
                Label rightLabel = new Label();
                rightLabel.Text = (i + 1).ToString();
                rightLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                rightLabel.Size = new Size(cellSize, cellSize);
                rightLabel.Location = new Point(numberOfFields * cellSize, i * cellSize);
                rightLabel.TextAlign = ContentAlignment.MiddleCenter;
                this.Controls.Add(rightLabel);
            }

            // Add help label
            Label helpLabel = new Label();
            helpLabel.Text = "Do you need help?\nAsk ChatGPT.";
            helpLabel.TextAlign = ContentAlignment.MiddleLeft;
            helpLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            helpLabel.Size = new Size(cellSize * 4, cellSize);
            helpLabel.Location = new Point(numberOfFields * cellSize + 20, 5 * cellSize);
            helpLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(helpLabel);

            // Add hint button
            hintButton = new Button();
            hintButton.Text = "Hint";
            hintButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            hintButton.Size = new Size(cellSize * 3, cellSize);
            hintButton.Location = new Point((numberOfFields + 1) * cellSize, cellSize * 6);
            hintButton.Click += new EventHandler(hintButton_Click);
            this.Controls.Add(hintButton);

            // Add reset button
            resetButton = new Button();
            resetButton.Text = "Reset Game";
            resetButton.Size = new Size(cellSize * 3, cellSize);
            resetButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            resetButton.Location = new Point((numberOfFields + 1) * cellSize, cellSize * 7);
            resetButton.Click += (s, e) =>
            {
                this.Controls.Clear();
                Init();
            };
            this.Controls.Add(resetButton);

            // Add thinking label
            thinkingLabel = new Label();
            thinkingLabel.Text = "";
            thinkingLabel.TextAlign = ContentAlignment.MiddleLeft;
            thinkingLabel.Size = new Size(cellSize * 4, cellSize);
            thinkingLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            thinkingLabel.Location = new Point((numberOfFields + 1) * cellSize, cellSize * 3);
            thinkingLabel.ForeColor = Color.DarkRed;
            this.Controls.Add(thinkingLabel);

            Label normalPart = new Label
            {
                Text = "Choose your side and set how long ",
                Location = new Point((numberOfFields + 1) * cellSize, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            Label boldPart = new Label
            {
                Text = "the bot should think",
                Location = new Point(normalPart.Right, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            // Add thinking label
            currentPlayerLabel = new Label();
            currentPlayerLabel.Text = "Black player is currently playing.";
            currentPlayerLabel.TextAlign = ContentAlignment.MiddleLeft;
            currentPlayerLabel.Size = new Size(cellSize * 4, cellSize);
            currentPlayerLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            currentPlayerLabel.Location = new Point((numberOfFields + 1) * cellSize, 0);
            currentPlayerLabel.ForeColor = Color.Black;
            this.Controls.Add(currentPlayerLabel);

        }

        // Add field
        public void AddField(int i, int j)
        {
            Button button = new Button();
            button.Location = new Point(j * cellSize, i * cellSize);
            button.Size = new Size(cellSize, cellSize);
            button.Click += new EventHandler(OnFigurePress);
            if (board[i, j] == 1)
            {
                button.Image = whiteFigure;
            }
            else if (board[i, j] == 2)
            {
                button.Image = blackFigure;
            }

            button.BackColor = GetPrevButtonColor(button);

            buttons[i, j] = button;

            this.Controls.Add(button);
        }
        private Image LoadImage(string fileName)
        {
            string fullPath = Path.Combine(assetsPath, fileName);
            return new Bitmap(Image.FromFile(fullPath), new Size(cellSize - 10, cellSize - 10));
        }
        #endregion

        #region Game core

        public void OnFigurePress(object sender, EventArgs e)
        {
            if (prevButton != null)
                prevButton.BackColor = GetPrevButtonColor(prevButton);

            pressedButton = sender as Button;

            if (IsCurrentPlayerFigure(pressedButton))
            {
                HandleFigureSelection(pressedButton);
            }
            else
            {
                if (isMoving)
                {
                    HandleMoveExecution(pressedButton);
                }
            }

            prevButton = pressedButton;
        }

        private bool IsCurrentPlayerFigure(Button b)
        {
            int i = b.Location.Y / cellSize;
            int j = b.Location.X / cellSize;

            if (isContinue && b != prevButton)
                return false;

            return board[i, j] != 0 && board[i, j] == currentPlayer;
        }

        // If the pressed button is figure selection
        private void HandleFigureSelection(Button b)
        {
            CloseSteps();
            HighlightCurrentPlayerFiguresOnly();
            b.Enabled = true;
            countEatSteps = 0;

            bool allowBackward = b.Text == "D";
            ShowSteps(b.Location.Y / cellSize, b.Location.X / cellSize, allowBackward);

            if (isMoving)
            {
                CloseSteps();
                b.BackColor = GetPrevButtonColor(b);
                ShowPossibleSteps();
                isMoving = false;
            }
            else
            {
                isMoving = true;
            }
        }

        // If the pressed button is button where to go
        private void HandleMoveExecution(Button b)
        {
            if (countEatSteps == 0 && isEatStep)
            {
                MessageBox.Show("Moraš da pojedeš figuru!");
                return;
            }

            isContinue = false;

            if (IsJumpMove(prevButton, b))
            {
                isContinue = true;
                DeleteEaten(b, prevButton);
            }

            PerformMove(prevButton, b);
            AfterMoveChecks(b);
        }

        // Check if it is jump
        private bool IsJumpMove(Button from, Button to)
        {
            int fromX = from.Location.X / cellSize;
            int fromY = from.Location.Y / cellSize;
            int toX = to.Location.X / cellSize;
            int toY = to.Location.Y / cellSize;

            return Math.Abs(fromX - toX) > 1;
        }

        private void PerformMove(Button from, Button to)
        {
            int fromI = from.Location.Y / cellSize;
            int fromJ = from.Location.X / cellSize;
            int toI = to.Location.Y / cellSize;
            int toJ = to.Location.X / cellSize;

            board[toI, toJ] = board[fromI, fromJ];
            board[fromI, fromJ] = 0;

            to.Image = from.Image;
            from.Image = null;

            to.Text = from.Text;
            from.Text = "";

            TryToSwitchButtonToDraught(to);
        }

        private void AfterMoveChecks(Button to)
        {
            countEatSteps = 0;
            isMoving = false;
            CloseSteps();
            HighlightCurrentPlayerFiguresOnly();

            bool isDraught = to.Text != "D";
            ShowSteps(to.Location.Y / cellSize, to.Location.X / cellSize, isDraught);

            if (countEatSteps == 0 || !isContinue)
            {
                CloseSteps();
                SwitchPlayer();
                ShowPossibleSteps();

                if (currentPlayer == botPlayer) // Bot
                    BotTurn();

                isContinue = false;
            }
            else
            {
                to.Enabled = true;
                isMoving = true;
            }
        }

        // Show all legal steps
        public void ShowPossibleSteps()
        {
            isEatStep = false;
            DeactivateAllButtons();

            for (int i = 0; i < numberOfFields; i++)
            {
                for (int j = 0; j < numberOfFields; j++)
                {
                    if (board[i, j] == currentPlayer)
                    {
                        bool isOneStep = buttons[i, j].Text != "D";

                        if (IsButtonHasEatStep(i, j, isOneStep, new int[2] { 0, 0 }))
                        {
                            isEatStep = true;
                            buttons[i, j].Enabled = true;
                        }
                    }
                }
            }

            if (!isEatStep)
                ActivateAllButtons();
        }

        // Show legal steps for the selected figure.
        public void ShowSteps(int iCurrFigure, int jCurrFigure, bool allowBackward = true)
        {
            simpleSteps.Clear(); 
            ShowDiagonal(iCurrFigure, jCurrFigure, true, allowBackward);

            // if there are eat steps, only display them
            if (countEatSteps > 0)
                CloseSimpleSteps(simpleSteps);
        }

        // Check diagonal field and mark legal one
        public void ShowDiagonal(int IcurrFigure, int JcurrFigure, bool isOneStep, bool includeBackward)
        {
            int direction = currentPlayer == 1 ? 1 : -1;

            var directions = includeBackward
                ? new (int, int)[] { (-1, -1), (-1, 1), (1, -1), (1, 1) }
                : (currentPlayer == 1
                    ? new (int, int)[] { (1, -1), (1, 1) }
                    : new (int, int)[] { (-1, -1), (-1, 1) });

            foreach (var (di, dj) in directions)
            {
                int ni = IcurrFigure + di;
                int nj = JcurrFigure + dj;

                if (IsInsideBorders(ni, nj))
                {
                    if (!DeterminePath(ni, nj))
                        continue;
                }

                if (isOneStep) continue;
            }
        }

        // Check if figure can jump to this field
        public bool DeterminePath(int ti, int tj)
        {
            // Regular move
            if (board[ti, tj] == 0 && !isContinue)
            {
                buttons[ti, tj].BackColor = Color.DarkRed;
                buttons[ti, tj].Enabled = true;
                simpleSteps.Add(buttons[ti, tj]);
            }
            else
            {
                // Eating move
                if (board[ti, tj] != currentPlayer)
                {
                    ShowProceduralEat(ti, tj);
                }

                return false;
            }

            return true;
        }

        // If there is eating move, close simple move
        public void CloseSimpleSteps(List<Button> simpleSteps)
        {
            if (simpleSteps.Count > 0)
            {
                for (int i = 0; i < simpleSteps.Count; i++)
                {
                    simpleSteps[i].BackColor = GetPrevButtonColor(simpleSteps[i]);
                    simpleSteps[i].Enabled = false;
                }
            }
        }

        public void ShowProceduralEat(int i, int j, bool isOneStep = true) // isOneStep is when I tought that draught can move more then 1 step
        {
            int dirX = i - pressedButton.Location.Y / cellSize;
            int dirY = j - pressedButton.Location.X / cellSize;
            dirX = dirX < 0 ? -1 : 1;
            dirY = dirY < 0 ? -1 : 1;

            // ❗ Blokiraj nelegalno jedenje unazad za obične figure
            if (pressedButton.Text != "D")
            {
                int allowedDirection = currentPlayer == 1 ? 1 : -1;
                if (dirX != allowedDirection)
                    return;
            }

            int il = i;
            int jl = j;
            bool isEnemyFound = false;

            while (IsInsideBorders(il, jl))
            {
                if (board[il, jl] != 0 && board[il, jl] != currentPlayer)
                {
                    isEnemyFound = true;
                    break;
                }

                il += dirX;
                jl += dirY;

                if (isOneStep) break;
            }

            if (!isEnemyFound) return;

            List<Button> toClose = new List<Button>();
            bool closeSimple = false;

            int ik = il + dirX;
            int jk = jl + dirY;

            while (IsInsideBorders(ik, jk))
            {
                if (board[ik, jk] == 0)
                {
                    if (IsButtonHasEatStep(ik, jk, isOneStep, new int[2] { dirX, dirY }))
                    {
                        closeSimple = true;
                    }
                    else
                    {
                        toClose.Add(buttons[ik, jk]);
                    }

                    buttons[ik, jk].BackColor = Color.DarkRed;
                    buttons[ik, jk].Enabled = true;
                    countEatSteps++;
                }
                else break;

                if (isOneStep) break;

                ik += dirX;
                jk += dirY;
            }

            if (closeSimple && toClose.Count > 0)
            {
                CloseSimpleSteps(toClose);
            }
        }

        // Check eating options
        public bool IsButtonHasEatStep(int i, int j, bool isOneStep, int[] dir)
        {
            int[,] directions;

            // Draght - all 4 diagonals
            if (!isOneStep)
            {
                directions = new int[,]
                {
                    { -1, -1 }, { -1, 1 },
                    { 1, -1 }, { 1, 1 }
                };
            }
            else
            {
                // Simple figre - 2 diagonals
                directions = currentPlayer == 1
                    ? new int[,] { { 1, -1 }, { 1, 1 } } // white down
                    : new int[,] { { -1, -1 }, { -1, 1 } }; // black up
            }

            for (int d = 0; d < directions.GetLength(0); d++)
            {
                int di = directions[d, 0];
                int dj = directions[d, 1];

                int enemyI = i + di;
                int enemyJ = j + dj;
                int landI = i + 2 * di;
                int landJ = j + 2 * dj;

                if (IsInsideBorders(enemyI, enemyJ) && IsInsideBorders(landI, landJ))
                {
                    int enemy = board[enemyI, enemyJ];
                    int landing = board[landI, landJ];

                    if (enemy != 0 && enemy != currentPlayer && landing == 0)
                        return true;
                }
            }

            return false;
        }

        // Change the current player and prepare everything for new move
        public void SwitchPlayer()
        {
            currentPlayer = currentPlayer == 1 ? 2 : 1;
            currentPlayerLabel.Text = currentPlayer == 1
                ? "White player is currently playing."
                : "Black player is currently playing.";
            ResetGame();
        }

        public bool IsInsideBorders(int ti, int tj)
        {
            if (ti >= numberOfFields || tj >= numberOfFields || ti < 0 || tj < 0)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region End of the game
        public void ResetGame()
        {
            bool player1HasFigures = false;
            bool player2HasFigures = false;

            for (int i = 0; i < numberOfFields; i++)
            {
                for (int j = 0; j < numberOfFields; j++)
                {
                    if (board[i, j] == 1)
                        player1HasFigures = true;
                    else if (board[i, j] == 2)
                        player2HasFigures = true;
                }
            }

            string winner = null;
            // If there is no more figures
            if (!player1HasFigures && player2HasFigures)
                winner = "Black";
            else if (!player2HasFigures && player1HasFigures)
                winner = "White";

            // If there is no legal moves
            else if (!PlayerHasLegalMoves(2))
                winner = "White";
            else if (!PlayerHasLegalMoves(1))
                winner = "Black";

            if (!String.IsNullOrEmpty(winner))
            {
                MessageBox.Show($"{winner} is winner!");
                this.Controls.Clear();
                Init();
            }
        }

        // Check if there are legal moves
        public bool PlayerHasLegalMoves(int player)
        {
            for (int i = 0; i < numberOfFields; i++)
            {
                for (int j = 0; j < numberOfFields; j++)
                {
                    if (board[i, j] == player)
                    {
                        bool isDama = buttons[i, j].Text == "D";

                        // Draughts goes in 4 diagonals
                        int[] dirX = { -1, -1, 1, 1 };
                        int[] dirY = { -1, 1, -1, 1 };

                        for (int d = 0; d < 4; d++)
                        {
                            int ni = i + dirX[d];
                            int nj = j + dirY[d];

                            if (ni >= 0 && ni < numberOfFields && nj >= 0 && nj < numberOfFields)
                            {
                                if (board[ni, nj] == 0)
                                {
                                    // Simple figures can't go back
                                    if (!isDama)
                                    {
                                        if ((player == 1 && dirX[d] < 0) || (player == 2 && dirX[d] > 0))
                                            continue;
                                    }

                                    return true;
                                }

                                // Jump check
                                int jumpI = i + 2 * dirX[d];
                                int jumpJ = j + 2 * dirY[d];

                                if (jumpI >= 0 && jumpI < numberOfFields && jumpJ >= 0 && jumpJ < numberOfFields)
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
        #endregion

        #region GUI utills

        // Get background of the button
        public Color GetPrevButtonColor(Button prevButton)
        {
            int row = prevButton.Location.Y / cellSize;
            int col = prevButton.Location.X / cellSize;

            bool isDark = (row + col) % 2 == 1;
            return isDark ? Color.DarkGreen : Color.WhiteSmoke;
        }

        // Mark only figures of current player
        public void HighlightCurrentPlayerFiguresOnly()
        {
            for (int i = 0; i < numberOfFields; i++)
            {
                for (int j = 0; j < numberOfFields; j++)
                {
                    if (board[i, j] == currentPlayer)
                    {
                        buttons[i, j].Enabled = true;
                        buttons[i, j].BackColor = GetPrevButtonColor(buttons[i, j]);
                    }
                    else
                    {
                        buttons[i, j].Enabled = false;
                        buttons[i, j].BackColor = GetPrevButtonColor(buttons[i, j]);
                    }
                }
            }
        }

        // Make figure draught If it is needed
        public void TryToSwitchButtonToDraught(Button button)
        {
            int i = button.Location.Y / cellSize;
            int j = button.Location.X / cellSize;

            if (board[i, j] == 1 && i == numberOfFields - 1) // white figure became draught
            {
                button.Text = "D";
                button.ForeColor = Color.White; 
                button.Image = whiteDraught; 
            }

            if (board[i, j] == 2 && i == 0) //  Black figure became draught
            {
                button.Text = "D";
                button.ForeColor = Color.Black;
                button.Image = blackDraught; 
            }
        }

        // Delete figure
        public void DeleteEaten(Button endButton, Button startButton)
        {
            int count = Math.Abs(endButton.Location.Y / cellSize - startButton.Location.Y / cellSize);
            int startIndexX = endButton.Location.Y / cellSize - startButton.Location.Y / cellSize;
            int startIndexY = endButton.Location.X / cellSize - startButton.Location.X / cellSize;
            startIndexX = startIndexX < 0 ? -1 : 1;
            startIndexY = startIndexY < 0 ? -1 : 1;
            int currCount = 0;
            int i = startButton.Location.Y / cellSize + startIndexX;
            int j = startButton.Location.X / cellSize + startIndexY;
            while (currCount < count - 1)
            {
                board[i, j] = 0;
                buttons[i, j].Image = null;
                buttons[i, j].Text = "";
                i += startIndexX;
                j += startIndexY;
                currCount++;
            }

        }

        public void CloseSteps()
        {
            for (int i = 0; i < numberOfFields; i++)
            {
                for (int j = 0; j < numberOfFields; j++)
                {
                    buttons[i, j].BackColor = GetPrevButtonColor(buttons[i, j]);
                }
            }
        }

        public void ActivateAllButtons()
        {
            for (int i = 0; i < numberOfFields; i++)
            {
                for (int j = 0; j < numberOfFields; j++)
                {
                    buttons[i, j].Enabled = true;
                }
            }
        }

        public void DeactivateAllButtons()
        {
            for (int i = 0; i < numberOfFields; i++)
            {
                for (int j = 0; j < numberOfFields; j++)
                {
                    buttons[i, j].Enabled = false;
                }
            }
        }
        private async void hintButton_Click(object sender, EventArgs e)
        {
            try
            {
                thinkingLabel.Text = "ChatGPT is thinking.\nPlease wait...";
                hintButton.Enabled = false;

                DeactivateAllButtons();
                ChatGptClient chatGptClient = new ChatGptClient();
                string odgovor = await chatGptClient.AskForHintAsync(board, currentPlayer, buttons);

                MessageBox.Show(odgovor);
                ActivateAllButtons();
            }
            catch (Exception)
            {
                MessageBox.Show("Error with ChatGpt connection. Please check API key and credits.");
            }
            finally
            {
                thinkingLabel.Text = "";
                hintButton.Enabled = true;
            }
        }


        #endregion

        #region Bot region

        public async void BotTurn(int? fromI = null, int? fromJ = null)
        {
            await Task.Delay(300); // Move can be visible

            MinimaxAi ai = new MinimaxAi();
            var bestMove = await ai.FindBestMoveWithTimeoutAsync(board, buttons, currentPlayer, 4, botThinkingTime * 1000);

            if (bestMove == null)
            {
                MessageBox.Show("No moves available for bot.");
                return;
            }

            Button fromBtn = buttons[bestMove.FromI, bestMove.FromJ];
            Button toBtn = buttons[bestMove.ToI, bestMove.ToJ];

            OnFigurePress(fromBtn, null);
            await Task.Delay(300); // Move can be visible
            isMoving = true; 
            OnFigurePress(toBtn, null);

            // If gthe figure became draught, end of the move
            if (toBtn.Text == "D")
                return;

            // If there is move for the same figure, continue
            bool canContinueJump = ai.GenerateAllMoves(board, buttons, currentPlayer)
                .Any(m => m.FromI == bestMove.ToI &&
                          m.FromJ == bestMove.ToJ &&
                          m.IsJump);

            if (canContinueJump)
            {
                await Task.Delay(300); // Move can be visible
                BotTurn(bestMove.ToI, bestMove.ToJ);
            }
        }
        #endregion
    }
}
