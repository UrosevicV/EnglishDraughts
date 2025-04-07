# English Draughts (Checkers) Game in C#

This is a fully functional Windows Forms application implementing the traditional English Draughts (Checkers) game. The player competes against an AI bot powered by the Minimax algorithm with customizable thinking time.

## Features

- The program is written using .NET
- The program have a GUI.
- The bot has a limited time to calculate moves (set by the user). When the time is over, the bot has to make a move, choosing the most optimal one from the ones it had time to calculate
- The bot  use all available processor cores to calculate the move
- A player can get a hint from the AI (just ask the ChatGPT)
- Third-party libraries, except for GUI, AI, and unit-test frameworks, are not used

- Bonus: A player can choose which side to play for (black(red) or white)
- Bonus: Reset button
## How to Play

1. When the game starts, you will be prompted to:
   - Choose your side (Black or White)
   - Choose how many seconds the bot should "think"
2. The game board appears and the player with the first move begins
3. You can click on your pieces to see possible moves
4. If there's a valid capture, you **must** make it
5. When no moves are available or all pieces are captured, the winner is declared

## Hint Feature

Click the "Hint" button to ask ChatGPT for the best legal move.  
(Requires a working OpenAI API key and internet connection.)

**To use the Hint functionality (powered by ChatGPT), you must have your own OpenAI API key.**
1. Open the ChatGptClient.cs file that is located at Utils folder.
2. Locate the line that defines the API key: private readonly string ChatGptUser = "chatGptUser";
3. Replace "chatGptUser" with your actual ChatGPT API key from OpenAI.

## AI Details

- AI uses the Minimax algorithm
- All available processor cores are used for parallel computation
- The alrgorithm has limited time

## Technologies Used

- C# (.NET Windows Forms)
- OpenAI GPT API (for hint functionality)
- Multithreading & `Task`, `Parallel.ForEach`, `CancellationToken` for AI performance

## Requirements

- .NET Framework
- Windows OS
- OpenAI API key (if using Hint)

## Credits

 This project was made possible thanks to the effort and tutorials of many generous content creators. Special thanks to:

- YouTube creators who explained **English Draughts rules** clearly and visually
- Developers who shared **C# GUI tutorials** using Windows Forms
- Open-source communities for inspiration and support

If you are one of the creators who inspired this project, thank you for sharing your knowledge and helping others learn!
---

Vera Urosevic
