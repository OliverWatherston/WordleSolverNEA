using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordleSolver
{
    public class WordList
    {
        private readonly string _wordListPath =
            $@"{Path.GetDirectoryName(AppContext.BaseDirectory)}\wordlist.txt";

        private List<string> _words = new List<string>();
        
        private Dictionary<char, int> _letterCount = new Dictionary<char, int>();
        private Dictionary<string, int> _wordScores = new Dictionary<string, int>();

        private List<Dictionary<char, int>> _letterCountPositional = new List<Dictionary<char, int>>();
        private Dictionary<string, int> _wordScoresPositional = new Dictionary<string, int>();

        public WordList()
        {
            if (!File.Exists(_wordListPath)) // check if word list exists in same folder as executable else exit
            {
                Console.WriteLine($"Could not find 'wordlist.txt' at location '{_wordListPath}'!");
                System.Threading.Thread.Sleep(10000);
                Environment.Exit(0);
            }
            
            string[] wordListFile = File.ReadAllLines(_wordListPath); // assign word list to the string list _words
            _words.AddRange(wordListFile);

            CalculateAllWordScores();
        }

        public int GetWordListLength()
        {
            return _words.Count;
        }
        
        public void TryRemoveWord(string word)
        {
            if (_words.Contains(word))
            {
                _words.Remove(word);
            }
        }
        
        public bool ContainsWord(string word)
        {
            return _words.Contains(word);
        }

        public string GetWordListAsStringOrdered()
        {
            return string.Join(", ", _words.OrderBy(word => _wordScores[word]));
        }

        public string CalculateBestGuessWordMiniMax(bool usePositional = true)
        {
            var scores = usePositional ? _wordScoresPositional : _wordScores; // allow use of two different scoring methods, but only positional is currently used
            var bestPair = new KeyValuePair<string, int>();

            foreach (var word in scores) // simply compare all words in the scores dictionary and return the one with the highest score
            {
                if (word.Value > bestPair.Value)
                {
                    bestPair = word;
                }
            }

            return bestPair.Key;
        }
        
        private static int IsLetterInWord(char letter, string word)
        {
            return word.Contains(letter) ? 1 : 0;
        }

        public string CalculateReuseBestGuessWordUsingMinimax(List<char> priorityLetters)
        {
            CalculateLetterCountNormal();
            var bestPair = new KeyValuePair<string, int>();

            foreach (var word in _words)
            {
                var score = priorityLetters.Sum(c => IsLetterInWord(c, word));
                if (score > bestPair.Value)
                {
                    bestPair = new KeyValuePair<string, int>(word, score);
                }
            }

            return bestPair.Key;
        }

        private void CalculateLetterCountNormal()
        {
            _letterCount = new Dictionary<char, int>();

            foreach (var letter in Globals.Alphabet)
            {
                _letterCount.TryAdd(letter, 0);
            }

            foreach (var word in _words)
            {
                foreach (var letter in word.Distinct().ToList()) // count the number of times each letter appears in the word list
                {
                    _letterCount[letter]++;
                }
            }
        }

        private void CalculateLetterCountPositional()
        {
            _letterCountPositional = new List<Dictionary<char, int>>();
            for (var i = 0; i < 5; i++) // initialise the dictionary for each position in the word list
            {
                _letterCountPositional.Add(new Dictionary<char, int>());
                foreach (var letter in Globals.Alphabet) // initialise the dictionary for each letter in the alphabet
                {
                    _letterCountPositional[i].Add(letter, 0);
                }
            }

            foreach (var word in _words)
            {
                for (var i = 0; i < word.Length; i++) // count the number of times each letter appears in each position in the word list
                {
                    var letter = word[i];
                    _letterCountPositional[i][letter]++;
                }
            }
        }

        public void CalculateAllWordScores()
        {
            CalculateWordScoresNormal();
            CalculateWordScoresPositional();
        }

        private void CalculateWordScoresNormal()
        {
            CalculateLetterCountNormal();
            _wordScores = new Dictionary<string, int>();
            foreach (var word in _words) // score each word in the word list by adding the number of times each letter appears in the word list, this is the basis for the minimax algorithm
            {
                _wordScores.Add(word, word.Sum(letter => _letterCount[letter]));
            }
        }

        private void HandleWordScoringPositional(string word)
        {
            var score = new Dictionary<char, int>();
            for (var i = 0; i < word.Length; i++)
            {
                var letter = word[i];
                if (!score.ContainsKey(letter))
                {
                    score[letter] = _letterCountPositional[i][letter];
                }
                else
                {
                    score[letter] = Math.Max(score[letter], _letterCountPositional[i][letter]); // if the letter appears more than once in the word, only use the highest score for that letter
                    // this is to reduce the chance of the algorithm picking a word that has a letter that appears more than once
                }
            }

            _wordScoresPositional[word] = score.Values.Sum(); // add the scores for each letter in the word to get the total score for the word
        }

        private void CalculateWordScoresPositional()
        {
            CalculateLetterCountPositional();
            _wordScoresPositional = new Dictionary<string, int>();
            foreach (var word in _words) // score each word in the word list by adding the number of times each letter appears in that position in the word list, this is the basis for the positional minimax algorithm
            {
                HandleWordScoringPositional(word);
            }
        }
        
        private static bool CheckWordAgainstRequiredMask(string word, Mask requiredMask)
        {
            for (var i = 0; i < requiredMask.GetMask().Count; i++)
            {
                var requiredLetters = requiredMask.GetIndex(i);
                if (requiredLetters.Count > 0 && word[i] != requiredLetters[0])
                {
                    return true;
                }
            }

            return false;
        }
        
        private static bool CheckWordAgainstForbiddenMask(string word, Mask forbiddenMask)
        {
            for (var i = 0; i < forbiddenMask.GetMask().Count; i++)
            {
                var forbiddenLetters = forbiddenMask.GetIndex(i);
                foreach (var forbiddenLetter in forbiddenLetters) // check if the word contains any of the forbidden letters
                {
                    if (word[i] == forbiddenLetter)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        private static bool CheckWordAgainstAllowedMask(string word, Mask allowedMask)
        {
            foreach (var letter in Globals.Alphabet) // check if the word contains any letters that are not allowed in the allowed mask
            {
                var count = word.Count(x => x == letter);
                if (!allowedMask.GetIndex(count).Contains(letter))
                {
                    return true;
                }
            }
            
            return false;
        }

        public void FilterWordListUsingMasks(Mask allowedMask, Mask requiredMask, Mask forbiddenMask)
        {
            var filteredWordList = new List<string>();
            foreach (var word in _words)
            {
                if (CheckWordAgainstRequiredMask(word, requiredMask) ||
                    CheckWordAgainstForbiddenMask(word, forbiddenMask) ||
                    CheckWordAgainstAllowedMask(word, allowedMask))
                {
                    continue;
                }

                filteredWordList.Add(word);
            }

            _words = filteredWordList; // replace the word list with the filtered word list
        }

        public void UpdateMaskWithRemainingWords(Mask allowedMask)
        {
            CalculateLetterCountNormal();
            foreach (var letterCountPair in _letterCount) // remove any letters from the allowed mask that are not in the remaining words
            {
                if (letterCountPair.Value != 0)
                {
                    continue;
                }
                
                for (var i = 1; i < 3; i++)
                {
                    allowedMask.RemoveFromPositionMask(i, letterCountPair.Key);
                }
            }
        }
    }
}