using System;
using System.Text.RegularExpressions;

namespace WordleSolver
{
    public class Guess
    {
        public string Word { get;}
        public string Response { get; set; }
        private readonly bool _responseExiting;

        public Guess(string word)
        {
            Word = word;

            do
            {
                AskForResponse();
            } while (ValidateResponse() && Response != "EXIT"); // check if the response is valid or if the user wants to exit
            
            if (Response == "EXIT")
            {
                _responseExiting = true;
            }
        }

        public bool IsExitResponse()
        {
            return _responseExiting;
        }

        public bool CheckIfResponseIsCorrect()
        {
            return Response == "GGGGG";
        }

        private void AskForResponse()
        {
            Console.Write("Response: ");
            Response = Console.ReadLine()?.ToUpper(); // get the response from the user and convert it to uppercase (using null conditional operator to avoid null reference exceptions)
        }

        private bool ValidateResponse()
        {
            return String.IsNullOrEmpty(Response) || !Regex.IsMatch(Response, @"\b[GYX]{5}\b"); // check if the response is empty or if it doesn't match the regex pattern
        }                                                                                                   // regex pattern matches a string of 5 characters and each character must be either G, Y or X
    }
}