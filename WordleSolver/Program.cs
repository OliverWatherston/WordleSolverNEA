using System;
using Spectre.Console;
using static WordleSolver.Menu;

namespace WordleSolver
{
    internal class Program
    {
        public static Solver CurrentWordleSolver { get; set; }
        static void Main(string[] args)
        {
            Console.Title = "Wordle Solver";
            MainMenu();
        }

        private static void ResetGameState()
        {
            CurrentWordleSolver = new Solver();
            Console.Clear();
        }

        private static void MainMenu(bool resetGameState = true)
        {
            if (resetGameState)
            {
                ResetGameState();
            }
                
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Wordle Solver - Menu")
                    .PageSize(6)
                    .AddChoices(new []
                    {
                        "Solver",
                        "Helper",
                        "Stats",
                        "Quit"
                    }));

            switch (option)
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
            
            MainMenu();
        }
    }
}
