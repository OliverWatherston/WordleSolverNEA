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
            
            _words.AddRange(File.ReadAllLines(_wordListPath)); // assign word list to the string list _words

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
            // allow use of two different scoring methods, but only positional is currently used
            var scores = usePositional ? _wordScoresPositional : _wordScores;

            // simply compare all words in the scores dictionary and return the one with the highest score
            return scores.Aggregate((x,y) => x.Value > y.Value ? x : y).Key;
        }
        
        private static int IsLetterInWord(char letter, string word)
        {
            return word.Contains(letter) ? 1 : 0;
        }
        
        private static int GetWordPriorityScore(List<char> priorityLetters, string word)
        {
            return priorityLetters.Sum(letter => IsLetterInWord(letter, word));
        }

        private static string ReturnHighestScoringWordPositional(List<char> priorityLetters, string highestScoringWord, string currentWord)
        {
            var highestScoringWordScore = GetWordPriorityScore(priorityLetters, highestScoringWord);
            var currentWordScore = GetWordPriorityScore(priorityLetters, currentWord);
            
            if (currentWordScore > highestScoringWordScore)
            {
                return currentWord;
            }

            return highestScoringWord;
        }

        public string CalculateReuseBestGuessWordUsingMinimax(List<char> priorityLetters)
        {
            return _words.Aggregate((x,y) => ReturnHighestScoringWordPositional(priorityLetters, x, y));
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
                // count the number of times each letter appears in the word list
                foreach (var letter in word.Distinct().ToList())
                {
                    _letterCount[letter]++;
                }
            }
        }

        private void CalculateLetterCountPositional()
        {
            _letterCountPositional = new List<Dictionary<char, int>>();
            // initialise the dictionary for each position in the word list
            for (var i = 0; i < 5; i++) 
            {
                _letterCountPositional.Add(new Dictionary<char, int>());
                // initialise the dictionary for each letter in the alphabet
                foreach (var letter in Globals.Alphabet) 
                {
                    _letterCountPositional[i].Add(letter, 0);
                }
            }

            foreach (var word in _words)
            {
                // count the number of times each letter appears in each position in the word list
                for (var i = 0; i < word.Length; i++) 
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
            // score each word in the word list by adding the number of times each letter appears in the word list
            // this is the basis for the minimax algorithm
            foreach (var word in _words)
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
                    continue;
                }
                
                // if the letter appears more than once in the word, only use the highest score for that letter
                // reduces the chance of the algorithm picking a word that has a letter that appears more than once
                score[letter] = Math.Max(score[letter], _letterCountPositional[i][letter]);
            }

            // add the scores for each letter in the word to get the total score for the word
            _wordScoresPositional[word] = score.Values.Sum(); 
        }

        private void CalculateWordScoresPositional()
        {
            CalculateLetterCountPositional();
            _wordScoresPositional = new Dictionary<string, int>();
            // score each word in the word list by adding the number of times each letter appears in that position
            // this is the basis for the positional minimax algorithm
            foreach (var word in _words)
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
                if (forbiddenLetters.Contains(word[i]))
                {
                    return true;
                }
            }

            return false;
        }
        
        private static bool CheckWordAgainstAllowedMask(string word, Mask allowedMask)
        {
            // check if the word contains any letters that are not allowed in the allowed mask
            foreach (var letter in Globals.Alphabet)
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
            // remove any letters from the allowed mask that are not in the remaining words
            foreach (var letterCountPair in _letterCount)
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