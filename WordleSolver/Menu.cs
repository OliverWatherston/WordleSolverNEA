using System;
using System.Text.RegularExpressions;
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
            Console.WriteLine(@"Below is the description of how to operate the Wordle solver, when asked for a 'Response' you should input the response from Wordle as described below.
  Green  :  G
  Yellow :  Y
  Grey   :  X");
        }
        
        public static void SolverMenuOption()
        {
            
            string solverGuess = CurrentWordleSolver.MakeGuess();

            if (String.IsNullOrEmpty(solverGuess))
            {
                Console.WriteLine("Solver has run out of words to guess!");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine($"\nSolver next guess : {solverGuess.ToUpper()}");

            Guess guess = new Guess(solverGuess);
            
            if (guess.CheckIfResponseIsCorrect())
            {
                Console.WriteLine("\n\nSolver has solved the Wordle!");
                Console.WriteLine($"The word was : {solverGuess.ToUpper()}");
                Console.WriteLine("Press any key to return to the main menu.");
                Console.ReadKey();
                return;
            } else if (guess.IsExitReponse())
            {
                Console.WriteLine("\nReturning you to the main menu...");
                Thread.Sleep(1000);
            }
            else
            {
                CurrentWordleSolver.UpdateSolver(guess);
                SolverMenuOption();
            }
        }

        public static void HelperMenuOption()
        {
            /*PrintGameInfo();
            
            Console.WriteLine($"Remaining words : {_solver.GetWordsRemaining()}");
            if (_solver.GetWordsRemaining() <= 20)
            {
                Console.WriteLine(_solver.GetWordsString());
            } 
            Console.Write("Would you like to input another word? (Y/n) ");
            string inputNewWord = Console.ReadLine();

            string guess;

            if (inputNewWord?.ToLower() == "n")
            {
                guess = _solver.MakeGuess();
                Console.WriteLine($"\nHelper next guess : {guess.ToUpper()}");
            }
            else
            {
                Console.Write("\nGuess : ");
                guess = Console.ReadLine();
            }
            Console.Write("Response : ");
            string wordMatch = Console.ReadLine();
            
            guess = guess?.ToLower();
            wordMatch = wordMatch?.ToUpper();

            if (guess == null || wordMatch == null || !Regex.IsMatch(wordMatch, @"\b[GYX]{5}\b") || !_solver.IsWordInWordList(guess))
            {
                Console.WriteLine("You did not enter a valid guess or response!");
                HelperMenuOption();
                return; // return to avoid executing the code below
            }
            
            Guess guessStruct = new Guess(guess, wordMatch);
            
            _solver.RemoveWord(guess);
            
            _solver.UpdateMasks(guessStruct);
            
            _solver.FilterWordList();
            
            _solver.UpdateMaskWithRemainingWords();

            HelperMenuOption();*/
        }

        public static void StatsMenuOption()
        {
            
        }

        public static void QuitMenuOption()
        {
            Environment.Exit(0);
        }
    }
}