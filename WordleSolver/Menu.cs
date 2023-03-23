using System;
using System.Threading;
using static WordleSolver.Program;

namespace WordleSolver
{
    public class Menu
    {
        private static readonly WordList _helperWordList = new WordList(); // clean wordlist to be used by the helper
        
        public static void PrintGameInfo()
        {
            Console.WriteLine(Globals.AsciiArtTitle);
            Console.WriteLine(Globals.GameInformation);
        } // print information about the solver along with a description of how to use it and a nice ascii art title

        private static bool SolverMenuDialogue(string bestGuess)
        {
            if (!CurrentWordleSolver.HasGuessesRemaining()) // check if the solver has run out of words to guess, this could be because a use has inputted an invalid response but we can't really control them for that
            {
                Console.WriteLine("\nSolver has run out of words to guess!");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
                return true;
            }
            
            Console.WriteLine($"\nSolver next guess : {bestGuess.ToUpper()}");

            var guess = new Guess(bestGuess); // create a new guess object to handle the response from the user, this tidies up the menu a bit since we use it here and within the helper menu option
            
            if (guess.IsExitResponse()) // we have a condition in the guess response for if you want to exit to the main menu quickly, this is handled here
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

            CurrentWordleSolver.UpdateSolver(guess); // if the guess is incorrect, the solver needs to be fed the response from the previous guess so it can update its internal state
            return false;
        }
        
        public static void SolverMenuOption()
        {
            var bestGuess = CurrentWordleSolver.MakeGuess(); // get the next guess from the solver

            if(!SolverMenuDialogue(bestGuess))
            {
                SolverMenuOption(); // we loop here to keep asking for guesses until the solver solves the wordle
            }
        }

        private static string HandleSelfGuess(string bestGuess)
        {
            do
            {
                if (bestGuess != null)
                {
                    Console.WriteLine("Invalid word, please try again.\n");
                }
                    
                Console.Write("\nGuess : ");
                bestGuess = Console.ReadLine()?.ToLower();
            } while (!_helperWordList.ContainsWord(bestGuess));
            
            return bestGuess;
        }

        public static void HelperMenuOption()
        {
            Console.Write("\nWould you like to input another word? (Y/n) ");
            var selfWordChoice = Console.ReadLine();

            string bestGuess = null;
            
            if (selfWordChoice?.ToLower() != "n") // if the user doesn't want to input another word, we can just use the solver to get the next guess this is the main functionality of the helper over the solver since the user has more input
            {
                bestGuess = HandleSelfGuess(bestGuess);
            }
            else
            {
                bestGuess = CurrentWordleSolver.MakeGuess();
            }

            if(!SolverMenuDialogue(bestGuess))
            {
                HelperMenuOption(); // we loop here to keep asking for guesses until the user solves the wordle
            }
        }

        public static void QuitMenuOption()
        {
            Environment.Exit(0); // exit gracefully (overkill to define exit code but it's good practice)
        }
    }
}