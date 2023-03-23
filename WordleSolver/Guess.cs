using System;
using System.Text.RegularExpressions;

namespace WordleSolver
{
    public class Guess
    {
        public string Word { get;}
        public string Feedback { get; private set; }
        private readonly bool _responseExiting;

        public Guess(string word)
        {
            Word = word;

            do
            {
                CheckIfFeedbackInvalid();
                GetFeedbackFromUser();
            } while (ValidateFeedback() && Feedback != "EXIT"); 
            // check if the response is valid or if the user wants to exit
            
            if (Feedback == "EXIT")
            {
                _responseExiting = true;
            }
        }

        public bool IsExitResponse()
        {
            return _responseExiting;
        }

        public bool IsSolved()
        {
            return Feedback == "GGGGG";
        }

        private void CheckIfFeedbackInvalid()
        {
            if (Feedback != null)
            {
                Console.WriteLine("Invalid response, please try again.\n");
            }
        }

        private void GetFeedbackFromUser()
        {
            Console.Write("Response: ");
            Feedback = Console.ReadLine()?.ToUpper(); // get the response from the user and convert it to uppercase (using null conditional operator to avoid null reference exceptions)
        }

        private bool ValidateFeedback()
        {
            // check if the response is empty or if it doesn't match the regex pattern
            // regex pattern matches a string of 5 characters and each character must be either G, Y or X
            return string.IsNullOrEmpty(Feedback) || !Regex.IsMatch(Feedback, @"\b[GYX]{5}\b"); 
        }
    }
}