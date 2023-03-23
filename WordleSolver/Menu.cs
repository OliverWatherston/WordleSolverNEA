using System;
using System.Threading;
using static WordleSolver.Program;

namespace WordleSolver
{
    public class Menu
    {
        public static void PrintGameInfo()
        {
            Console.WriteLine(@"                          
   _ _ _           _ _        _____     _             
  | | | |___ ___ _| | |___   |   __|___| |_ _ ___ ___ 
  | | | | . |  _| . | | -_|  |__   | . | | | | -_|  _|
  |_____|___|_| |___|_|___|  |_____|___|_|\_/|___|_|  
            ");
            Console.WriteLine(
                @"Below is the description of how to operate the Wordle solver, when asked for a 'Response' you should input the response from Wordle as described below.
  Green  :  G
  Yellow :  Y
  Grey   :  X

Please note that if you'd like to exit the solver or helper at any time, you can input 'EXIT' as the response.");
        }
        
        public static void SolverMenuOption()
        {
            string solverGuess = CurrentWordleSolver.MakeGuess();

            if (String.IsNullOrEmpty(solverGuess))
            {
                Console.WriteLine("\nSolver has run out of words to guess!");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine($"\nSolver next guess : {solverGuess.ToUpper()}");

            Guess guess = new Guess(solverGuess);
            
            if (guess.IsExitResponse())
            {
                Console.WriteLine("\nReturning you to the main menu...");
                Thread.Sleep(1000);
                return;
            }
            
            if (guess.CheckIfResponseIsCorrect())
            {
                Console.WriteLine("\n\nSolver has solved the Wordle!");
                Console.WriteLine($"The word was : {solverGuess.ToUpper()}");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
            }
            else
            {
                CurrentWordleSolver.UpdateSolver(guess);
                SolverMenuOption();
            }
        }

        public static void HelperMenuOption()
        {
            Console.Write("\nWould you like to input another word? (Y/n) ");
            string choice = Console.ReadLine();

            string playerOrSolverGuess;
            
            if (choice?.ToLower() == "n")
            {
                playerOrSolverGuess = CurrentWordleSolver.MakeGuess();
                Console.WriteLine($"\nSolver next guess : {playerOrSolverGuess.ToUpper()}");
                
                if (String.IsNullOrEmpty(playerOrSolverGuess))
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
                playerOrSolverGuess = Console.ReadLine();
            }

            if (String.IsNullOrEmpty(playerOrSolverGuess))
            {
                Console.WriteLine("You did not enter a valid guess!");
                HelperMenuOption();
                return;
            }

            Guess guess = new Guess(playerOrSolverGuess);
            
            if (guess.IsExitResponse())
            {
                Console.WriteLine("\nReturning you to the main menu...");
                Thread.Sleep(1000);
                return;
            }
            
            if (guess.CheckIfResponseIsCorrect())
            {
                Console.WriteLine("\n\nYou have solved the Wordle!");
                Console.WriteLine($"The word was : {playerOrSolverGuess.ToUpper()}");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
            }
            else
            {
                CurrentWordleSolver.UpdateSolver(guess);
                HelperMenuOption();
            }
        }

        public static void QuitMenuOption()
        {
            Environment.Exit(0);
        }
    }
}