using System;
using System.Threading;
using Spectre.Console;
using static WordleSolver.Program;

namespace WordleSolver
{
    public class Menu
    {
        private static readonly WordList HelperWordList = new WordList(); // clean wordlist to be used by the helper
        
        private static void HandleMenuOption(string option)
        {
            switch (option) // switch statement handles menu options
            {
                case "Solver":
                    PrintGameInfo();
                    SolverMenuOption();
                    break;
                case "Helper":
                    PrintGameInfo();
                    HelperMenuOption();
                    break;
                case "Quit":
                    QuitMenuOption();
                    break;
            }
        }

        public static void MainMenu(bool resetGameState = true)
        {
            if (resetGameState) // control for whether or not to reset the game state (this isn't actually used in any of the menu options, but it's here for future use)
            {
                ResetGameState();
            }
                
            var option = AnsiConsole.Prompt( // use Spectre.Console to create a menu, this is the only external library used in the project
                new SelectionPrompt<string>()
                    .Title(Globals.AsciiArtTitle)
                    .PageSize(4)
                    .AddChoices(new[]
                    {
                        "Solver",
                        "Helper",
                        "Quit"
                    }));
            
            HandleMenuOption(option); // handle the menu option

            MainMenu(); // recall the main menu as most menu options return to here instead of recalling the main menu
        }
        
        private static void PrintGameInfo()
        {
            Console.WriteLine(Globals.AsciiArtTitle);
            Console.WriteLine(Globals.GameInformation);
        } // print information about the solver along with a description of how to use it and a nice ascii art title

        private static bool SolverAndHelperDialogue(string bestGuess)
        {
            // check if the solver has run out of words to guess
            // this could be because a user has inputted invalid feedback but we can't do much about that
            if (!CurrentWordleSolver.HasGuessesRemaining())
            {
                Console.WriteLine("\nSolver has run out of words to guess!");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
                return true;
            }
            
            Console.WriteLine($"\nSolver next guess : {bestGuess.ToUpper()}");

            // create a new guess object to handle the response from the user
            // this tidies up the menu a bit since we use it here and within the helper menu option
            var guess = new Guess(bestGuess);
            
            // condition in the guess response for if you want to exit to the main menu quickly, handled here
            if (guess.IsExitResponse()) 
            {
                Console.WriteLine("\nReturning you to the main menu...");
                Thread.Sleep(1000);
                return true;
            }
            
            if (guess.IsSolved()) // if the guess is correct, we can end the game and return to the main menu
            {
                Console.WriteLine("\n\nSolver has solved the Wordle!");
                Console.WriteLine($"The word was : {bestGuess.ToUpper()}");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
                return true;
            }

            // if the guess is incorrect
            // the solver needs to be fed the response from the previous guess so it can update its internal state
            CurrentWordleSolver.UpdateSolver(guess);
            return false;
        }
        
        private static void SolverMenuOption()
        {
            var bestGuess = CurrentWordleSolver.MakeGuess(); // get the next guess from the solver

            if(!SolverAndHelperDialogue(bestGuess))
            {
                SolverMenuOption(); // we loop here to keep asking for guesses until the solver solves the wordle
            }
        }

        private static void HandleSelfGuess(ref string bestGuess)
        {
            do
            {
                if (bestGuess != null)
                {
                    Console.WriteLine("Invalid word, please try again.\n");
                }
                    
                Console.Write("\nGuess : ");
                bestGuess = Console.ReadLine()?.ToLower();
            } while (!HelperWordList.ContainsWord(bestGuess));
        }

        private static void HelperMenuOption()
        {
            Console.Write("\nWould you like to input another word? (Y/n) ");
            var selfWordChoice = Console.ReadLine();

            string bestGuess = null;
            
            // if the user doesn't want to input another word
            // we can just use the solver to get the next guess
            // this is the main functionality of the helper over the solver
            // since the user has more control, the helper can actually help them
            if (selfWordChoice?.ToLower() != "n")
            {
                HandleSelfGuess(ref bestGuess);
            }
            else
            {
                bestGuess = CurrentWordleSolver.MakeGuess();
            }

            if(!SolverAndHelperDialogue(bestGuess))
            {
                HelperMenuOption(); // we loop here to keep asking for guesses until the user solves the wordle
            }
        }

        private static void QuitMenuOption()
        {
            Environment.Exit(0); // exit gracefully (overkill to define exit code but it's good practice)
        }
    }
}