using System;
using static WordleSolver.Menu;

namespace WordleSolver
{
    internal class Program
    {
        public static Solver CurrentWordleSolver { get; private set; } // create a static instance of the solver to make it easier to use in the menu
        private static void Main(string[] args)
        {
            Console.Title = "Wordle Solver";
            MainMenu();
        }

        public static void ResetGameState() // reset game state by making a new solver instance, also clear the console for better user experience
        {
            CurrentWordleSolver = new Solver();
            Console.Clear();
        }
    }
}
