using System;
using Spectre.Console;
using static WordleSolver.Menu;

namespace WordleSolver
{
    internal class Program
    {
        public static Solver CurrentWordleSolver { get; set; } // create a static instance of the solver to make it easier to use in the menu
        static void Main(string[] args)
        {
            Console.Title = "Wordle Solver";
            MainMenu();
        }

        private static void ResetGameState() // reset game state by making a new solver instance, also clear the console for better user experience
        {
            CurrentWordleSolver = new Solver();
            Console.Clear();
        }

        private static void MainMenu(bool resetGameState = true)
        {
            if (resetGameState) // control for whether or not to reset the game state (this isn't actually used in any of the menu options, but it's here for future use)
            {
                ResetGameState();
            }
                
            var option = AnsiConsole.Prompt( // use Spectre.Console to create a menu, this is the only external library used in the project
                new SelectionPrompt<string>()
                    .Title(AsciiArtTitle)
                    .PageSize(4)
                    .AddChoices(new []
                    {
                        "Solver",
                        "Helper",
                        "Quit"
                    }));

            switch (option) // switch statement handles menu options, menu options are handled within their own class called Menu.cs
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
            
            MainMenu(); // recall the main menu as most menu options return to here instead of recalling the main menu
        }
    }
}
