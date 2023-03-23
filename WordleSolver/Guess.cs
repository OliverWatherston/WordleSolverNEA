namespace WordleSolver
{
    public struct Guess
    {
        public string Word { get; set; }
        public string Response { get; set; }

        public Guess(string word, string response)
        {
            Word = word;
            Response = response;
        }
    }
}