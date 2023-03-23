using System;
using System.IO;
using System.Text.RegularExpressions;

namespace WordleSolver
{
    internal class Program
    {
        private static Solver _solver = new Solver();
        static void Main(string[] args)
        {
            Console.WriteLine("Wordle Solver.\nWord match usage described below.\n\nGreen: G\nYellow: Y\nGrey: X\n\n");
            Console.Title = "Wordle Solver";

            SolveWordle();
        }

        static void SolveWordle()
        {
            string guess = _solver.MakeGuess();
            Console.WriteLine($"Guess : {guess.ToUpper()}");
            
            Console.Write("Response: ");

            string wordMatch = Console.ReadLine();
            if (wordMatch == null) // guard case for checking that a value does exist, only checks whether a value is inputted
            {
                Console.WriteLine("Please enter response from Wordle.");
                SolveWordle();
                return; // return to avoid executing the code below
            }

            wordMatch = wordMatch.ToUpper();

            if (!Regex.IsMatch(wordMatch, @"\b[GYX]{5}\b"))
            {
                Console.WriteLine("You did not enter a valid response!");
                SolveWordle();
                return;
            }

            Guess guessStruct = new Guess(guess, wordMatch);
            
            _solver.RemoveWord(guess);
            
            _solver.UpdateMasks(guessStruct);
            
            _solver.FilterWordList();
            
            _solver.UpdateMaskWithRemainingWords();

            SolveWordle();
        }
    }
}
