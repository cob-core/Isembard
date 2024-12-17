# Isembard
### The Victoria 3 Crash Log Interpreter
Isembard reads the log files from the crashes folder of your game, analyses their contents, and generates reasoning for why the game is crashing.
> This application is extremely work in progress. It is buggy and unpolished.
> > Please leave your crash folder in the 'Issues' section if you get inconclusive results! I'll see what's going on with it and hopefully improve it's detection.


![image](https://github.com/user-attachments/assets/8e19c486-fc06-44e2-af8b-4ff6371f3d1e)


## Description
Luckily, Isembard doesn't just spit out errors in your face and go 'deal with it'. It understands what the errors you get mean and how that led to the crash. From this, it suggests the best solution and how to move forward. 
If it's a game bug, it will tell you, if it's a mod you've just started using, it will tell you that too. Even if it's not Victoria 3 with the issue, it'll tell you that too. Bluescreens, driver issues, it will help you get Victoria 3 back up and running.

### AI Usage
It does not parse the crash logs using AI, though parts of the code is written with the help of ChatGPT.

## How to Use
### Windows
1. Download the .exe from the releases.
2. Run the .exe! 

The app is self-contained and can run with the single file.

### Linux
1. Download the file from the releases.
2. Run the app via terminal. .\Isembard.

Apologies, I have not tested the linux binary. Please let me know if there are issues.
Please note the default file paths do not work on Linux.

## To-Do

- Multi-game support. It appears most of the other Paradox games use the same log format. This tool should (in theory) also anaylse those games' crashes.
- Add a 'Deep Analysis' button which reads through the entire crash log. Currently the app reads through the last 20 lines in order to find critical errors but it will struggle with mass errors that don't contain file paths.
- Check mod conflicts.
- Check mod dependencies.
- Read the exception file.
- Give it a good ol' polish.
