using System;
using System.Drawing;
using System.Windows.Forms;

namespace EnglishDraughts
{
    public class SetupGameDialog : Form
    {
        public int SelectedPlayer { get; private set; } = 2; // Default: black
        public int BotThinkTimeSeconds => (int)thinkTimeInput.Value;

        private Button whiteButton;
        private Button blackButton;
        private NumericUpDown thinkTimeInput;
        private Button okButton;

        public SetupGameDialog()
        {
            this.Text = "Set Up Game";
            this.Size = new Size(420, 260);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10);

            Label label = new Label
            {
                Text = "Choose your side and set how long the bot should think:",
                Location = new Point(20, 20),
                Size = new Size(380, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };

            blackButton = new Button
            {
                Text = "Black (First)",
                Location = new Point(60, 70),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 10)
            };
            blackButton.Click += (s, e) =>
            {
                SelectedPlayer = 2;
                blackButton.BackColor = Color.LightBlue;
                whiteButton.BackColor = SystemColors.Control;
            };

            whiteButton = new Button
            {
                Text = "White (Second)",
                Location = new Point(220, 70),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 10)
            };
            whiteButton.Click += (s, e) =>
            {
                SelectedPlayer = 1;
                whiteButton.BackColor = Color.LightBlue;
                blackButton.BackColor = SystemColors.Control;
            };

            thinkTimeInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 10,
                Value = 2,
                Location = new Point(60, 130),
                Size = new Size(60, 25),
                Font = new Font("Segoe UI", 10)
            };

            Label secondsLabel = new Label
            {
                Text = "seconds",
                Location = new Point(130, 132),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };

            okButton = new Button
            {
                Text = "Start Game",
                Location = new Point(140, 180),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            okButton.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            this.Controls.Add(label);
            this.Controls.Add(whiteButton);
            this.Controls.Add(blackButton);
            this.Controls.Add(thinkTimeInput);
            this.Controls.Add(secondsLabel);
            this.Controls.Add(okButton);

            // Highlight default selected button
            blackButton.BackColor = Color.LightBlue;
        }
    }
}
