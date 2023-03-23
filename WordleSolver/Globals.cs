using System.Collections.Generic;
using System.Linq;

namespace WordleSolver
{
    public static class Globals
    {
        public static readonly List<char> Alphabet = "abcdefghijklmnopqrstuvwxyz".ToList();
        public static readonly List<char> Vowels = "aeiou".ToList();
        public static readonly string AsciiArtTitle = @"                          
   _ _ _           _ _        _____     _             
  | | | |___ ___ _| | |___   |   __|___| |_ _ ___ ___ 
  | | | | . |  _| . | | -_|  |__   | . | | | | -_|  _|
  |_____|___|_| |___|_|___|  |_____|___|_|\_/|___|_|  
            ";

        public static readonly string GameInformation =
            @"Below is the description of how to operate the Wordle solver, when asked for a 'Response' you should input the response from Wordle as described below.
  Green  :  G
  Yellow :  Y
  Grey   :  X

Please note that if you'd like to exit the solver or helper at any time, you can input 'EXIT' as the response.";
    }
}