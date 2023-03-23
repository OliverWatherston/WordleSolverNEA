using System;
using System.Threading;
using static WordleSolver.Program;

namespace WordleSolver
{
    public class Menu
    {
        public static string AsciiArtTitle = @"                          
   _ _ _           _ _        _____     _             
  | | | |___ ___ _| | |___   |   __|___| |_ _ ___ ___ 
  | | | | . |  _| . | | -_|  |__   | . | | | | -_|  _|
  |_____|___|_| |___|_|___|  |_____|___|_|\_/|___|_|  
            ";
        public static void PrintGameInfo()
        {
            Console.WriteLine(AsciiArtTitle);
            Console.WriteLine(
                @"Below is the description of how to operate the Wordle solver, when asked for a 'Response' you should input the response from Wordle as described below.
  Green  :  G
  Yellow :  Y
  Grey   :  X

Please note that if you'd like to exit the solver or helper at any time, you can input 'EXIT' as the response.");
        } // print information about the solver along with a description of how to use it and a nice ascii art title
        
        public static void SolverMenuOption()
        {
            string solverGuess = CurrentWordleSolver.MakeGuess(); // get the next guess from the solver

            if (String.IsNullOrEmpty(solverGuess)) // check if the solver has run out of words to guess, this could be because a use has inputted an invalid response but we can't really control them for that
            {
                Console.WriteLine("\nSolver has run out of words to guess!");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine($"\nSolver next guess : {solverGuess.ToUpper()}");

            Guess guess = new Guess(solverGuess); // create a new guess object to handle the response from the user, this tidies up the menu a bit since we use it here and within the helper menu option
            
            if (guess.IsExitResponse()) // we have a condition in the guess response for if you want to exit to the main menu quickly, this is handled here
            {
                Console.WriteLine("\nReturning you to the main menu...");
                Thread.Sleep(1000);
                return;
            }
            
            if (guess.CheckIfResponseIsCorrect()) // if the guess is correct, we can end the game and return to the main menu
            {
                Console.WriteLine("\n\nSolver has solved the Wordle!");
                Console.WriteLine($"The word was : {solverGuess.ToUpper()}");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
            }
            else
            {
                CurrentWordleSolver.UpdateSolver(guess); // if the guess is incorrect, the solver needs to be fed the response from the previous guess so it can update its internal state
                SolverMenuOption(); // we loop here to keep asking for guesses until the solver solves the wordle
            }
        }

        public static void HelperMenuOption()
        {
            Console.Write("\nWould you like to input another word? (Y/n) ");
            string choice = Console.ReadLine();

            string playerOrSolverGuess;
            
            if (choice?.ToLower() == "n") // if the user doesn't want to input another word, we can just use the solver to get the next guess this is the main functionality of the helper over the solver since the user has more input
            {
                playerOrSolverGuess = CurrentWordleSolver.MakeGuess();
                Console.WriteLine($"\nSolver next guess : {playerOrSolverGuess.ToUpper()}");
                
                if (String.IsNullOrEmpty(playerOrSolverGuess)) // we need to check twice for this since this shows the solver cannot guess the word and we need to bail out to the main menu
                {
                    Console.WriteLine("\nSolver has run out of words to guess!");
                    Console.WriteLine("Press any key to return to the main menu.");
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.Write("\nGuess : ");
                playerOrSolverGuess = Console.ReadLine()?.ToLower();
            }

            if (String.IsNullOrEmpty(playerOrSolverGuess)) // this check makes sure that the user has inputted a valid guess, if they haven't we can just return to the helper menu
            {
                Console.WriteLine("You did not enter a valid guess!");
                HelperMenuOption();
                return;
            }

            Guess guess = new Guess(playerOrSolverGuess); // create a new guess object to handle the response from the user, this tidies up the menu a bit since we use it here and within the solver menu option
            
            if (guess.IsExitResponse()) // we have a condition in the guess response for if you want to exit to the main menu quickly, this is handled here
            {
                Console.WriteLine("\nReturning you to the main menu...");
                Thread.Sleep(1000);
                return;
            }

            if (!guess.IsWordInWordList)
            {
                Console.WriteLine("The word you have entered is not in the word list!");
                HelperMenuOption();
                return;
            }
            
            if (guess.CheckIfResponseIsCorrect()) // if the guess is correct, we can end the game and return to the main menu
            {
                Console.WriteLine("\n\nYou have solved the Wordle!");
                Console.WriteLine($"The word was : {playerOrSolverGuess.ToUpper()}");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
            }
            else
            {
                CurrentWordleSolver.UpdateSolver(guess); // if the guess is incorrect, the solver needs to be fed the response from the previous guess so it can update its internal state
                HelperMenuOption(); // we loop here to keep asking for guesses until the user solves the wordle
            }
        }

        public static void QuitMenuOption()
        {
            Environment.Exit(0); // exit gracefully (overkill to define exit code but it's good practice)
        }
    }
}