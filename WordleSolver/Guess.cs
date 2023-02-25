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
            } while (ValidateResponse() && Response != "EXIT");
            
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
            Response = Console.ReadLine()?.ToUpper();
        }

        private bool ValidateResponse()
        {
            return String.IsNullOrEmpty(Response) || !Regex.IsMatch(Response, @"\b[GYX]{5}\b");
        }
    }
}